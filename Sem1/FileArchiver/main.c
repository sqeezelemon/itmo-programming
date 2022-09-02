#include <stdio.h>
#include <string.h>
#include <stdint.h>
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

// File functions

char* sanitizeFilename(char* filename) {
    char* currAddress = filename;
    char* filenameStart = filename;
    while (*currAddress != '\0') {
        if (*currAddress == '/' || *currAddress == '\\') {
            filenameStart = currAddress + 1;
        }
        currAddress++;
    }
    return filenameStart;
}

// Huffman tree

/*
FILE FORMAT

HEADER
    char[3] ARC - file identifier
    uint16 - amount of files
FILE
    uint16 - filename size
    char[] - filename
    uint8  - HuffNode count - 1 i.e. it will be 0 if bitSize is 1
    huffTreeNode[]
        uint8   - decoded value
        uint8   - bitSize -1, i.e. it will be 0 if bitSize is 1
        uint8[] - code
    uint64 - data size (pre-encoding)
    uint64 - data size (post-encoding)
    byte[] - encoded data
EOF
*/

typedef struct HuffLeaf_s HuffLeaf;
struct HuffLeaf_s {
    // Tree stuff
    HuffLeaf* parent;
    HuffLeaf* one;
    HuffLeaf* zero;
    // Size of the value encoded with huffman algorithm
    uint16_t   bitSize;
    // Encoded value of the node
    uint8_t*  encoded;
    // Actual decoded bytevalue
    uint8_t   decoded;
};

HuffLeaf* newHuffLeaf(uint8_t decoded) {
    HuffLeaf* node = (HuffLeaf*)malloc(sizeof(HuffLeaf));
    node->decoded = decoded;
    node->one = NULL;
    node->zero = NULL;
    node->parent = NULL;
    node->encoded = NULL;
    return node;
}

void freeTree(HuffLeaf* head) {
    if (head->one != NULL) {
        freeTree(head->one);
    }
    if (head->zero != NULL) {
        freeTree(head->zero);
    }
    free(head);
}

void calculateCode(HuffLeaf* leaf) {
    HuffLeaf* curr = leaf;
    leaf->encoded = (uint8_t*)malloc(sizeof(uint8_t)*(leaf->bitSize));
    int bitIndex = leaf->bitSize;
    while (bitIndex > 0) {
        bitIndex--;
        leaf->encoded[bitIndex] = (curr->parent->one == curr) ? 1 : 0;
        curr = curr->parent;
    }
}

void finalizeTree(int headSize, HuffLeaf* head, HuffLeaf** leafs) {
    if (head == NULL) return;
    head->bitSize = headSize;
    headSize++;
    if (head->one == NULL && head->zero == NULL) {
        leafs[head->decoded] = head;
        calculateCode(head);
        return;
    }
    if (head->zero != NULL) {
        head->zero->parent = head;
        finalizeTree(headSize, head->zero, leafs);
    }
    if (head->one != NULL) {
        head->one->parent = head;
        finalizeTree(headSize, head->one, leafs);
    }
}

//////// Priority queue for encoding

// Priority queue
typedef struct pqNode_s pqNode;
struct pqNode_s {
    pqNode*   next;
    int       priority;
    HuffLeaf* leaf;
};

pqNode* newQueueNode(int priority, HuffLeaf* value) {
    pqNode* node = (pqNode*)malloc(sizeof(pqNode));
    node->next = NULL;
    node->leaf = value;
    node->priority = priority;
    return node;
}

int queueNodePrioritySum(pqNode* x, pqNode* y) {
    return ((x == NULL) ? 0 : x->priority) + ((y == NULL) ? 0 : y->priority);
}

typedef struct pq_s {
    pqNode* head;
} pq;

pq* newQueue() {
    pq* queue = (pq*)malloc(sizeof(pq));
    queue->head = NULL;
    return queue;
}

