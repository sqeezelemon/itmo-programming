#include <stdio.h>
#include <stdint.h>
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

//////// Synchsafe stuff
uint32_t fromSynchsafe(uint32_t x) {
    return (
        (x >> 24) | (((x >> 16) & 0x000000FF) << 7) | (((x >> 8) & 0x000000FF) << 14) | ((x & 0x000000FF) << 21)
    );
}

uint32_t toSynchsafe(uint32_t x) {
    return (
        ((x & 0b01111111) << 24) | (((x>>7) & 0b01111111) << 16) | (((x>>14) & 0b01111111) << 8) | ((x>>21) & 0b01111111)
    );
}

uint32_t reverseByteorder(uint32_t x) {
    return (
        (x >> 24) | (((x >> 16) & 0x000000FF) << 8) | (((x >> 8) & 0x000000FF) << 16) | ((x & 0x000000FF) << 24)
    );
}

//////// Headers
typedef struct header {
    char     fileId[3];
    uint8_t  majorVersion;
    uint8_t  minorVersion;
    uint8_t  flags;
    uint32_t tagSize;
} header;

header readHeader(FILE* song) {
    header temp;
    fread(temp.fileId, sizeof(char), 3, song);
    fread(&temp.majorVersion, sizeof(uint8_t), 1, song);
    fread(&temp.minorVersion, sizeof(uint8_t), 1, song);
    fread(&temp.flags, sizeof(uint8_t), 1, song);
    fread(&temp.tagSize, sizeof(uint32_t), 1, song);
    temp.tagSize = fromSynchsafe(temp.tagSize);
    return temp;
}

int skipExtendedHeader(FILE* song) {
    uint32_t size;
    fread(&size, sizeof(uint32_t), 1, song);
    size = fromSynchsafe(size);
    fseek(song, size-4, SEEK_CUR);
    return size;
}

void writeHeader(FILE* song, header header) {
    fwrite(header.fileId, sizeof(char), 3, song);
    fwrite(&header.majorVersion, sizeof(uint8_t), 1, song);
    fwrite(&header.minorVersion, sizeof(uint8_t), 1, song);
    fwrite(&header.flags, sizeof(uint8_t), 1, song);
    header.tagSize = toSynchsafe(header.tagSize);
    fwrite(&header.tagSize, sizeof(uint32_t), 1, song);
}

//////// Frame stuff
typedef struct frame {
    char     tagId[4];
    uint32_t tagSize;
    uint8_t  flags[2];
    uint8_t* data;
} frame;

frame readFrame(FILE* song) {
    frame temp;
    fread(temp.tagId, sizeof(char), 4, song);
    fread(&temp.tagSize, sizeof(char), 4, song);
    fread(temp.flags, sizeof(char), 2, song);

    temp.tagSize = reverseByteorder(temp.tagSize);
    temp.data = (uint8_t*)malloc(sizeof(uint8_t)*temp.tagSize);
    fread(temp.data, sizeof(uint8_t), temp.tagSize, song);
    return temp;
}

void writeFrame(FILE* song, frame* frm) {
    fwrite(frm->tagId, sizeof(char), 4, song);
    uint32_t temp = reverseByteorder(frm->tagSize);
    fwrite(&temp, sizeof(uint32_t), 1, song);
    fwrite(frm->flags, sizeof(uint8_t), 2, song);
    fwrite(frm->data, sizeof(uint8_t), frm->tagSize, song);
    free(frm);
}

//////// Linked list for encoding

typedef struct llnode llnode;
struct llnode {
    frame value;
    llnode* next;
};

typedef struct {
    llnode* head;
} linkedlist;

llnode* llpush(linkedlist* ll, frame frm) {
    llnode* newNode = (llnode*)malloc(sizeof(llnode));
    newNode->value = frm;
    newNode->next = NULL;

    if (ll->head == NULL) {
        ll->head = newNode;
        return newNode;
    }
    llnode* curr = ll->head;
    while (curr->next != NULL) {
        curr = curr->next;
    }
    curr->next = newNode;
    return newNode;
}

frame llpop(linkedlist* ll) {
    llnode* tempNode = ll->head;
    ll->head = ll->head->next;
    frame tempFrame = tempNode->value;
    free(tempNode);
    return tempFrame;
}

