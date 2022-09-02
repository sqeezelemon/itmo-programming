#pragma once
#include <cmath>
#include <iostream>
#include <vector>
#include <stdexcept>

namespace mymath {
    class Point {
    public:
        double x,y;

        Point(double inp_x = 0, double inp_y = 0):
            x(inp_x), y(inp_y) {}
        Point(const Point& p):
            x(p.x), y(p.y) {}
        
        Point& operator= (const Point& p) {
            x = p.x;
            y = p.y;
            return *this;
        }
        Point& operator+= (const Point& p) {
            x += p.x;
            y += p.y;
            return *this;
        }
        Point& operator-= (const Point& p) {
            x -= p.x;
            y -= p.y;
            return *this;
        }
        Point operator+ (const Point& p) {
            Point res;
            res += p;
            return res;
        }
        Point operator- (const Point& p) {
            Point res;
            res -= p;
            return res;
        }

        inline friend bool operator== (const Point& l, const Point& r) {
            return ((l.x == r.x) && (l.y == r.y));
        }
        inline friend bool operator!= (const Point& l, const Point& r){
            return !(l == r);
        }

        // iostream output/input
        friend std::ostream& operator<<(std::ostream& s, const Point& p) {
            s << p.x << ' ' << p.y;
            return s;
        }
        friend std::istream& operator>>(std::istream& s, Point& p) {
            s >> p.x >> p.y;
            return s;
        }

        // Distance to another Point
        float distance(const Point& p) const {
            return sqrt(pow((x - p.x),2) + pow((y - p.y), 2));
        }
    };

    class Polyline {
    protected:
        std::vector<Point> pts;
    public:
        Polyline(const std::vector<Point> p = {}):
            pts(p) {}
        Polyline(const Polyline& l):
            pts(l.pts) {}
        
        friend bool operator== (const Polyline& l, const Polyline& r) {
            if (l.pts.size() != r.pts.size()) return false;
            for (int i = 0; i < l.pts.size(); i++) {
                if (l.pts[i] != r.pts[i]) return false;
            }
            return true;
        }
        inline friend bool operator!= (const Polyline& l, const Polyline& r) {
            return !(l==r);
        }

        // iostream output/input
        friend std::ostream& operator<<(std::ostream& s, const Polyline& l) {
            for (int i = 0; i < l.pts.size()-1; i++) {
                s << l.pts[i] << ' ';
            }
            s << l.pts.back();
            return s;
        }
        friend std::istream& operator>>(std::istream& s, Polyline& l) {
            l.pts.resize(0);
            Point temp;
            while (s >> temp) {
                l.pts.push_back(temp);
            }
            return s;
        }

        virtual double perimiter() const {
            double len;
            for (int i = 1; i < pts.size(); i++) {
                len += pts[i].distance(pts[i-1]);
            }
            return len;
        }

        inline int pointsAmmount() const {
            return pts.size();
        }

        Point operator[] (int i) const {
            if (i >= pts.size()) {
                throw std::invalid_argument("Polyline[] - Invalid index");
            }
            return pts[i];
        }
        virtual inline bool isEnclosed() const {
            return (pts.front() == pts.back());
        }
    };

    class Line: public Polyline {
        public:
            Line(Point p1, Point p2):
                Polyline({p1,p2}) {}
            Line(const Line& l):
                Polyline(l.pts) {}
            
            inline double yCoeff() const {
                return p2().x - p1().x;
            }
            inline double xCoeff() const {
                return p1().y - p2().y;
            }
            inline double freeCoeff() const {
                return p1().x*p2().y-p2().x*p1().y;
            }
            inline Point p1() const {
                return pts[0];
            }
            inline Point p2() const {
                return pts[1];
            }
            virtual inline bool isEnclosed() const {
                return false;
            }
            inline double slope() const {
                return -xCoeff()/yCoeff();
            }
    };

    class ClosedPolyline: public Polyline {
        public:
        ClosedPolyline(const std::vector<Point> p):
            Polyline(p) {}
        ClosedPolyline(const Polyline l):
            Polyline(l) {
                if (!l.isEnclosed()) {
                    throw std::invalid_argument("ClosedPolyline - Supplied Polyline isn't enclosed");
                }
                pts.pop_back();
            }
        ClosedPolyline(const ClosedPolyline& l):
            Polyline(l.pts) {}
        
        friend bool operator== (const ClosedPolyline& l, const ClosedPolyline& r) {
            if (l.pts.size() != r.pts.size()) return false;
            for (int i = 0; i < l.pts.size(); i++) {
                if (l.pts[i] != r.pts[i]) return false;
            }
            return true;
        }
        inline friend bool operator!= (const ClosedPolyline& l, const ClosedPolyline& r) {
            return !(l==r);
        }

        friend std::istream& operator>>(std::istream& s, ClosedPolyline& l) {
            Polyline p;
            s >> p;
            ClosedPolyline c(p);
            l = c;
            return s;
        }