void push(pqNode* node, pq* queue) {
    if (queue->head == NULL) {
        queue->head = node;
        return;
    }

    if (queue->head->priority > node->priority) {
        node->next = queue->head;
        queue->head = node;
        return;
    }

    pqNode* curr = queue->head;
    while (curr->next != NULL && curr->next->priority < node->priority) {
        curr = curr->next;
    }

    node->next = curr->next;
    curr->next = node;
}

HuffLeaf* popLeaf(pq* queue) {
    if (queue->head == NULL) {
        return NULL;
    }
    pqNode*   node = queue->head;
    HuffLeaf* leaf = node->leaf;
    queue->head = node->next;
    free(node);
    return leaf;
}

pqNode* popQueueNode(pq* queue) {
    if (queue->head == NULL) {
        return NULL;
    }
    pqNode* node = queue->head;
    queue->head = node->next;
    return node;
}

//////// FILE

void encodeItem(FILE* archive, char* filename) {
    printf(BOLD YEL "ENCODER:" RESET ENDBOLD " Started encoding file %s\n", filename);
    char* pathExcludedFilename = sanitizeFilename(filename);

    // 1. Get file byte stats
    FILE* file = fopen(filename, "rb");
    int byteStats[256];
    memset(byteStats, 0, sizeof(int)*256);
    uint8_t byteBuffer;
    while (fread(&byteBuffer, sizeof(uint8_t), 1, file)) {
        byteStats[byteBuffer]++;
    }
    fclose(file);
    
    // 2. Generate priority queue
    pq* queue = newQueue();
    
    HuffLeaf* currLeaf;
    pqNode* currNode;
    for (int i = 0; i < 256; i++) {
        if (byteStats[i] == 0) continue;
        currLeaf = newHuffLeaf(i);
        currNode = newQueueNode(byteStats[i], currLeaf);
        push(currNode, queue);
    }

    // 3. Generate tree using the queue
    pqNode* zero;
    pqNode* one;
    while (queue->head->next != NULL) {
        zero = popQueueNode(queue);
        one = popQueueNode(queue);
        currLeaf = newHuffLeaf(-1);
        currLeaf->one = one->leaf;
        currLeaf->zero = zero->leaf;
        currNode = newQueueNode(queueNodePrioritySum(one, zero), currLeaf);
        push(currNode, queue);
        free(zero);
        free(one);
    }

    // 3.1 Cleanup
    HuffLeaf* head = popLeaf(queue);
    free(queue);

    // 4. Generate tree
    // Each byte corresponds to one leaf
    // Aka, position 1 corresponds to a leaf which encodes byte 00000001
    HuffLeaf* leafs[256];
    memset(leafs, 0, sizeof(HuffLeaf*)*256);
    finalizeTree(0, head, leafs);

    printf(BOLD YEL "ENCODER:" RESET ENDBOLD " Built tree for %s\n", filename);

    // 5. Write metadata
    uint16_t filenameSize = strlen(pathExcludedFilename);
    fwrite(&filenameSize, sizeof(uint16_t), 1, archive);
    fwrite(pathExcludedFilename, sizeof(char)*filenameSize, 1, archive);
    uint8_t temp = -1;

    // Write amount of leafs
    for (int i = 0; i < 256; i++) {
        if (leafs[i] != NULL) temp++;
    }
    temp--;
    fwrite(&temp, sizeof(uint8_t), 1, archive);
    
    // Write leafs
    for (int i = 0; i < 256; i++) {
        if (leafs[i] == NULL) continue;
        //printEncodedLeaf(leafs[i]);
        //Write decoded value
        fwrite(&(leafs[i]->decoded), sizeof(uint8_t), 1, archive);
        // Write bitsize
        temp = leafs[i]->bitSize - 1;
        fwrite(&temp, sizeof(uint8_t), 1, archive);
        temp = 0;
        for (int bit = 0; bit < leafs[i]->bitSize; bit++) {
            temp = temp | (leafs[i]->encoded[bit] << (7-(bit%8)));
            if (((bit+1)%8 == 0 && bit != 0) || (bit+1) == leafs[i]->bitSize ) {
                fwrite(&temp, sizeof(uint8_t), 1, archive);
                temp = 0;
            }
        }
    }

    printf(BOLD GRN "ENCODER:" RESET ENDBOLD " Encoded metadata for %s\n", filename);

    // Amount of bytes before encoding, aka how much leafs should be read
    uint64_t leafAmount = 0;
    // Amount of encoded bits, translated to whole bytes before writing
    uint64_t encodedSize = 0;
    for (int i = 0; i < 256; i++) {
        if (leafs[i] == NULL) continue;
        leafAmount += byteStats[i];
        encodedSize += byteStats[i] * leafs[i]->bitSize;
    }
    fwrite(&leafAmount, sizeof(uint64_t), 1, archive);
    encodedSize = (encodedSize+7) / 8;
    fwrite(&encodedSize, sizeof(uint64_t), 1, archive);

    // 6. Encoding the actual file
    uint8_t encodingBuffer = 0;
    int bitIndex = 7;
    file = fopen(filename, "rb");
    for (int byte = 0; byte < leafAmount; byte++) {
        fread(&byteBuffer, sizeof(uint8_t), 1, file);
        currLeaf = leafs[byteBuffer];
        for (int bit = 0; bit < currLeaf->bitSize; bit++) {
            encodingBuffer = encodingBuffer | ((currLeaf->encoded[bit]) << bitIndex);
            bitIndex--;
            if (bitIndex == -1) {
                fwrite(&encodingBuffer, sizeof(uint8_t), 1, archive);
                encodingBuffer = 0;
                bitIndex = 7;
            }
        }
    }
    if (bitIndex != 7) {
        fwrite(&encodingBuffer, sizeof(uint8_t), 1, archive);
    }
    fclose(file);
    freeTree(head);

    printf(BOLD GRN "ENCODER:" RESET ENDBOLD " Encoded file at path %s\n", filename);
}

