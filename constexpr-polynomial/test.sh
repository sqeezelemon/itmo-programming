clear
g++ -c polynomial.cpp -std=c++17 && g++ -c test.cpp -std=c++17 && \
g++ -o test polynomial.o test.o -std=c++17 -lgtest && ./test
rm ./test.o
rm ./polynomial.o
rm ./test