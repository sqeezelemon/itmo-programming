#include <iostream>

namespace lab6 {

template <int Power, class T = int>
class polynomial {
    private:
    T coeffs_[Power+1];

    //////// CONSTEXPR HELPERS

    constexpr inline T ce_pow(T v, int pw) const {
        return (pw < 1) ? 1 : v * ce_pow(v,pw-1);
    }

    constexpr inline int ce_min(const int& a, const int& b) const {
        return (a < b) ? a : b;
    }

    constexpr int ce_value(T& x, int pwr) const noexcept {
        if (pwr == 0) {
            return (coeffs_[0]);
        } else {
            return coeffs_[pwr] * ce_pow(x, pwr) + ce_value(x, pwr-1);
        }
    }

    constexpr void ce_array_init(const T cfs[], int cf) noexcept {
        if (cf < 0) {
            return;
        } else {
            coeffs_[cf] = cfs[cf];
            ce_array_init(cfs, cf-1);
        }
    }

    constexpr void ce_copyconstr_init(const polynomial& p, const int cf) noexcept {
        if (cf < 0) {
            return;
        } else {
            coeffs_[cf] = p.get(cf);
            ce_copyconstr_init(p, cf-1);
        }
    }

    constexpr void ce_default_init(int cf) noexcept {
        if (cf < 0) {
            return;
        } else {
            coeffs_[cf] = T();
            ce_default_init(cf-1);
        }
    }

    constexpr int ce_maxpow(const int pw) const noexcept {
        if (pw < 0) {
            return -1;
        } else {
            return (coeffs_[pw] != 0) ? pw : ce_maxpow(pw-1);
        }
    }

    template <int Pow1, int Pow2>
    constexpr friend bool ce_equal(const polynomial<Pow1>& l, const polynomial<Pow2>& r, const int pw) {
        if (pw < 0) {
            return true;
        } else if (l.coeffs_[pw] != r.coeffs_[pw]) {
            return false;
        } else {
            return ce_equal(l,r,pw-1);
        }
    }

    template<int Pow1, int Pow2>
    constexpr void ce_sum_init(const T cfs1[], const T cfs2[], const int cf, const int mult2) noexcept {
        if (cf < 0) {
            return;
        } else {
            if (cf > Pow1 && cf > Pow2) {
                coeffs_[cf] = 0;
            } else if (cf <= Pow1 && cf <= Pow2) {
                coeffs_[cf] = cfs1[cf] + cfs2[cf]*mult2;
            } else if (cf > Pow1) {
                coeffs_[cf] = cfs2[cf]*mult2;
            } else if (cf > Pow2) {
                coeffs_[cf] = cfs1[cf];
            }
            ce_sum_init<Pow1, Pow2>(cfs1, cfs2, cf-1, mult2);
        }
    }

    constexpr void ce_sum_init(const T cfs1[], const T cfs2[], const int& lim1, const int& lim2, const int cf, const T& mult2) noexcept {
        if (cf < 0) {
            return;
        } else {
            if (cf > lim1 && cf > lim2) {
                coeffs_[cf] = 0;
            } else if (cf <= lim1 && cf <= lim2) {
                coeffs_[cf] = cfs1[cf] + cfs2[cf]*mult2;
            } else if (cf > lim1) {
                coeffs_[cf] = cfs2[cf]*mult2;
            } else if (cf > lim2) {
                coeffs_[cf] = cfs1[cf];
            }
            ce_sum_init(cfs1, cfs2, lim1, lim2, cf-1, mult2);
        }
    }

    // Hacky initializer just for +/- ops
    constexpr polynomial(const T cfs1[], const T cfs2[], const int pw1, const int pw2, const T& mult) noexcept {
        ce_sum_init(cfs1,cfs2,pw1,pw2,Power,mult);
    }

    public:

    //////// INITIALIZERS

    constexpr polynomial(const T cfs[], const int pw = Power) noexcept {
        static_assert(Power>=0, "Power of a polynomial can't be lower than 1");
        ce_array_init(cfs, ce_min(Power, pw));
    }

    constexpr polynomial() noexcept {
        ce_default_init(Power);
    }
    
    constexpr inline T get(int cf) const noexcept {
        return (cf > Power) ? 0 : coeffs_[cf];
    }

    constexpr void set(const int& cf, const T& v) noexcept {
        if (cf > Power) {
            return;
        } else {
            coeffs_[cf] = v;
        }
    }

    constexpr inline T value(T x) const noexcept {
        return ce_value(x, Power);
    }

    constexpr inline T maxpow() const noexcept {
        return ce_maxpow(Power);
    }

    //////// OPERATORS

    constexpr T operator[](int cf) noexcept {
        return get(cf);
    }

    constexpr T operator() (const T& x) noexcept {
        return value(x);
    }

    template<int Pow1, int Pow2>
    constexpr friend bool operator== (const polynomial<Pow1>& l, const polynomial<Pow2>& r) {
        if (l.ce_maxpow(Pow1) != r.ce_maxpow(Pow2)) {
            return false;
        } else {
            return ce_equal(l,r,l.ce_maxpow(Pow1));
        }
    }

    template<int Pow1, int Pow2>
    constexpr friend bool operator!= (const polynomial<Pow1>& l, const polynomial<Pow2>& r) {
        return !(l==r);
    }

    template<int Pow2>
    constexpr operator polynomial<Pow2>() const {
        return polynomial<Pow2>(coeffs_, ce_min(Power, Pow2));
    }

    template<int Pow1, int Pow2>
    constexpr friend polynomial<Pow1> operator+ (const polynomial<Pow1>& l, const polynomial<Pow2>& r) {
        return polynomial<Pow1>(l.coeffs_, r.coeffs_, Pow1, Pow2, 1);
    }

    template<int Pow1, int Pow2>
    constexpr friend polynomial<Pow1> operator- (const polynomial<Pow1>& l, const polynomial<Pow2>& r) {
        return polynomial<Pow1>(l.coeffs_, r.coeffs_, Pow1, Pow2, -1);
    }

    //////// IOSTREAM

    template <int Pow1>
    friend std::ostream& operator<< (std::ostream& os, polynomial<Pow1> p) {
        bool started = false;
        for (int i = Pow1; i >= 0; i--)
            if (p.coeffs_[i] != 0) {
                os << (p.coeffs_[i] < 0 ? " - " : (started ? " + " : ""))
                << (p.coeffs_[i] < 0 ? -p.coeffs_[i] : p.coeffs_[i])
                << "x^" << i;
                started = true;
            }
        if (p.coeffs_[0] == 0 && !started) {
            os << '0';
        }
        return os;
    }
};

}