        bool intersectsItself() {
            for (int i = 0; i < pts.size()-1; i++) {
                for (int j = i+1; j <= pts.size(); j++) {
                    Line l1(pts[i], pts[i+1]);
                    Line l2(pts[j], pts[(j+1)%pts.size()]);
                    double x = (
                        ((l1.yCoeff()/l2.yCoeff())*l2.freeCoeff() - l1.freeCoeff()) /
                        ((l1.yCoeff()/l2.yCoeff())*l2.xCoeff() + l1.xCoeff())
                    );
                    double y = (-l1.xCoeff()*x - l1.freeCoeff()) / l1.yCoeff();
                    if (
                        std::min(l1.p1().x, l1.p2().x) <= x && x <= std::max(l1.p1().x, l1.p2().x)
                        && std::min(l2.p1().x, l2.p2().x) <= x && x <= std::max(l2.p1().x, l2.p2().x)
                        && std::min(l1.p1().y, l1.p2().y) <= y && y <= std::max(l1.p1().y, l1.p2().y)
                        && std::min(l2.p1().y, l2.p2().y) <= y && y <= std::max(l2.p1().y, l2.p2().y)
                    ) {
                        return false;
                    }
                }
            }
            return true;
        }
        virtual double perimeter() const {
            double len;
            for (int i = 1; i < pts.size(); i++) {
                len += pts[i].distance(pts[i-1]);
            }
            len += pts[0].distance(pts.back());
            return len;
        }
        virtual inline bool isEnclosed() const {
            return true;
        }
    };

    class Polygon: public ClosedPolyline {
        public:
        Polygon(std::vector<Point> p):
            ClosedPolyline(p) {
                if (p.size() < 3) {
                    throw std::invalid_argument("Polygon - Too few points");
                }
                if (!intersectsItself()) {
                    throw std::invalid_argument("Polygon - Sides intersect themselves");
                }
            }
        Polygon(const Polygon& p):
            ClosedPolyline(p.pts) {}
        Polygon(const ClosedPolyline& l):
            ClosedPolyline(l) {
                if (l.pointsAmmount() < 3) {
                    throw std::invalid_argument("Polygon - Too few points");
                }
                if (!intersectsItself()) {
                    throw std::invalid_argument("Polygon - Sides intersect themselves");
                }
            }
        Polygon(const Polyline& l):
            Polygon(ClosedPolyline(l)) {}
        
        double area() {
            // Shoelace formula
            double res = 0;
            int j = pts.size() - 1;
            for (int i = 0; i < pts.size(); i++) {
                res += (pts[i].x + pts[j].x) * (pts[i].y - pts[j].y);
                j = i;
            }
            res /= 2;
            return abs(res);
        }

        friend std::istream& operator>>(std::istream& s, Polygon& pg) {
            Polyline l;
            s >> l;
            ClosedPolyline c(l);
            Polygon p(c);
            pg = p;
            return s;
        }
    };

    class Triangle: public Polygon {
        public:
        Triangle(std::vector<Point> p):
            Triangle(Polygon(p)) {}
        Triangle(const Polygon& p):
            Polygon(p) {
                if (p.pointsAmmount() != 3) {
                    throw std::invalid_argument("Triangle - invalid number of points (expected 3)");
                }
            }
        Triangle(const Triangle& p):
            Polygon(p.pts) {}
        
        bool rightAngled() const {
            for (int i = 0; i < 3; i++) {
                if (Line(pts[i], pts[(i+1)%3]).slope() == 1/Line(pts[(i+1)%3], pts[(i+2)%3]).slope()) {
                    return true;
                }
            }
            return false;
        }
    };

    class Trapezoid: public Polygon {
        public:
        Trapezoid(std::vector<Point> p):
            Trapezoid(Polygon(p)) {}
        Trapezoid(Polygon p):
            Polygon(p) {
                if (p.pointsAmmount() != 4) {
                    throw std::invalid_argument("Trapezoid - too little points (expected 4)");
                }
                if (
                    (Line(pts[0], pts[1]).slope() != Line(pts[2], pts[3]).slope())
                    && (Line(pts[1], pts[2]).slope() != Line(pts[3], pts[1]).slope())
                ) {
                    throw std::invalid_argument("Trapezoid - not a trapezoid (no parallel lines)");
                }
            }
        Trapezoid(const Trapezoid& t):
            Polygon(t.pts) {}
        bool isParallelogram() const {
            return (
                (Line(pts[0], pts[1]).slope() == Line(pts[2], pts[3]).slope())
                && (Line(pts[1], pts[2]).slope() == Line(pts[3], pts[1]).slope())
            );
        }
    };

    class RightPolygon: public Polygon {
        public:
        RightPolygon(std::vector<Point> p):
            Polygon(p) {
                double baseDist = pts[0].distance(*pts.end());
                for (int i = 1; i < pts.size(); i++) {
                    if (pts[i].distance(pts[i-1]) != baseDist) {
                        throw std::invalid_argument("RightPolygon - different side sizes");
                    }
                }
                for (int i = 0; i < pts.size(); i++) {
                    Point vec1 = pts[(i+1)%pts.size()] - pts[i];
                    Point vec2 = pts[(i+2)%pts.size()] - pts[(i+1)%pts.size()];
                    double angle = acos(
                        (vec1.x*vec2.x + vec1.y*vec2.y)
                        /(vec1.distance(Point())*vec1.distance(Point()))
                    );
                    if (angle != 360/pts.size()) {
                        throw std::invalid_argument("RightPolygon - different angles");
                    }
                }
            }

            double internalRadius() const {
                return side()/(2*tan(180/pts.size()));
            }
            inline double externalRadius() const {
                return side()/(2*sin(180/pts.size()));
            }
            inline double angle() const {
                return (360/pts.size());
            }
            inline double side() const {
                return pts[0].distance(pts[1]);
            }
    };

}