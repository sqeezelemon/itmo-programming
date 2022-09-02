#include <stdio.h>
#include <stdint.h>
#include <math.h>
#include <string.h>
#include <stdlib.h>

// ASCII formatting stuff
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

// FROM HERE:
// http://www.daubnet.com/en/file-format-bmp

typedef struct {
    int8_t   type[2];
    uint32_t size;
    uint32_t reserved;
    uint32_t startAddress;
} header;

typedef struct {
    uint32_t headerSize;
    int32_t  width;
    int32_t  height;
    uint16_t planes;
    uint16_t bitsPerPixel;
    uint32_t compression;
    uint32_t imageSize;

    // Compression
    uint32_t xPixelsPerM;
    uint32_t yPixelsPerM;
    uint32_t colorsUsed;
    uint32_t colorsImportant;
} infoHeader;

uint32_t reverse(uint32_t x) {
    x = (x >> 24) | (((x >> 16) & 0xFF) << 8) | (((x >> 8) & 0xFF) << 16) | ((x & 0xFF) << 24);
    return x; 
}

int main(int argc, char **argv) {
    if (argc == 2 && strcmp(argv[1],"--help")*strcmp(argv[1],"-h") == 0) {
        printf(
BOLD "COMMANDS:" ENDBOLD "\n\
--input [FILE]       \tLocation of the input image file.\n\
--output [DIR]       \tDirectory where the output files would be situated.\n\
--invert             \tInverts the values in initial input.\n\
--alpha              \tMakes the program use the alpha value (32bpp bmp only) for tipping_point calculation\n\
--max_iter           \tThe maximum amount of iterations that would be simulated.\n\
--dump_freq [INT]    \tHow often should the result be dumped into a file\n\
--tipping_point [INT]\tAfter what RGB median from inital input shall the cell (pixel) be alive.\n\
--help/-h            \tDisplay this menu.\n"
        );
        return 0;
    }

    int tippingPoint = 255;
    uint64_t maxIter = 1;
    int inputIndex = 0;
    int outputIndex = -1;
    int dumpFreq = 1;
    int invert = 0;
    int alpha = 0;


    
    //////// Parse arguments
    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "--max_iter") == 0) {
            if (i < argc - 1) {
                maxIter = atoi(argv[i+1]);
                if (maxIter < 1) {
                    maxIter = 1;
                    printf(MAG BOLD "WARNING:" ENDBOLD RESET " %llu is not a valid iteration count. max_iteration parameter was setted to the closest acceptable value (1).\n", maxIter);
                }
                i++;
            } else {
                printf(MAG BOLD "WARNING:" ENDBOLD RESET " --max_iter wasn't followed by a value, setting to infinity.\n");
            }
        } else if (strcmp(argv[i], "--input") == 0) {
            if (i < argc - 1) {
                inputIndex = i+1;
                i++;
            } else {
                printf(RED BOLD "ERROR:" ENDBOLD RESET " no input was provided.\n");
                return 1;
            }
        } else if (strcmp(argv[i], "--output") == 0) {
            if (i < argc - 1) {
                outputIndex = i+1;
                i++;
            }
        } else if (strcmp(argv[i], "--dump_freq") == 0) {
            if (i < argc - 1) {
                dumpFreq = atoi(argv[i+1]);
                if (dumpFreq < 1) {
                    dumpFreq = 1;
                    printf(MAG BOLD "WARNING:" ENDBOLD RESET " %d is not a valid dump frequency. dump_frequency parameter was setted to the closest acceptable value (1).\n", dumpFreq);
                }
                i++;
            } else {
                dumpFreq = 1;
                printf(MAG BOLD "WARNING:" ENDBOLD RESET " --dump_freq wasn't followed by a value, defaulting to 1.\n");
            }
        } else if (strcmp(argv[i], "--tipping_point") == 0) {
            if (i < argc - 1) {
                tippingPoint = atoi(argv[i+1]);
                if (tippingPoint < 0) {
                    tippingPoint = 0;
                    printf(MAG BOLD "WARNING:" ENDBOLD RESET " %d is not in range(0-255). tipping_point parameter was setted to the closest acceptable value (0).\n", tippingPoint);
                } else if (tippingPoint > 255) {
                    tippingPoint = 255;
                    printf(MAG BOLD "WARNING:" ENDBOLD RESET " %d is not in range(0-255). tipping_point parameter was setted to the closest acceptable value (255).\n", tippingPoint);
                }
                i++;
            } else {
                printf(MAG BOLD "WARNING:" ENDBOLD RESET " --tipping_point wasn't followed by a value, defaulting to 255.\n");
            }
        } else if (strcmp(argv[i], "--invert") == 0) {
            invert = 1;
        } else if (strcmp(argv[i], "--alpha") == 0) {
            alpha = 1;
        }
        
    }

    if (outputIndex == -1) {
        printf(RED BOLD "WARNING:" ENDBOLD RESET " no output directory was provided, outputting into the current directory.\n");
    }
    
    //////// Open file
    FILE* input = fopen(argv[inputIndex], "rb");
    if (input == NULL || inputIndex == 0) {
        printf(RED BOLD "ERROR:" ENDBOLD RESET " File not found\n");
        return 1;
    }

    
    //////// Read header
    header hdr;
    fread(&hdr.type,         sizeof(uint8_t ), 2, input);
    fread(&hdr.size,         sizeof(uint32_t), 1, input);
    fread(&hdr.reserved,     sizeof(uint32_t), 1, input);
    fread(&hdr.startAddress, sizeof(uint32_t), 1, input);
    if (hdr.type[0] != 'B' || hdr.type[1] != 'M') {
        printf(RED BOLD "ERROR:" ENDBOLD RESET " Bitmap header of type %c%c is not supported. Please use a BM type header.\n", hdr.type[0], hdr.type[1]);
        return 1;
    }
    
    //////// Read DIB header
    infoHeader info;
    fread(&info.headerSize,      sizeof(uint32_t), 1, input);
    fread(&info.width,           sizeof(uint32_t), 1, input);
    fread(&info.height,          sizeof(uint32_t), 1, input);
    fread(&info.planes,          sizeof(uint16_t), 1, input);
    fread(&info.bitsPerPixel,    sizeof(uint16_t), 1, input);
    fread(&info.compression,     sizeof(uint32_t), 1, input);
    fread(&info.imageSize,       sizeof(uint32_t), 1, input);
    fread(&info.yPixelsPerM,     sizeof(uint32_t), 1, input);
    fread(&info.xPixelsPerM,     sizeof(uint32_t), 1, input);
    fread(&info.colorsUsed,      sizeof(uint32_t), 1, input);
    fread(&info.colorsImportant, sizeof(uint32_t), 1, input);
    info.width = abs(info.width);
    info.height = abs(info.height);
    if (hdr.startAddress > 54) {
        uint8_t trash;
        for (int i = 0; i < hdr.startAddress - 54; i++) {
            fread(&trash, sizeof(uint8_t), 1, input);
        }
    }

    //////// Read file into the game array
    const int byteWidth = (info.width + 31) / 32;
    uint32_t game[info.height][byteWidth];
    // Compression is used
    if (info.compression != 0) {
        printf(MAG BOLD "WARNING:" ENDBOLD RESET " Compressed BMP files are not supported. Result may be unexpected.\n");
        //return 1;
    }
    if (info.bitsPerPixel == 1) {
            for (int h = 0; h < info.height; h++) {
                for (int w = 0; w < byteWidth; w++) {
                    fread(&(game[h][w]), sizeof(uint32_t), 1, input);
                }
            }
    } else if (info.bitsPerPixel == 24) {
        uint8_t r,g,b;
        int bitIndex, byteIndex;
        for (int h = 0; h < info.height; h++) {
            for (int w = 0; w < info.width; w++) {

                fread(&b, sizeof(uint8_t), 1, input);
                fread(&g, sizeof(uint8_t), 1, input);
                fread(&r, sizeof(uint8_t), 1, input);
                byteIndex = w / 32;
                bitIndex  = w % 32;
                if (bitIndex == 0) {
                    game[h][byteIndex] = 0;
                }
                if (invert) {
                    r = 255 - r;
                    g = 255 - g;
                    b = 255 - b;
                }
                game[h][byteIndex] = game[h][byteIndex] | ((((r + b + g) / 3 >= tippingPoint) ? 1 : 0) << (31-bitIndex));
            }
        }
    } else if (info.bitsPerPixel == 32) {
        uint8_t r,g,b,a;
        int bitIndex, byteIndex;
        for (int h = 0; h < info.height; h++) {
            for (int w = 0; w < info.width; w++) {

                fread(&b, sizeof(uint8_t), 1, input);
                fread(&g, sizeof(uint8_t), 1, input);
                fread(&r, sizeof(uint8_t), 1, input);
                fread(&a, sizeof(uint8_t), 1, input);
                byteIndex = w / 32;
                bitIndex  = w % 32;
                if (bitIndex == 0) {
                    game[h][byteIndex] = 0;
                }
                if (invert) {
                    r = 255 - r;
                    g = 255 - g;
                    b = 255 - b;
                    a = 255 - a;
                }
                if (alpha) {
                    game[h][byteIndex] = game[h][byteIndex] | ((((r + b + g + a) / 4 >= tippingPoint) ? 1 : 0) << (31-bitIndex));
                } else {
                    game[h][byteIndex] = game[h][byteIndex] | ((((r + b + g) / 3 >= tippingPoint) ? 1 : 0) << (31-bitIndex));
                }
                
            }
        }
    } else {
        printf(RED BOLD "ERROR:" ENDBOLD RESET " %d bit BMP files are not supported\n", info.bitsPerPixel);
        return 1;
    }
    fclose(input);



    //////// Game
    uint32_t temp[info.height][byteWidth];
    int neighbours, bitIndex, byteIndex;
    char outputPath[4096];

    for (int iter = 0; iter < maxIter; iter++) {
        // Play round
        for (int h = 0; h < info.height; h++) {
            for (int w = 0; w < info.width; w++) {
                neighbours = 0;
                byteIndex = w / 32;
                bitIndex  = w % 32;
                if (bitIndex == 0) {
                    temp[h][byteIndex] = 0;
                }
                // Left side
                if (w > 0) {
                    // Left Center
                    neighbours += (game[h][(w-1)/32] & (0x80000000 >> ((w-1)%32))) ? 1 : 0;
                    // Left Top
                    if (h > 0) {
                        neighbours += (game[h-1][(w-1)/32] & (0x80000000 >> ((w-1)%32))) ? 1 : 0;
                    }
                    // Left Bottom
                    if (h < info.height-1) {
                        neighbours += (game[h+1][(w-1)/32] & (0x80000000 >> ((w-1)%32))) ? 1 : 0;
                    }
                }
                // Right side
                if (w < info.width-1) {
                    // Right Center
                    neighbours += (game[h][(w+1)/32] & (0x80000000 >> ((w+1)%32))) ? 1 : 0;
                    // Right Top
                    if (h > 0) {
                        neighbours += (game[h-1][(w+1)/32] & (0x80000000 >> ((w+1)%32))) ? 1 : 0;
                    }
                    // Right Bottom
                    if (h < info.height-1) {
                        neighbours += (game[h+1][(w+1)/32] & (0x80000000 >> ((w+1)%32))) ? 1 : 0;
                    }
                }
                // Top
                if (h > 0) {
                    neighbours += (game[h-1][byteIndex] & (0x80000000 >> bitIndex)) ? 1 : 0;
                }
                // Bottom
                if (h < info.height-1) {
                    neighbours += (game[h+1][byteIndex] & (0x80000000 >> bitIndex)) ? 1 : 0;
                }
                if (
                    ((neighbours == 2 || neighbours == 3) && (game[h][byteIndex] & (0x80000000 >> bitIndex)))
                    || (neighbours == 3)) {
                        temp[h][byteIndex] = temp[h][byteIndex] | (0x80000000 >> bitIndex);
                }
            }
        }

        if ((iter+1) % dumpFreq == 0 || iter == maxIter-1) {
            memset(outputPath, '\0', sizeof(outputPath));
            if (outputIndex > 0) {
                strcpy(outputPath, argv[argc-1]);
            } else {
                outputPath[0] = '/';
            }
            char filename[30];
            sprintf(outputPath, "gen-%d.bmp", iter+1);

            FILE* output = fopen(outputPath, "wb");
            if (output == NULL) {
                printf(RED BOLD "ERROR:" ENDBOLD RESET " Unable to create output file for generation %d\n", (iter+1));
                return 1;
            }

            //////// BM Header
            // BM
            fwrite(hdr.type, sizeof(uint8_t), 2, output);
            // File size
            uint32_t temp32 = sizeof(game) + 54+8;
            fwrite(&temp32, sizeof(uint32_t), 1, output);
            // Unused
            temp32 = 0;
            fwrite(&temp32, sizeof(uint32_t), 1, output);
            // Header size
            temp32 = 54+8;
            fwrite(&temp32, sizeof(uint32_t), 1, output);

            //////// DIB Header
            // DIB Header size
            temp32 = 40;
            fwrite(&temp32, sizeof(uint32_t), 1, output);
            // Dimensions
            fwrite(&info.width, sizeof(uint32_t), 1, output);
            fwrite(&info.height, sizeof(uint32_t), 1, output);
            // Color plane
            uint16_t temp16 = 1;
            fwrite(&temp16, sizeof(uint16_t), 1, output);
            // Palette
            fwrite(&temp16, sizeof(uint16_t), 1, output);
            
            // No compression
            temp32 = 0;
            fwrite(&temp32, sizeof(uint32_t), 1, output);
            // Image size
            temp32 = sizeof(game);
            fwrite(&temp32, sizeof(uint32_t),1, output);
            // Print dimensions
            fwrite(&info.xPixelsPerM, sizeof(uint32_t), 1, output);
            fwrite(&info.yPixelsPerM, sizeof(uint32_t), 1, output);
            temp32 = 0;
            fwrite(&temp32, sizeof(uint32_t), 1, output);
            fwrite(&temp32, sizeof(uint32_t), 1, output);
            // Palette
            temp32 = 0;
            fwrite(&temp32, sizeof(uint32_t), 1, output);
            temp32 = 0x00FFFFFF;
            fwrite(&temp32, sizeof(uint32_t), 1, output);
            //fwrite(temp, sizeof(game), 1, output);
            uint32_t field;
            for (int h = info.height-1; h > -1; h--) {
                for (int w = 0; w < byteWidth; w++) {
                    field = reverse(temp[h][w]);
                    fwrite(&field, sizeof(uint32_t), 1, output);
                }
            }
            fclose(output);
            printf(BOLD GRN "SUCCESS:" RESET ENDBOLD " Written generation %d\tto path: %s\n", iter+1, outputPath);
        }

        for (int h = 0; h < info.height; h++) {
            for (int w = 0; w < byteWidth; w++) {
                game[h][w] = temp[h][w];
            }
        }
    }
}