int llsize(linkedlist* ll) {
    int counter = 0;
    llnode* curr = ll->head;
    while (curr != NULL) {
        counter += curr->value.tagSize + 10;
        curr = curr->next;
    }
    return counter;
}

llnode* llsearch(linkedlist* ll, char* tag) {
    llnode* curr = ll->head;
    while (curr != NULL) {
        if (
            curr->value.tagId[0] == tag[0] && curr->value.tagId[1] == tag[1] &&
            curr->value.tagId[2] == tag[2] && curr->value.tagId[3] == tag[3]
        ) {
            break;
        }
        curr = curr->next;
    }
    return curr;
}

void llremove(linkedlist* ll, llnode* nodeToRemove) {
    llnode* curr = ll->head;
    if (curr == NULL || nodeToRemove == NULL) return;
    else if (curr == nodeToRemove) {
        ll->head = curr->next;
        return;
    }

    while (curr != NULL) {
        if (curr->next == nodeToRemove) {
            curr->next = nodeToRemove->next;
            free(nodeToRemove->value.data);
            free(nodeToRemove);
            break;
        }
        curr = curr->next;
    }
}


//////// RUNTIME STUFF

typedef enum {
    UNKNOWN,
    SET,
    LIST,
    GET,
    GETPIC,
    SETPIC,
    REMOVE
} programMode;

