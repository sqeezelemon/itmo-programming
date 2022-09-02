namespace stl {

template <class InputIterator, class Predicate>
constexpr bool all_of(InputIterator first, InputIterator last, Predicate pred) {
    while (first != last) {
        if (!pred(*first))
            return false;
        ++first;
    }
    return true;
}

template <class InputIterator, class Predicate>
constexpr bool any_of(InputIterator first, InputIterator last, Predicate pred) {
    while (first != last) {
        if (pred(*first))
            return true;
        ++first;
    }
    return false;
}

template <class InputIterator, class Predicate>
constexpr bool none_of(InputIterator first, InputIterator last, Predicate pred) {
    while (first != last) {
        if (pred(*first))
            return false;
        ++first;
    }
    return true;
}

template <class InputIterator, class Predicate>
constexpr bool one_of(InputIterator first, InputIterator last, Predicate pred) {
    int count = 0;
    while (first != last) {
        count += pred(*first);
        ++first;
    }
    return (count == 1);
}

template <class InputIterator, class Predicate>
constexpr bool is_sorted(InputIterator first, InputIterator last, Predicate pred) {
    InputIterator second = first;
    ++second;
    while (second != last) {
        if (!pred(*first, *second))
            return false;
        ++first;
        ++second;
    }
    return true;
}

template <class InputIterator, class Predicate>
constexpr bool is_partitioned(InputIterator first, InputIterator last, Predicate pred) {
    InputIterator ans;
    bool firstVal = pred(*first);
    bool hasChanged = false;
    ++first;
    while (first != last) {
        bool currVal = pred(*first);
        if (currVal == firstVal && hasChanged)
            return false;
        if (currVal != firstVal)
            hasChanged = true;
    }
    return true;
}

template <class InputIterator, class T>
constexpr InputIterator find_not(InputIterator first, InputIterator last, const T& value) {
    while (first != last) {
        if (!(*first == value))
            break;
        ++first;
    }
    return first;
}

template<class BidirectionalIterator, class T>
constexpr BidirectionalIterator find_backward(BidirectionalIterator first, BidirectionalIterator last, const T& value) {
    while (last != first) {
        if (*last == value)
            break;
        --last;
    }
    return last;
}

template <class BidirectionalIterator, class Predicate>
constexpr bool is_palindrome(BidirectionalIterator first, BidirectionalIterator last, Predicate pred) {
    while (first > last) {
        if (pred(*first, *last))
            return false;
        ++first;
        --last;
    }
    return true;
}

}