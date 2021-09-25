#include <stdio.h>
#include <string.h>

int main(int argc, char **argv) {
    if (argc == 2) {
        if (strcmp(argv[1], "--help") == 0 || strcmp(argv[1], "--h") == 0) {
            printf("Помощь по команде\n");
            printf("-h \t --help \t Вызов этого меню помощи.\n");
            printf("-w \t --words \t Количество слов в документе.\n");
            printf("-l \t --lines \t Количество линий в документе.\n");
            printf("-c \t --bytes \t Количество байтов в документе.\n");
            printf("Если аргументы не введены, то будут выведены все 3 аргумента.\n");

            printf("Формат вызова команды:\t./main [АРГУМЕНТЫ] путь\n");
            printf("Пример использования:\t./main --words text.txt\n");
            return 0;
        }
    }

    // argsInt - умноженные простые числа, каждому аргументу соответствует число.
    // Позже при выводе проводится проверка того, делится ли argsInt на это простое число - если делится,
    // значит аргумент был использован, если нет, то аргумент не был использован, а если 1, то аргументов не было.
    // Это защищает от дублирования аргументов и делает так,
    // что порядок вывода аргументов всегда одинаковый
    int i;
    int argsInt = 1;
    for (i = 1; i < (argc - 1); i++) {
        if (strcmp(argv[i], "--words") == 0 || strcmp(argv[i], "-w") == 0) {
            argsInt *= 2;
        } else if (strcmp(argv[i], "--lines") == 0 || strcmp(argv[i], "-l") == 0) {
            argsInt *= 3;
        } else if (strcmp(argv[i], "--bytes") == 0 || strcmp(argv[i], "-c") == 0) {
            argsInt *= 5;
        } else {
            printf("ВНИМАНИЕ: Неопознанный аргумент на позиции %d: %s\n", (i + 1), argv[i]);
            printf("Для справки по командам запустите программу с аргументом --help или -h\n");
        }
    }

    if (argsInt == 1) {
        argsInt = 30; // 2*3*5, потому что если 1, то аргументов не было, т.е. выводим всё
    }

    int wordCount = 1;
    int lineCount = 1;
    int byteCount = 0;


    FILE *file = fopen(argv[argc - 1], "r");


    char ch = fgetc(file);
    while (ch != EOF) {
        byteCount += 1;
        if (ch == '\n') {
            wordCount += 1;
            lineCount += 1;
        } else if (ch == ' ') {
            wordCount += 1;
        }

        ch = fgetc(file);
    }
    fclose(file);


    if (argsInt % 2 == 0) {
        printf("Слов:\t%d\n", wordCount);
    }
    if (argsInt % 3 == 0) {
        printf("Линий:\t%d\n", lineCount);
    }
    if (argsInt % 5 == 0) {
        printf("Байтов:\t%d\n", byteCount);
    }
    return 0;
}