int main(int argc, char **argv) {
    if (argc == 2) {
        if (strcmp(argv[1], "--help") * strcmp(argv[1], "-h") == 0) {
            printf(BOLD "COMMANDS" ENDBOLD "\n\
--filepath=[FILE]           Sets the path to the file\n\
--show                      Prints the values of all tags\n\
--set=[TAG] --value=[VALUE] Sets the tag to chosen value\n\
--get=[TAG]                 Prints the value of the selected tag\n\
--remove=[TAG]              Removes the selected tag from the file\n\
--getPic=[FILENAME]         Outputs the artwork into a file with the correct extension\n\
--setPic=[FILE]             Sets the artwork to the contents of the file\n\
--help / -h                 Displays this help dialog\n"
            );
            return 0;
        }
    }

    programMode mode = UNKNOWN;
    // Argument parsing
    char* neededtag = NULL;
    char* valueToSet = NULL;
    char* filepath = NULL;
    char* picName = NULL;

    for (int i = 1; i < argc; i++) {
        if (strstr(argv[i], "--set=") == argv[i]) {
            if (strstr(argv[i+1], "--value=") == argv[i+1]) {
                neededtag = argv[i] + 6;
                mode = SET;
                i++;
                valueToSet = argv[i] + 8;
            } else {
                printf(RED BOLD "ERROR:" ENDBOLD RESET " --set at position %d isn't followed by a --value\n", i);
                return 1;
            }
        } else if (strstr(argv[i], "--filepath=") == argv[i]) {
            filepath = argv[i] + 11;
        } else if (strstr(argv[i], "--show") == argv[i]) {
            mode = LIST;
        } else if (strstr(argv[i], "--get=") == argv[i]) {
            mode = GET;
            neededtag= argv[i] + 6;
        } else if (strstr(argv[i], "--remove=") == argv[i]) {
            mode = REMOVE;
            neededtag= argv[i] + 9;
        } else if (strstr(argv[i], "--getPic=") == argv[i]) {
            mode = GETPIC;
            picName = argv[i] + 9;
        } else if (strstr(argv[i], "--setPic=") == argv[i]) {
            mode = SETPIC;
            picName = argv[i] + 9;
        } else {
            printf(MAG BOLD "WARNING:" ENDBOLD RESET " Unknown argument %s at position %d\n", argv[i], i);
        }
    }

    // Check for issues with input data
    if (filepath == NULL) {
        printf(RED BOLD "ERROR:" ENDBOLD RESET " Filepath wasn't provided\n");
        return 1;
    }
    if (mode == UNKNOWN) {
        printf(RED BOLD "ERROR:" ENDBOLD RESET " Mode not set\n");
        return 1;
    } else if (mode == GET || mode == SET) {
        if (strlen(neededtag) != 4) {
            printf(RED BOLD "ERROR:" ENDBOLD RESET " Tag %s is %lu characters long, not 4\n", neededtag, strlen(neededtag));
            return 1;
        }
    }

    // Initial file actions
    FILE* file;
    file = fopen(filepath, "rb");
    if (file == NULL) {
        printf(RED BOLD "ERROR:" ENDBOLD RESET " Unable to open file at path %s\n", filepath);
        return 1;
    }
    header hdr = readHeader(file);
    if (!(hdr.fileId[0] == 'I' && hdr.fileId[1] == 'D' && hdr.fileId[2] == '3')) {
        printf(RED BOLD "ERROR:" ENDBOLD RESET " File at path %s doesn't have ID3 metadata\n", filepath);
        return 1;
    }
    int bytesRead = 10;
    int extendedHeaderSize = 0;
    if ((hdr.flags & 0b01000000) != 0) {
        // We have extended header
        printf(MAG BOLD "WARNING:" ENDBOLD RESET " File contains an extended header which is not supported, skipping\n");
        extendedHeaderSize += skipExtendedHeader(file);
        bytesRead += extendedHeaderSize;
    }

    switch (mode) {
        case LIST: {
            printf(BOLD "TAG\tSIZE\tVALUE\n" ENDBOLD);
            while (bytesRead < hdr.tagSize-10) {
                frame frm = readFrame(file);
                if (frm.tagId[0] + frm.tagId[1] + frm.tagId[2] + frm.tagId[3] == 0) {
                    // means we're in the padding
                    return 0;
                }
                // Print frame ID
                printf("%c%c%c%c\t", frm.tagId[0], frm.tagId[1], frm.tagId[2], frm.tagId[3]);
                printf("%d\t", frm.tagSize);
                // Check if it is an image
                if (frm.tagId[0] == 'A' && frm.tagId[1] == 'P' &&
                frm.tagId[2] == 'I' && frm.tagId[3] == 'C') {
                    printf(YEL "use --getPic [FILE] to output image data\n" RESET);
                    bytesRead += frm.tagSize+10;
                    continue;
                }
                for (int i = 0; i < frm.tagSize; i++) {
                    printf("%c", frm.data[i] > 127 ? 0 : frm.data[i]);
                }
                printf("\n");
                bytesRead += frm.tagSize+10;
            }
            break;
        }
        case SETPIC:
        case REMOVE:
        case SET: {
            // 1. Generate frame
            frame newFrame;
            memset(newFrame.flags, 0, 2);
            if (mode == SETPIC) {
                memcpy(newFrame.tagId, "APIC", 4);
                neededtag = (char*)&newFrame.tagId;
                if (picName == NULL) {
                    printf(RED BOLD "ERROR:" ENDBOLD RESET " Image name wasn't provided\n");
                    return 1;
                }

                FILE* image = fopen(picName, "rb");
                if (image == NULL) {
                    printf(RED BOLD "ERROR:" ENDBOLD RESET " Unable to open image at path %s\n", picName);
                    return 1;
                }
                // Can't get fseek to work properly, so this is how it is
                //fseek(image, 0, SEEK_END);
                // newFrame.tagSize = ftell(file);
                // fseek(image, 0, SEEK_SET);
                newFrame.tagSize = 0;
                uint8_t trash;
                while (fread(&trash, 1, 1, image)) {
                    newFrame.tagSize++;
                }
                fclose(image);
                image = fopen(picName, "rb");

                if (strstr(picName, ".jpeg") != NULL || strstr(picName, ".jpg") != NULL) {
                    newFrame.tagSize += 14;
                    newFrame.data = (uint8_t*)malloc(newFrame.tagSize);
                    char jpegHeader[14] = {
                        00,
                        'i', 'm', 'a', 'g', 'e', '/', 'j', 'p', 'e', 'g',
                        00, 03, 00
                    };
                    memcpy(newFrame.data, jpegHeader, 14);
                    fread(newFrame.data+14, newFrame.tagSize-14, 1, image);
                } else if (strstr(picName, ".png") != NULL) {
                    newFrame.tagSize += 13;
                    newFrame.data = (uint8_t*)malloc(newFrame.tagSize);
                    char pngHeader[13] = {
                        00,
                        'i', 'm', 'a', 'g', 'e', '/', 'p', 'n', 'g',
                        00, 03, 00
                    };
                    memcpy(newFrame.data, pngHeader, 13);
                    fread(newFrame.data+13, newFrame.tagSize-13, 1, image);
                } else {
                    printf(RED BOLD "ERROR:" ENDBOLD RESET " Unsupported image filetype\n");
                    return 1;
                }
                fclose(image);
                printf(YEL BOLD "SET:" ENDBOLD RESET " Encoded new frame\n");
            } else if (mode == SET) {
                memcpy(newFrame.tagId, neededtag, 4);
                newFrame.tagSize = strlen(valueToSet)+1;
                newFrame.data = (uint8_t*)malloc(newFrame.tagSize);
                memcpy(newFrame.data+1, valueToSet, newFrame.tagSize-1);
                printf(YEL BOLD "SET:" ENDBOLD RESET " Encoded new frame\n");
            }
            // 2. Parse all frames inside the file
            linkedlist* ll = (linkedlist*)malloc(sizeof(linkedlist));
            ll->head = NULL;
            while (bytesRead < hdr.tagSize-10) {
                frame frm = readFrame(file);
                if (frm.tagId[0] + frm.tagId[1] + frm.tagId[2] + frm.tagId[3] == 0) {
                    break;
                }
                llpush(ll, frm);
                bytesRead += frm.tagSize+10;
            }

            // 3. Locate the llnode for the tag
            llnode* newFrameNode = llsearch(ll, neededtag);
            if (mode == REMOVE) {
                if (newFrameNode == NULL) {
                    fclose(file);
                    printf(YEL BOLD "REMOVE:" ENDBOLD RESET " Tag not found, nothing to delete\n");
                    return 0;
                } else {
                    llremove(ll, newFrameNode);
                }
            } else {
                if (newFrameNode == NULL) {
                    newFrameNode = llpush(ll, newFrame);
                } else {
                    newFrameNode->value = newFrame;
                }
            }
            if (mode == REMOVE) {
                printf(YEL BOLD "REMOVE:" ENDBOLD RESET " Writing new ID3 header\n");
            } else {
                printf(YEL BOLD "SET:" ENDBOLD RESET " Writing new ID3 header\n");
            }
            // 4 Write headers
            FILE* tempFile = fopen(".temp.mp3", "wb");
            header newHeader;
            memcpy(&newHeader, &hdr, sizeof(header));
            newHeader.tagSize = llsize(ll) + extendedHeaderSize;
            writeHeader(tempFile, newHeader);
            fseek(file, 10, SEEK_SET);
            for (int i = 0; i < extendedHeaderSize; i++) {
                uint8_t buffer;
                fread(&buffer, 1, 1, file);
                fwrite(&buffer, 1, 1, tempFile);
            }
           
            // 5 Write frames
            llnode* currNode = ll->head;
            while (currNode != NULL) {
                writeFrame(tempFile, &(currNode->value));
                ll->head = currNode;
                currNode = currNode->next;
            }
            if (mode == REMOVE) {
                printf(YEL BOLD "REMOVE:" ENDBOLD RESET " Copying data from old file\n");
            } else {
                printf(YEL BOLD "SET:" ENDBOLD RESET " Copying data from old file\n");
            }
            // 6 write data
            uint8_t dataBuffer;
            fseek(file, hdr.tagSize+10, SEEK_SET);
            while (fread(&dataBuffer, 1, 1, file)) {
                fwrite(&dataBuffer, 1, 1, tempFile);
            }
            fclose(tempFile);
            fclose(file);
            if (mode == REMOVE) {
                printf(YEL BOLD "REMOVE:" ENDBOLD RESET " Deleting temporary file\n");
            } else {
                printf(YEL BOLD "SET:" ENDBOLD RESET " Deleting temporary file\n");
            }
            remove(filepath);
            rename(".temp.mp3", filepath);
            if (mode == REMOVE) {
                printf(GRN BOLD "REMOVE:" ENDBOLD RESET " Finished writing\n");
            } else {
                printf(GRN BOLD "SET:" ENDBOLD RESET " Finished writing\n");
            }

            // Also working method, but the one above should work fine
            // tempFile = fopen("temp.mp3", "rb");
            // file = fopen(filepath, "wb");
            // uint8_t midfileBuffer;
            // while (fread(&midfileBuffer, 1, 1, tempFile)) {
            //     fwrite(&midfileBuffer, 1, 1, file);
            // }
            // fclose(file);
            // remove(".temp.mp3");

            break;
        }
        case GET: {
            while (bytesRead < hdr.tagSize-10) {
                frame frm = readFrame(file);
                if (frm.tagId[0] + frm.tagId[1] + frm.tagId[2] + frm.tagId[3] == 0) {
                    // means we're in the padding
                    printf(MAG BOLD "WARNING:" ENDBOLD RESET " Tag %s wasn't found\n", neededtag);
                    return 0;
                }

                if (!(frm.tagId[0] == neededtag[0] && frm.tagId[1] == neededtag[1] &&
                frm.tagId[2] == neededtag[2] && frm.tagId[3] == neededtag[3])) {
                    bytesRead += frm.tagSize+10;
                    continue;
                };

                // Print frame ID
                printf(BOLD "TAG:" ENDBOLD "\t%c%c%c%c\n", frm.tagId[0], frm.tagId[1], frm.tagId[2], frm.tagId[3]);
                printf(BOLD "SIZE:" ENDBOLD "\t%d Bytes\n" BOLD "VALUE:" ENDBOLD "\t", frm.tagSize);
                if (neededtag[0] == 'A' && neededtag[1] == 'P' &&
                neededtag[2] == 'I' && neededtag[3] == 'C') {
                    printf(YEL "use --getPic [FILE] to output image data" RESET);
                } else {
                    for (int i = 0; i < frm.tagSize; i++) {
                    printf("%c", frm.data[i] > 127 ? 0 : frm.data[i]);
                }
                }
                printf("\n");
                return 0;
            }
            break;
        }
        case GETPIC: {
            if (picName == NULL) {
                printf(RED BOLD "ERROR:" ENDBOLD RESET " Output path for the picture wasn't provided\n");
                return 1;
            }
            while (bytesRead < hdr.tagSize-10) {
                frame frm = readFrame(file);
                if (frm.tagId[0] + frm.tagId[1] + frm.tagId[2] + frm.tagId[3] == 0) {
                    // means we're in the padding
                    printf(MAG BOLD "WARNING:" ENDBOLD RESET " APIC tag wasn't found\n");
                    return 0;
                }
                if (frm.tagId[0] == 'A' && frm.tagId[1] =='P' &&
                frm.tagId[2] == 'I' && frm.tagId[3] == 'C') {
                    fclose(file);
                    printf(YEL BOLD "GETPIC:" ENDBOLD RESET " Found %u picture bytes\n", frm.tagSize);
                    char* picPath = (char*)malloc(strlen(picName) + 5);
                    strcpy(picPath, picName);

                    uint8_t* fileStart = frm.data;
                    if (strstr((char*)frm.data+1, "image/jpeg") != NULL ) {
                        strcat(picPath, ".jpeg");
                        // if this exits then we found the magic bytes for the JPEG file format
                        while (!(*(fileStart) == 0xFF
                        && *(fileStart+1) == 0xD8 && *(fileStart+2) == 0xFF)) {
                            fileStart++;
                        }
                    } else if (strstr((char*)frm.data+1, "image/png") != NULL ) {
                        strcat(picPath, ".png");
                        // if this exits then we found the magic bytes for the JPEG file format
                        while (!(*(fileStart) == 0x89 && *(fileStart+1) == 0x50
                        && *(fileStart+2) == 0x4E && *(fileStart+3) == 0x47
                        && *(fileStart+4) == 0x0D && *(fileStart+5) == 0x0A
                        && *(fileStart+6) == 0x1A && *(fileStart+7) == 0x0A)) {
                            fileStart++;
                        }
                    } else {
                        printf(RED BOLD "ERROR:" ENDBOLD RESET " Artwork has unknown MIME type: %s\n", frm.data+1);
                        return 1;
                    }

                    FILE* output = fopen(picPath, "wb");
                    fwrite(fileStart, sizeof(uint8_t), (frm.tagSize + (fileStart - frm.data)), output);
                    fclose(output);
                    printf(GRN BOLD "GETPIC:" ENDBOLD RESET " %u picture bytes succesfully written to %s\n", frm.tagSize, picPath);
                    return 0;
                } else {
                    bytesRead += frm.tagSize+10;
                    continue;
                }
            }
            break;
        }
        case UNKNOWN: {
            printf(RED BOLD "ERROR:" ENDBOLD RESET " Mode equals UNKNOWN\n");
            return 1;
        }
    }
}