void decodeItem(FILE* archive) {
    // Read filename
    uint16_t filenameSize;
    fread(&filenameSize, sizeof(uint16_t), 1, archive);
    char* filename = (char*)malloc(sizeof(char)*filenameSize + 1);
    fread(filename, sizeof(char)*filenameSize, 1, archive);
    filename[filenameSize] = '\0';

    printf(BOLD YEL "DECODER:" RESET ENDBOLD " Started decoding file %s\n", filename);

    FILE* output = fopen(filename, "wb");
    if (output == NULL) {
        printf(BOLD RED "ERROR:" RESET ENDBOLD " Unable to open file for writing at path %s\n", filename);
    }

    // Read the huffman tree
    uint8_t leafsCount;
    fread(&leafsCount, sizeof(uint8_t), 1, archive);
    leafsCount++;
    HuffLeaf* head = newHuffLeaf(0);
    uint8_t leafValue, leafBitSize, byteBuffer;
    HuffLeaf* newLeaf;
    HuffLeaf* currLeaf;
    for (int leaf = 0; leaf <= leafsCount; leaf++) {
        fread(&leafValue, sizeof(uint8_t), 1, archive);
        fread(&leafBitSize, sizeof(uint8_t), 1, archive);
        leafBitSize++;
        currLeaf = head;

        for (int bit = 0; bit < leafBitSize; bit++) {
            if (bit % 8 == 0) {
                fread(&byteBuffer, sizeof(uint8_t), 1, archive);
            }
            if ( ((byteBuffer >> (7-(bit%8))) & 1) == 1) {
                // meaning this bit is 1
                if (currLeaf->one == NULL) {
                    newLeaf = newHuffLeaf(leafValue);
                    newLeaf->bitSize = bit + 1;
                    currLeaf->one = newLeaf;
                    currLeaf->one->parent = currLeaf;
                }
                currLeaf = currLeaf->one;
            } else {
                // meaning this bit is 0
                if (currLeaf->zero == NULL) {
                    newLeaf = newHuffLeaf(leafValue);
                    newLeaf->bitSize = bit + 1;
                    currLeaf->zero = newLeaf;
                    currLeaf->zero->parent = currLeaf;
                }
                currLeaf = currLeaf->zero;
            }
        }
    }

    printf(BOLD GRN "DECODER:" RESET ENDBOLD " Decoded metadata for %s\n", filename);

    // Decoding the file
    uint64_t dataSize;
    fread(&dataSize, sizeof(uint64_t), 1, archive);
    uint64_t encodedSize;
    fread(&encodedSize, sizeof(uint64_t), 1, archive);
    currLeaf = head;
    int bitIndex = 7;
    uint8_t outputBuffer;

    fread(&byteBuffer, sizeof(uint8_t), 1, archive);
    for(int byte = 0; byte < dataSize; byte++) {
        while (!(currLeaf->one == NULL && currLeaf->zero == NULL)) {
            if (((byteBuffer >> bitIndex) & 1) == 1) {
                currLeaf = currLeaf->one;
            } else {
                currLeaf = currLeaf->zero;
            }
            //printf("%d", ((byteBuffer >> bitIndex) & 1) == 1);
            bitIndex--;
            if (bitIndex < 0) {
                fread(&byteBuffer, sizeof(uint8_t), 1, archive);
                bitIndex = 7;
            }
        }
        outputBuffer = currLeaf->decoded;
        fwrite(&outputBuffer, sizeof(uint8_t), 1, output);
        currLeaf = head;
    }
    fclose(output);
    printf(BOLD GRN "DECODER:" RESET ENDBOLD " Decoded file %s\n", filename);
}

