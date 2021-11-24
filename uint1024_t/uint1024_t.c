#include <stdio.h>
#include <stdint.h>

typedef struct uint1024_s
{
    uint32_t pcs[32];
} uint1024_t;

uint1024_t from_uint(unsigned int x)
{
    uint1024_t result;
    for (int i = 31; i > -1; i--)
    {
        result.pcs[i] = x % 4294967296;
        x = x / 4294967296;
    }
    return result;
}

uint1024_t add_op(uint1024_t x, uint1024_t y)
{
    uint32_t overflow = 0;
    uint1024_t result = from_uint(0);
    for (int i = 31; i > -1; i--)
    {
        result.pcs[i] = x.pcs[i] + y.pcs[i] + overflow;
        overflow = (x.pcs[i] > result.pcs[i] || y.pcs[i] > result.pcs[i]) ? 1 : 0;
        //printf("%u\t%u\t%u\t%u\n", result.pcs[i], x.pcs[i], y.pcs[i], overflow);
    }
    return result;
}

uint1024_t subtr_op(uint1024_t x, uint1024_t y)
{
    for (int i = 31; i > -1; i--)
    {
        y.pcs[i] = ~y.pcs[i];
    }
    y = add_op(y, from_uint(1));
    return add_op(x, y);
}

void shift_left(uint1024_t *x)
{
    for (int i = 0; i < 31; i++)
    {
        x->pcs[i] = (x->pcs[i] << 1) + (x->pcs[i + 1] >> 31);
    }
    x->pcs[31] = (x->pcs[31] << 1);
}

void shift_right(uint1024_t *x)
{
    for (int i = 31; i > 0; i--)
    {
        x->pcs[i] = (x->pcs[i] >> 1) + (x->pcs[i - 1] << 31);
    }
    x->pcs[0] = (x->pcs[0] >> 1);
}

uint1024_t mult_op(uint1024_t x, uint1024_t y)
{
    uint1024_t result = from_uint(0);

    for (int i = 0; i < 1024; i++)
    {
        // 1. ADD x TO result IF 1
        // 2. SHIFT x TO THE LEFT
        // 3. SHIFT y TO THE RIGHT

        if (y.pcs[31] & 0b00000001)
        {
            result = add_op(result, x);
        }
        shift_left(&x);
        shift_right(&y);
    }
    return result;
}

void printf_value(uint1024_t x)
{
    // DOUBLE DABBLE

    // 10^308 < 2^1024 < 10^309
    uint8_t results[155] = {0};

    for (int iter = 0; iter < 1024; iter++)
    {
        // ADD 3 IF NEEDED
        for (int i = 0; i < 155; i++)
        {
            if ((results[i] & 0b00001111) >= 5)
            {
                results[i] += 0b00000011;
            }
            if ((results[i] >> 4) >= 5)
            {
                results[i] += 0b00110000;
            }
        }

        // SHIFT BITS
        for (int i = 0; i < 154; i++)
        {
            results[i] = (results[i] << 1) + (results[i + 1] >> 7);
            //printf("%u", results[i]);
        }
        results[154] = (results[154] << 1) + (x.pcs[0] >> 31);

        // SHIFT UINT1024
        for (int shiftIndex = 0; shiftIndex < 31; shiftIndex++)
        {
            x.pcs[shiftIndex] = (x.pcs[shiftIndex] << 1) + (x.pcs[shiftIndex + 1] >> 31);
        }
        x.pcs[31] <<= 1;
    }

    uint8_t zeroEnded = 0;
    for (int i = 0; i < 155; i++)
    {
        char temp = (results[i] >> 4);
        if (temp != 0 || zeroEnded)
        {
            zeroEnded = 1;
            printf("%c", temp + '0');
        }
        temp = (results[i] & 0b00001111);
        if (temp != 0 || zeroEnded)
        {
            zeroEnded = 1;
            printf("%c", temp + '0');
        }
    }
    // Everything is 0, thus only print 0
    if (!zeroEnded)
    {
        printf("0");
    }
    printf("\n");
}

void scanf_value(uint1024_t *x)
{
    uint1024_t temp = from_uint(0);
    *x = temp;
    char ch = getchar();
    while (ch >= '0' && ch <= '9')
    {
        temp.pcs[31] = 10;
        *x = mult_op(*x, temp);
        temp.pcs[31] = ch - '0';
        *x = add_op(*x, temp);
        ch = getchar();
    }
}

void printf_bin(uint1024_t x)
{
    for (int pcsIndex = 0; pcsIndex < 32; pcsIndex++)
    {
        for (int bitIndex = 31; bitIndex > -1; bitIndex--)
        {
            printf("%c", ((x.pcs[pcsIndex] >> bitIndex) & 0b00000001) ? '1' : '0');
        }
    }
}

int main(int argc, char **argv)
{
    printf("scanf_value(a):\n");
    uint1024_t a;
    scanf_value(&a);

    printf("from_uint(b): 1000\n");
    uint1024_t b = from_uint(1000);
    // MULT
    uint1024_t mult = mult_op(a,b);
    printf("mult_op:\n");
    printf_value(mult);

    // SUBTR
    uint1024_t subtr = subtr_op(a,b);
    printf("subtr_op:\n");
    printf_value(subtr);

    // ADD
    uint1024_t add = add_op(a,b);
    printf("add_op:\n");
    printf_value(add);

    // SHIFT
    printf("printf_bin(a):\n");
    printf_bin(a);

    printf("shift_left(a):\n");
    shift_left(&a);
    printf_bin(a);

    printf("shift_right(a):\n");
    shift_right(&a);
    printf_bin(a);
}