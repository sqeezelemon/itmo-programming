#include "gtest/gtest.h"
#include "polynomial.cpp"

TEST(VALUE, ZERO_COEFFICIENTS) {
    constexpr int pows1[3] = {0,0,0};
    constexpr lab6::polynomial<2> poly1(pows1);
    constexpr int value1 = poly1.value(100);
    static_assert(value1 == 0, "INCORRECT VALUE");
    ASSERT_TRUE(value1 == 0);

    constexpr int pows2[3] = {10,0,0};
    constexpr lab6::polynomial<2> poly2(pows2);
    constexpr int value2 = poly2.value(100);
    static_assert(value2 == pows2[0], "INCORRECT VALUE");
    ASSERT_TRUE(value2 == pows2[0]);
}

TEST(VALUE, ZERO_VALUE) {
    constexpr int pows[3] = {2,1,1};
    constexpr lab6::polynomial<2> poly(pows);
    constexpr int value = poly.value(0);
    static_assert(value == pows[0], "INCORRECT VALUE");
    ASSERT_TRUE(value == pows[0]);
}

TEST(VALUE, POSITIVE_VALUE) {
    constexpr int pows[3] = {2,3,4};
    constexpr lab6::polynomial<2> poly(pows);
    constexpr int value = poly.value(1);
    static_assert(value == 9, "INCORRECT VALUE");
    ASSERT_TRUE(value == 9);
}

TEST(VALUE, NEGATIVE_VALUE) {
    constexpr int pows[3] = {2,3,4};
    constexpr lab6::polynomial<2> poly(pows);
    constexpr int value = poly.value(-1);
    static_assert(value == 3, "INCORRECT VALUE");
    ASSERT_TRUE(value == 3);
}

TEST(OPERATORS, EQUALITY) {
    constexpr int pows[4] = {2,3,4,0};
    constexpr lab6::polynomial<2> poly1(pows);
    constexpr lab6::polynomial<3> poly2(pows);
    static_assert(poly1 == poly2, "POLY1==POLY2");
    ASSERT_TRUE(poly1==poly2);

    constexpr lab6::polynomial<1> poly3(pows);
    static_assert(poly1 != poly3, "POLY1!=POLY2");
    ASSERT_FALSE(poly1==poly3);
    ASSERT_TRUE(poly1!=poly3);
}

TEST(OPERATORS, CAST) {
    constexpr int pows[4] = {2,3,4,5};
    constexpr lab6::polynomial<2> poly1(pows);
    constexpr lab6::polynomial<3> poly2(pows);
    static_assert(poly1!=poly2, "WRONG EQUALITY");
    ASSERT_TRUE(poly1!=poly2);
    static_assert(poly1==static_cast<lab6::polynomial<2>>(poly2), "WRONG EQUALITY");
    ASSERT_TRUE(poly1==static_cast<lab6::polynomial<2>>(poly2));
}

TEST(OPERATORS, PLUS) {
    constexpr int pows[4] = {2,3,4,5};
    constexpr lab6::polynomial<2> poly1(pows);
    constexpr lab6::polynomial<3> poly2(pows);
    constexpr bool verdict = (
        (poly2+poly1).get(0) == 4 &&
        (poly2+poly1).get(1) == 6 &&
        (poly2+poly1).get(2) == 8 &&
        (poly2+poly1).get(3) == 5
    );
    static_assert(verdict, "SUM-THING IS INCORRECT");
    ASSERT_TRUE(verdict);
}

TEST(OPERATORS, MINUS) {
    constexpr int pows[4] = {2,3,4,5};
    constexpr lab6::polynomial<2> poly1(pows);
    constexpr lab6::polynomial<3> poly2(pows);
    constexpr bool verdict = (
        (poly2-poly1).get(0) == 0 &&
        (poly2-poly1).get(1) == 0 &&
        (poly2-poly1).get(2) == 0 &&
        (poly2-poly1).get(3) == 5
    );
    static_assert(verdict, "MINUS IS INCORRECT");
    ASSERT_TRUE(verdict);
}

int main(int argc, char** argv) {
    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}