void listItem(FILE* archive) {
    // Read filename
    uint16_t filenameSize;
    fread(&filenameSize, sizeof(uint16_t), 1, archive);
    char* filename = (char*)malloc(sizeof(char)*filenameSize + 1);
    fread(filename, sizeof(char)*filenameSize, 1, archive);
    filename[filenameSize] = '\0';

    // Read leafs
    uint8_t leafsCount;
    fread(&leafsCount, sizeof(uint8_t), 1, archive);
    leafsCount++;
    uint8_t leafValue, leafBitSize;
    for (int leaf = 0; leaf <= leafsCount; leaf++) {
        fread(&leafValue, sizeof(uint8_t), 1, archive);
        fread(&leafBitSize, sizeof(uint8_t), 1, archive);
        leafBitSize++;
        for (int i = 0; i < (leafBitSize + 7)/8; i++) {
            getc(archive);
        }
    }
    uint64_t dataSize;
    fread(&dataSize, sizeof(uint64_t), 1, archive);
    printf("%s\t", filename);
    printf("%llu Bytes\n", dataSize);
    // Skip encoded data
    uint64_t encodedSize;
    fread(&encodedSize, sizeof(uint64_t), 1, archive);
    fseek(archive, encodedSize, SEEK_CUR);
}

typedef enum ProgramMode_e {
    CREATE,
    LIST,
    EXTRACT
} ProgramMode;

