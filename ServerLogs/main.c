#include <stdio.h>
#include <time.h>
#include <string.h>
#include <stdint.h>
#include <stdlib.h> // For atoi (implicit declaration of function 'atoi' is invalid in C99)

#define ERROR_CODE_LIMIT 12
#define INTERVALMAX 0b00000001
#define PRINTERRORS 0b00000010
#define ONLYPRINT 0b00000100

#define RED "\x1B[31m"
#define GRN "\x1B[32m"
#define YEL "\x1B[33m"
#define BLU "\x1B[34m"
#define MAG "\x1B[35m"
#define CYN "\x1B[36m"
#define WHT "\x1B[37m"
#define RESET "\x1B[0m"
#define BOLD "\e[1m"
#define ENDBOLD "\e[0m"

int main(int argc, char **argv) {
    // See if just needs help
    if (argc == 2)
    {
        if (strcmp(argv[1], "--help") == 0 || strcmp(argv[1], "-h") == 0)
        {
            printf("COMMANDS:\n\
            --intervalMax [SECONDS] \t Searches for the interval of set size with the maximum amount of errors\n\
            --printErrors \t Prints all lines with 5XX error codes\n\
            --printOnly [CODE] \t Prints all lines with a certain 5XX error code\n\
            --help / -h \t Shows the help menu\n");
            printf("EXAMPLE: --intervalMax 10000 /files/access_log_95.txt\n");
            return 0;
        }
    }

    // Check if file is openable
    FILE *file = fopen(argv[argc - 1], "r");
    if (file == NULL)
    {
        printf("\a");
        printf(RED BOLD "ERROR:" ENDBOLD RESET " file path isn't valid: %s\n", argv[argc - 1]);
        return 1;
    }

    // If openable, check parameters
    uint8_t flags = 0b00000000;
    char domainFilter[256] = "";
    int errorCodeFilter = 0;
    char requestFilter[32] = "";

    int statsTimeInterval = 0;

    for (int i = 1; i < argc - 1; i++) {
        if (strcmp(argv[i], "--intervalMax") == 0)
        {
            i++;
            statsTimeInterval = atoi(argv[i]);
            if (statsTimeInterval <= 0) {
                printf(MAG BOLD "WARNING:" ENDBOLD RESET " --intervalMax value at index %d isn't a positive integer: %s\n", i + 1, argv[i]);
            }
            flags = flags | INTERVALMAX;
        }

        else if (strcmp(argv[i], "--printErrors") == 0)
        {
            flags = flags | PRINTERRORS;
        }

        else if (strcmp(argv[i], "--printOnly") == 0)
        {
            i++;
            errorCodeFilter = atoi(argv[i]);
            if (errorCodeFilter < 500 || errorCodeFilter >= ERROR_CODE_LIMIT + 500)
            {
                printf(MAG BOLD "WARNING:" ENDBOLD RESET " --errorCodeFilter value at index %d ( %s ) isn't a valid 5XX error code, nothing would be printed.\n", i + 1, argv[i]);
                flags = flags && !ONLYPRINT;
                return 2;
            }
            else
            {
                flags = flags | ONLYPRINT;
            }
        }

        else
        {
            printf(MAG BOLD "WARNING:" ENDBOLD RESET " unknown argument at index %d: %s\nPlease use --help or -help to see all available commands.\n", i + 1, argv[i]);
        }
    }


    char lineBuffer[1024];
    uint_fast16_t lineIndex = 0;
    struct tm lineTime;
    unsigned long lineTimeStamp;
    int errorCodeStats[ERROR_CODE_LIMIT]; // 500-526
    memset(errorCodeStats, 0, ERROR_CODE_LIMIT * sizeof(int));

    int safeInterval = (statsTimeInterval > 0) ? statsTimeInterval : 1;
    uint8_t errorsBySecond[statsTimeInterval];
    memset(errorsBySecond, 0, statsTimeInterval * sizeof(uint8_t));
    int errorsBySecondSum = 0;
    int errorsBySecondMax = -1;
    unsigned long errorsBySecondMaxTimestamp = 0;
    int prevLineErrosBySecondIndex = -1;

    while (fgets(lineBuffer, 1024, file)) {

        lineIndex = 0;
        // ADDRESS - - [dd/mmm/yyyy:hh:mm:ss -0400] "METHOD PATH PROTOCOL CODE BYTES"
        // 199.72.81.55 - - [01/Jul/1995:00:00:01 -0400] "GET /history/apollo/ HTTP/1.0" 200 6245
        while (lineBuffer[lineIndex] != '[' && lineBuffer[lineIndex] != '\0') lineIndex++;
        // TIME
        // Day
        lineTime.tm_mday = (lineBuffer[lineIndex+1] - '0') * 10 + (lineBuffer[lineIndex + 2] - '0');
        lineIndex += 3;
        // Month
        char months[12][4] = {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };
        char monthBuffer[4] = {
            lineBuffer[lineIndex+1], lineBuffer[lineIndex+2], lineBuffer[lineIndex+3], '\0'
        };
        for (int i = 0; i < 12; i++) {
            if (strcmp(months[i], monthBuffer) == 0) {
                lineTime.tm_mon = i;
                break;
            }
        }
        lineIndex += 4;
        // Year
        lineTime.tm_year = (lineBuffer[lineIndex + 1] - '0') * 1000
        + (lineBuffer[lineIndex + 2] - '0') * 100 + (lineBuffer[lineIndex + 3] - '0') * 10 + (lineBuffer[lineIndex + 4] - '0') -1900;
        lineIndex += 5;
        // Hour
        lineTime.tm_hour = (lineBuffer[lineIndex + 1] - '0') * 10 + (lineBuffer[lineIndex + 2] - '0');
        lineIndex += 3;
        // Minutes
        lineTime.tm_min = (lineBuffer[lineIndex + 1] - '0') * 10 + (lineBuffer[lineIndex + 2] - '0');
        lineIndex += 3;
        // Seconds
        lineTime.tm_sec = (lineBuffer[lineIndex + 1] - '0') * 10 + (lineBuffer[lineIndex + 2] - '0');
        lineIndex += 4;

        lineTimeStamp = mktime(&lineTime);
        // Time Zone
        int lineTimeZone = ((lineBuffer[lineIndex + 1] - '0') * 10 + (lineBuffer[lineIndex + 2] - '0')) * 3600 + ((lineBuffer[lineIndex + 3] - '0') * 10 + (lineBuffer[lineIndex + 4] - '0')) * 60;
        if (lineBuffer[lineIndex] == '+') {
            lineTimeStamp += lineTimeZone;
        } else if (lineBuffer[lineIndex] == '-') {
            lineTimeStamp -= lineTimeZone;
        }

        // GET TO ERROR CODE
        int doubleQuoteCounter = 0;
        while (doubleQuoteCounter != 2 && lineIndex < 1019) {
            doubleQuoteCounter += (lineBuffer[lineIndex] == '"');
            lineIndex++;
        }
        lineIndex += 1;

        //INTERVAL AND ERROR CODE
        int index = lineTimeStamp % statsTimeInterval;
        if (prevLineErrosBySecondIndex != index) {
            errorsBySecondSum -= errorsBySecond[index];
            errorsBySecond[index] = 0;
        }

        int lineErrorCode = (lineBuffer[lineIndex] - '0') * 100
        + (lineBuffer[lineIndex+1] - '0') * 10 + (lineBuffer[lineIndex+2] - '0');
        if (lineErrorCode >= 500 && lineErrorCode <= 500 + ERROR_CODE_LIMIT) {
            errorCodeStats[lineErrorCode-500] += 1;
            if (flags & PRINTERRORS)
            {
                printf("%s", lineBuffer);
            }
            else if (((flags & ONLYPRINT) != 0) && (lineErrorCode == errorCodeFilter))
            {
                printf("%s", lineBuffer);
            }
            
            // Period with most errors
            if (statsTimeInterval <= 0) continue;

            prevLineErrosBySecondIndex = index;


            errorsBySecond[index] += 1;
            errorsBySecondSum += 1;
            if (errorsBySecondSum > errorsBySecondMax) {
                errorsBySecondMax = errorsBySecondSum;
                errorsBySecondMaxTimestamp = lineTimeStamp;
            }
        }
    }

    const char errorDescriptions [ERROR_CODE_LIMIT][200] = {
        "Internal Server Error",
        "Not Implemented",
        "Bad Gateway",
        "Service Unavailable",
        "Gateway Timeout",
        "HTTP Version Not Supported",
        "Variant Also Negotiates",
        "Insufficient Storage",
        "Loop Detected",
        "Bandwidth Limit Exceeded",
        "Not Extended",
        "Network Authentication Required"
    };
    printf("CODE\tCOUNT\tDESCRIPTION\n");
    for (int i = 0; i < ERROR_CODE_LIMIT; i++) {
        if (errorCodeStats[i] > 0)
        printf("%d\t%d\t%s\n", i+500, errorCodeStats[i], errorDescriptions[i]);
    }
    if (statsTimeInterval <= 0) return 0;
    char intervalStart[30];
    char intervalEnd[30];
    struct tm * timeBuffer;
    timeBuffer = gmtime( &errorsBySecondMaxTimestamp);
    strftime(intervalEnd, 30, "%Y-%m-%d %H:%M:%S", timeBuffer);
    errorsBySecondMaxTimestamp -= statsTimeInterval;
    timeBuffer = gmtime( &errorsBySecondMaxTimestamp);
    strftime(intervalStart, 30, "%Y-%m-%d %H:%M:%S", timeBuffer);
    printf("MAX ERROS WITHIN %d SECONDS (GMT):\n", statsTimeInterval);
    printf("%s - %s\n", intervalStart, intervalEnd);
}