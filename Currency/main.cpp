#define DEBUG 1

// Collections
#include <vector>
#include <set>

// Other stdlib stuff
#include <string>
#include <iostream>
#include <csignal> // For the signal() function

// Third party
#include <nlohmann/json.hpp>
#include <curl/curl.h>

// Cross-platform sleep
#include <thread>
#include <chrono>

// Debug/showcase
# if (DEBUG)
#include <cstdlib> // for rand()
#endif

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

#define ERROR "[" RED BOLD "ERROR" ENDBOLD RESET "] "
#define WARNING "[" MAG BOLD "WARNING" ENDBOLD RESET "] "


const char* apiUrl = "http://www.cbr-xml-daily.ru/daily_json.js";
static std::string response;

CURL* curl;
CURLcode curlCode;
nlohmann::json json;
std::vector<std::string> currencyList;

// 0 - OK, 1 - ERROR
int fetch() {
    curlCode = curl_easy_perform(curl);
    if (curlCode != CURLE_OK) return 1;
    json = nlohmann::json::parse(response);
    response.clear();
    if (json.find("Valute") == json.end()) {
        fprintf(stderr, ERROR "Valute key not present\n");
        return 1;
    }
    json = json["Valute"];
    // DEBUG / SHOWCASE ONLY
    #if (DEBUG)
    for (auto curr : currencyList) {
        if (!json.contains(curr)) continue;
        double rnd = (double)(rand() % 100000) / (double)10000 * (double)(rand() % 2 == 1 ? -1 : 1);
        json[curr]["Value"] = abs((double)json[curr]["Value"] + rnd);
        
    }
    #endif
    return 0;
}

// All the analysis stuff
struct currencyCounter {
    uint32_t value;
    uint32_t count;

    // For std::set
    friend inline bool operator< (currencyCounter l, currencyCounter r) {
        return l.value < r.value;
    }
};
std::vector< std::set<currencyCounter> > currency;
void analyse() {
    for (int i = 0; i < currencyList.size(); i++) {
        uint32_t rate = (double)json[currencyList[i]]["Value"] * 10000;
        currencyCounter temp { rate, 1 };
        auto searchRes = currency[i].find(temp);
        if (searchRes != currency[i].end()) {
            temp = *searchRes;
            temp.count++;
            currency[i].erase(temp);
        }
        currency[i].insert(temp);
    }
}

// Clears output using ANSI code magic
void clearLines(int amm) {
    for (int i = 0; i < amm; i++) {
        std::cout << "\u001b[1000D" // Move cursor left (CSI1000D)
        << "\u001b[2K"              // Clear whole line (CSI2K)
        << "\u001b[1A";             // Move cursor up   (CSI1A)
    }
    std::cout.flush();
}

// Executed when the program is terminated
void finish(int signum) {
    if (currency[0].empty()) {
        printf("Программа объявила дефолт!\n");
        exit(0);
    }
    clearLines(currencyList.size());
    std::cout << "\u001b[1000D" << "\u001b[2K"; // Remove line without going up to the line up (hense, why clearlines isn't used)
    std::cout.flush();
    curl_easy_cleanup(curl);
    curl_global_cleanup();

    printf(BOLD "CURR\tMIN\tMAX\tNMNL\tAVG\tMED\n" ENDBOLD);
    for (int i = 0; i < currencyList.size(); i++) {
        double min = (double)(*currency[i].begin()).value / 10000;
        double max = (double)(*currency[i].rbegin()).value / 10000;
        int nmnl = json[currencyList[i]]["Nominal"];
        uint64_t avgint = 0;
        uint64_t iterCount = 0;
        for (auto rate : currency[i]) {
            avgint += (uint64_t)rate.value * (uint64_t)rate.count;
            iterCount += rate.count;
        }
        double avg = (double)avgint / (double)(10000*iterCount);
        double med = 0;
        uint64_t currpos = 0;
        for (auto it = currency[i].begin(); it != currency[i].end(); it++) {
            currpos += it->count;
            if (currpos < iterCount/2) {
                continue;
            } else if (currpos == iterCount/2 && iterCount % 2 == 0) {
                med = (double)it->value / (double)10000;
                it++;
                med = (med + ((double)it->value / (double)10000))/2;
                break;
            } else if (currpos == iterCount/2) {
                it++;
                med = (double)it->value / (double)10000;
                break;
            } else {
                med = (double)it->value / (double)10000;
                break;
            }
        }

        std::cout << BOLD << currencyList[i] << ENDBOLD << '\t'
        << min << '\t' << max << '\t'
        << (nmnl > 1000 ? nmnl/1000 : nmnl) << (nmnl > 1000 ? "k\t" : "\t")
        << avg << '\t' << med << '\n';
    }

    exit(0);
}

// Used by CURL after data is received
size_t writeCallback(char *data, size_t size, size_t nitems, std::string *out) {
    if (out == NULL) return 0;
    out->append(data, size*nitems);
    return size*nitems;
}

// Output the table
void output() {
    for (auto curr : currencyList) {
        double rate = json[curr]["Value"];
        double prev = json[curr]["Previous"];
        double nominal = json[curr]["Nominal"];
        std::cout << '\n' << BOLD << curr << ENDBOLD
        << (rate == prev ? YEL : (rate > prev ? GRN : RED))
        << (rate == prev ? " ~" : (rate > prev ? " ▲" : " ▼")) << '\t' << rate << RESET << '\t'
        << prev << '\t' << (nominal > 1000 ? nominal/1000 : nominal) << (nominal > 1000 ? "k\t" : "\t")
        << (std::string)json[curr]["Name"];
    }
    std::cout.flush();
}

int main(int argc, char** argv) {
    signal(SIGINT, finish);

    std::chrono::seconds updateInterval(10);
    for (int i = 1; i < argc; i++) {
        if ((strlen(argv[i]) == 3) && (strstr("-", argv[i]) == NULL)) {
            std::string temp(argv[i]);
            bool duplicate = false;
            for (auto curr : currencyList) {
                if (curr == temp) {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate) currencyList.push_back(temp);
        } else if ((strstr("--interval", argv[i]) != argv[i] || strstr("-i", argv[i]) != argv[i]) && i < argc-1) {
            std::chrono::seconds temp(atoi(argv[i+1]));
            updateInterval = temp;
            i++;
        }
    }

    // CURL setup
    curl_global_init(CURL_GLOBAL_DEFAULT);
    curl = curl_easy_init();
    curl_easy_setopt(curl, CURLOPT_URL, apiUrl);
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, writeCallback);
    curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);
    curl_easy_setopt(curl, CURLOPT_TIMEOUT, 1);

    // First fetch: get info
    if (fetch()) {
        fprintf(stderr, ERROR "First fetch failed, terminating\n");
        return 1;
    }

    // Check if currency exists
    for (int i = 0; i < currencyList.size(); i++) {
        if (json.contains(currencyList[i])) continue;
        fprintf(stderr, WARNING "Currency " BOLD "%s" ENDBOLD " not found!\n", currencyList[i].c_str());
        currencyList.erase(std::next(currencyList.begin(), i));
    }

    // Add all available currencies if none were requested
    if (currencyList.size() == 0) {
        for (auto it = json.begin(); it != json.end(); it++) {
            currencyList.push_back(it.key());
        }
    }
    currency.resize(currencyList.size());
    printf(BOLD "CURR\tRATE\tPREV\tNMNL\tNAME" ENDBOLD);
    output();
    analyse();

    while (true) {
        std::this_thread::sleep_for(updateInterval);
        if (fetch()) {
            continue;
        }
        clearLines(currencyList.size());
        analyse();
        output();
    }
}