int main(int argc, char **argv) {
    
    //////// PARSE ARGUMENTS
    if (argc == 2) {
        if (strcmp("--help", argv[1])*strcmp("-h", argv[1]) == 0) {
            printf("This is an archiver with per-byte huffman codes.\n\
" BOLD "COMMANDS:" ENDBOLD "\n\
--file [FILE]   Set the archive read/write path\n\
--create        Creates an archive from files\n\
--extract       Extracts the archive into the current folder\n\
--list          Lists files inside the archive\n\
FILE1 FILE2...  File to be archived\n\
--help / -h     Outputs this dialog\n"
            );
            return 0;
        }
    }

    int archivePathIndex = 0;
    ProgramMode mode;
    uint8_t* isFileAtIndex = (uint8_t*)malloc(sizeof(uint8_t)*argc);
    memset(isFileAtIndex, 0, sizeof(uint8_t)*argc);
    for (int i = 1; i < argc; i++) {
        if (strcmp("--file", argv[i]) == 0) {
            i += 1;
            if (i < argc) {
                archivePathIndex = i;
            } else {
                printf(BOLD RED "ERROR:" RESET ENDBOLD " Argument --file at position %d doesn't provide a filepath\n", i);
                return 0;
            }
        } else if (strcmp("--list", argv[i]) == 0) {
            mode = LIST;
        } else if (strcmp("--extract", argv[i]) == 0) {
            mode = EXTRACT;
        } else if (strcmp("--create", argv[i]) == 0) {
            mode = CREATE;
        } else {
            FILE* test = fopen(argv[i], "rb");
            if (test == NULL) {
                printf(BOLD RED "ERROR:" RESET ENDBOLD " Unable to open file for reading at path %s\n", argv[i]);
            } else {
                isFileAtIndex[i] = 1;
                fclose(test);
            }
        }
    }

    if (archivePathIndex == 0) {
        printf(BOLD RED "ERROR:" RESET ENDBOLD " Filepath not provided\n");
        return 0;
    }
    FILE* archive;
    switch (mode) {
        case CREATE: {
            uint16_t fileAmount;
            for (int i = 0; i < argc; i++) {
                fileAmount += isFileAtIndex[i];
            }
            if (fileAmount == 0) {
                printf(BOLD RED "ERROR:" RESET ENDBOLD " No files were provided\n");
                return 0;
            }
            archive = fopen(argv[archivePathIndex], "wb");
            if (archive == NULL) {
                printf(BOLD RED "ERROR:" RESET ENDBOLD " Unable to open file for writing at path %s\n", argv[archivePathIndex]);
                return 0;
            }
            char header[3] = {'A', 'R', 'C'};
            fwrite(header, sizeof(uint8_t)*3, 1, archive);
            fwrite(&fileAmount, sizeof(uint16_t), 1, archive);
            for (int i = 0; i < argc; i++) {
                if (isFileAtIndex[i] != 1) continue;
                encodeItem(archive, argv[i]);
            }
            fclose(archive);
            printf(BOLD GRN "ENCODER:" RESET ENDBOLD " Encoded archive to path %s\n", argv[archivePathIndex]);
            break;
        }
        case EXTRACT: {
            archive = fopen(argv[archivePathIndex], "rb");
            if (archive == NULL) {
                printf(BOLD RED "ERROR:" RESET ENDBOLD " Unable to open file for writing at path %s\n", argv[archivePathIndex]);
                return 0;
            }

            char header[3];
            fread(header, sizeof(char), 3, archive);
            if (!(header[0] == 'A' && header[1] == 'R' && header[2] == 'C')) {
                printf(BOLD RED "ERROR:" RESET ENDBOLD " File header not recognized\n");
                return 0;
            }

            uint16_t fileAmount;
            fread(&fileAmount, sizeof(uint16_t), 1, archive);
            for (int i = 0; i < fileAmount; i++) {
                decodeItem(archive);
            }
            fclose(archive);
            printf(BOLD GRN "ENCODER:" RESET ENDBOLD " Decoded archive from %s\n", argv[archivePathIndex]);
            break;
        }
        case LIST: {
            archive = fopen(argv[archivePathIndex], "rb");
            if (archive == NULL) {
                printf(BOLD RED "ERROR:" RESET ENDBOLD " Unable to open file for writing at path %s\n", argv[archivePathIndex]);
                return 0;
            }

            char header[3];
            fread(header, sizeof(char), 3, archive);
            if (!(header[0] == 'A' && header[1] == 'R' && header[2] == 'C')) {
                printf(BOLD RED "ERROR:" RESET ENDBOLD " File header not recognized\n");
                return 0;
            }
            uint16_t fileAmount;
            fread(&fileAmount, sizeof(uint16_t), 1, archive);
            printf("%d FILES:\n", fileAmount);
            for (int i = 0; i < fileAmount; i++) {
                listItem(archive);
            }
            fclose(archive);
        }
    }
}