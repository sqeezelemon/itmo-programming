#pragma once
#include <vector>
#include <cmath>
#include <iostream>
#include <string>
#include <sstream>

namespace mymath {
class Polynomial {
    private:
        std::vector<double> pwr;
    public:
        Polynomial(const std::vector<double> p = {0}):
            pwr(p) {}
        Polynomial(const Polynomial& p):
            pwr(p.pwr) {}
        Polynomial(const std::initializer_list<double> l):
            pwr{l} {}

        // int arithmetic
        Polynomial operator+ (const double& n) {
            Polynomial res = *this;
            res += n;
            return res;
        }
        Polynomial operator- (const double& n) {
            Polynomial res = *this;
            res -= n;
            return res;
        }
        Polynomial operator* (const double& n) {
            Polynomial res = *this;
            res *= n;
            return res;
        }
        Polynomial operator/ (const double& n) {
            Polynomial res = *this;
            res /= n;
            return res;
        }
        Polynomial& operator+= (const double& n) {
            pwr[0] += n;
            return *this;
        }
        Polynomial& operator-= (const double& n) {
            pwr[0] -= n;
            return *this;
        }
        Polynomial& operator*= (const double& n) {
            for (int i = 0; i < pwr.size(); i++) {
                pwr[i] *= n;
            }
            return *this;
        }
        Polynomial& operator/= (const double& n) {
            for (int i = 0; i < pwr.size(); i++) {
                pwr[i] /= n;
            }
            return *this;
        }

        // Polynomial arithmetic
        Polynomial operator+ (const Polynomial& p) {
            Polynomial res = *this;
            res += p;
            return res;
        }
        Polynomial operator- (const Polynomial& p) {
            Polynomial res = *this;
            res -= p;
            return res;
        }
        Polynomial operator* (const Polynomial& p) {
            Polynomial res = *this;
            res *= p;
            return res;
        }
        Polynomial& operator+= (const Polynomial& p) {
            if (p.pwr.size() > pwr.size()) {
                pwr.resize(p.pwr.size());
            }
            for (int i = 0; i < p.pwr.size(); i++) {
                pwr[i] += p.pwr[i];
            }
            return *this;
        }
        Polynomial& operator-= (const Polynomial& p) {
            if (p.pwr.size() > pwr.size()) {
                pwr.resize(p.pwr.size());
            }
            for (int i = 0; i < p.pwr.size(); i++) {
                pwr[i] -= p.pwr[i];
            }
            return *this;
        }
        Polynomial& operator*= (const Polynomial& p) {
            std::vector<double> res;
            res.resize(pwr.size() + p.pwr.size()-1);
            for (int ti = 0; ti < pwr.size(); ti++) {
                for (int pi = 0; pi < p.pwr.size(); pi++) {
                    res[ti+pi] += pwr[ti]*p.pwr[pi];
                }
            }
            pwr = res;
            return *this;
        }

        // Copy constructors
        Polynomial& operator= (const Polynomial& p) {
            pwr = p.pwr;
            return *this;
        }
        Polynomial& operator= (const std::vector<double>& v) {
            pwr = v;
            return *this;
        }
        Polynomial& operator= (const double& n) {
            pwr.resize(1);
            pwr[0] = n;
            return *this;
        }

        // Comparasion
        friend bool operator== (const Polynomial& l, const Polynomial& r) {
            if (l.pwr.size() != r.pwr.size()) {
                return false;
            }
            for (int i = 0; i < r.pwr.size(); i++) {
                if (l.pwr[i] != r.pwr[i]) {
                    return false;
                }
            }
            return true;
        }
        friend inline bool operator!= (const Polynomial& l, const Polynomial& r) {
            return !(l==r);
        }

        [[nodiscard]] double value(double x) const {
            int res = 0;
            for (int i = 0; i < pwr.size(); i++) {
                res += pwr[i] * pow(x,i);
            }
            return res;
        }
        double operator[] (int i) const {
            if (i < pwr.size()) {
                return pwr[i];
            }
            return 0;
        }
        inline int power() const {
            return pwr.size();
        }
        void set(int power, double value) {
            if (power >= pwr.size()) {
                pwr.resize(power);
            }
            pwr[power] = value;
        }

        // Other math
        void takeDerivative() {
            if (pwr.size() < 2) {
                pwr[0] = 0;
                pwr.resize(0);
                return;
            }
            for (int i = 1; i < pwr.size(); i++) {
                pwr[i-1] = pwr[i]*(i);
            }
            pwr.resize(pwr.size()-2);
        }

        [[nodiscard]] friend Polynomial pow(Polynomial& pn, unsigned int pow) {
            if (pow == 0) {
                return Polynomial((std::vector<double>){0});
            }
            if (pow == 1) {
                return pn;
            }
            Polynomial res = pn;
            for (int i = 1; i < pow; i++) {
                res *= pn;
            }
            return res;
        }   

        // iostream
        friend std::ostream& operator<<(std::ostream& s, Polynomial& p) {
            s << p.pwr[0];
            for(int i = 1; i < p.pwr.size(); i++) {
                s << ' ' << p.pwr[i];
            }
            return s;
        }
        friend std::istream& operator>>(std::istream& s, Polynomial& p) {
            double temp;
            while (s >> temp) {
                p.pwr.push_back(temp);
            }
            return s;
        }

        std::string pretty(bool useSuperscript) const {
            std::ostringstream res;
            std::string superscripts[10] = {"⁰","¹","²","³","⁴","⁵","⁶","⁷","⁸","⁹"};
            for (int i = pwr.size(); i > -1; i--) {
                if (pwr[i] == 0) continue;
                res << pwr[i];
                if (i!=0) {
                    if (useSuperscript) {
                        std::string istr = "";
                        int icopy = i;
                        while (icopy != 0) {
                            istr = superscripts[icopy%10] + istr;
                            icopy /= 10;
                        }
                        res << "x" << istr << " + ";
                    } else {
                        res << "*x^" << i << " + ";
                    }
                }
            }
            return res.str();
        }
};
}