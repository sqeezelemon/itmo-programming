#include <iterator>
#include <cstdlib>
#include <cassert>
#include <memory>

namespace stl {

namespace cb_meta {
    template <class Alloc>
    struct cb_nonconst_traits {
        typedef typename Alloc::value_type      value_type;
        typedef typename Alloc::size_type       size_type;
        typedef typename Alloc::pointer         pointer;
        typedef value_type&                     reference;
        typedef typename Alloc::difference_type difference_type;
    };

    template <class Alloc>
    struct cb_const_traits {
        typedef typename Alloc::value_type      value_type;
        typedef typename Alloc::size_type       size_type;
        typedef typename Alloc::const_pointer   pointer;
        typedef const    value_type&            reference;
        typedef typename Alloc::difference_type difference_type;
    };
};

template <class T>
class circular_buffer {
public:
    typedef T                                   value_type;
    typedef std::allocator<value_type>          alloc_type;
    typedef typename alloc_type::size_type      size_type;
    typedef typename alloc_type::pointer        pointer;
    typedef typename alloc_type::const_pointer  const_pointer;
    typedef value_type&                         reference;
    typedef const value_type&                   const_reference;

private:
    // The start of the internal buffer
    pointer b_begin;
    // The one-past-end address for the end of the internal buffer
    // i.e. the first address after the end of the buffer
    pointer b_end;
    // The leftmost element in the buffer (i.e. points to the last written)
    // So, for example, if we want to write to front, we need to decrement it first,
    // or else we'll overwrite the last written value
    pointer b_left;
    // The one-past-end address for the rightmost element in the internal buffer
    // Basically, it is always at the next cell we'll be interacting with
    // So, for example, when we want to write to the back, we just write to that address
    pointer b_right;
    // The size of the internal buffer
    size_type b_size;

    //////// POINTER ARITHMETIC

    // Inccrements the pointer within the boundaries of CB
    pointer incptr(pointer& p) const noexcept {
        if (++p == b_end)
            p = b_begin;
        return p;
    }

    // Decrements the pointer within the boundaries of CB
    pointer decptr(pointer& p) const noexcept {
        if (p == b_begin)
            p = b_end;
        return --p;
    }

    // Offsets the pointer within the boundaries of CB
    pointer addptr(pointer& p, const size_type ofs) const noexcept {
        return p + (
            (p + ofs >= b_end)
            ? (ofs - b_end + b_begin)
            : (ofs)
        );
    }

    // Offsets the pointer within the boundaries of CB
    pointer subptr(pointer p, const size_type ofs) const noexcept {
        return p - (
            (p - ofs < b_begin)
            ? (ofs - b_begin + b_end)
            : (ofs)
        );
    }

    //////// INTERNAL MEMORY MANAGEMENT

    void deleteptr(pointer ptr) {
        ptr->~value_type();
    }

    void moveptr(pointer from, pointer to) {
        *to = *std::move(from);
    }

public:

    //////// INITIALIZERS

    explicit circular_buffer():
        b_begin(nullptr), b_end(nullptr), b_left(nullptr), b_right(nullptr), b_size(0) {}
    
    explicit circular_buffer(const size_type sz) {
        b_begin = (pointer)malloc(sz*sizeof(value_type));
        b_end   = b_begin + sz;
        b_left  = b_begin;
        b_right = b_left;
        b_size  = 0;
    }

    template <class InputIterator>
    circular_buffer(InputIterator first, InputIterator last) {
        assign(first, last);
    }

    template <class InputIterator>
    void assign(InputIterator first, InputIterator last);

    ~circular_buffer() {
        if (!empty()) {
            do {
                deleteptr(b_left);
                b_left = incptr(b_left);

            } while (b_left != b_right);
        }

        free(b_begin);
    }

    circular_buffer& operator= (const circular_buffer&& other) {
        set_capacity(other.capacity());
        for (auto elem : other)
            push_back(elem);
    }


    //////// ITERATOR

    template <class cbType, class Traits>
    class cb_iterator {
    public:
        typedef std::allocator<cbType> alloc;
        typedef typename Traits::value_type      value_type;
        typedef typename Traits::size_type       size_type;
        typedef typename Traits::pointer         pointer;
        typedef typename Traits::reference       reference;
        typedef typename Traits::difference_type difference_type;

        using iterator_category = std::random_access_iterator_tag;
    private:
        // The pointer to which the iterator is pointing to
        pointer i_ptr;
        // How offset the iterator is from the start of the circular buffer
        // Mostly for differentiating between start and end in full buffers
        difference_type i_offs;
        // The start of the cb's internal buffer
        pointer b_begin;
        // The one-past-end address for the end of cb's internal buffer
        // i.e. the first address after the end of the buffer
        pointer b_end;
        // The pointer to the leftmost element in the buffer
        // (used for pointer difference and comparasion)
        pointer b_left;

        //////// POINTER ARITHMETIC

        // Inccrements the pointer within the boundaries of CB
        void incptr(pointer& p) const noexcept {
            if (++p == b_end)
                p = b_begin;
        }

        // Decrements the pointer within the boundaries of CB
        void decptr(pointer& p) const noexcept {
            if (p == b_begin)
                p = b_end;
            --p;
        }

        // Offsets the pointer within the boundaries of CB
        pointer addptr(pointer p, const size_type ofs) const noexcept {
            return p + (
                (p + ofs >= b_end)
                ? (ofs - b_end + b_begin)
                : (ofs)
            );
        }

        // Offsets the pointer within the boundaries of CB
        pointer subptr(pointer p, const size_type ofs) const noexcept {
            return p - (
                (p - ofs < b_begin)
                ? (ofs - b_begin + b_end)
                : (ofs)
            );
        }

        // How offset the iterator is from the leftmost element in the CB
        difference_type offset() const noexcept {
            return i_offs;
        }

    public:

        //////// INITIALIZERS

        cb_iterator(const pointer& ptr, const difference_type& offs, const pointer& begin, const pointer& end, const pointer& left):
            i_ptr(ptr), i_offs(offs), b_begin(begin), b_end(end), b_left(left) {}
        
        cb_iterator(const cb_iterator& it):
            i_ptr(it.i_ptr), i_offs(it.i_offs), b_begin(it.b_begin), b_end(it.b_end), b_left(it.b_left) {}
        

        //////// VALUE STUFF

        constexpr reference operator* () const noexcept {
            return *i_ptr;
        }

        constexpr pointer operator-> () const noexcept {
            return i_ptr;
        }


        //////// ITERATOR COMPARASION

        constexpr inline bool operator== (const cb_iterator& other) const noexcept {
            return (offset() == other.offset());
        }
        constexpr inline bool operator!= (const cb_iterator& other) const noexcept {
            return (offset() != other.offset());
        }

        constexpr inline bool operator<  (const cb_iterator& other) const noexcept {
            return (offset() < other.offset());
        }
        constexpr inline bool operator>  (const cb_iterator& other) const noexcept {
            return (offset() > other.offset());
        }

        constexpr inline bool operator<=  (const cb_iterator& other) const noexcept {
            return (offset() <= other.offset());
        }
        constexpr inline bool operator>=  (const cb_iterator& other) const noexcept {
            return (offset() >= other.offset());
        }


        //////// ITERATOR ARITHMETICS

        // Prefix
        cb_iterator& operator++ () noexcept {
            incptr(i_ptr);
            ++i_offs;
            return *this;
        }
        cb_iterator& operator-- () noexcept {
            decptr(i_ptr);
            --i_offs;
            return *this;
        }

        // Postfix
        cb_iterator operator++ (int) noexcept {
            iterator temp(*this);
            ++*this;
            return temp;
        }
        cb_iterator operator-- (int) noexcept {
            iterator temp(*this);
            --*this;
            return temp;
        }

        // Difference type arithmetics
        cb_iterator& operator += (const difference_type diff) noexcept {
            if (diff > 0)
                addptr(i_ptr, diff);
            else
                subptr(i_ptr, diff);
            i_offs += diff;
            return *this;
        }
        cb_iterator& operator -= (const difference_type diff) noexcept {
            if (diff < 0)
                addptr(i_ptr, diff);
            else
                subptr(i_ptr, diff);
            i_offs -= diff;
            return *this;
        }

        cb_iterator operator + (const difference_type diff) const noexcept {
            iterator temp(*this);
            temp += diff;
            return temp;
        }
        cb_iterator operator - (const difference_type diff) const noexcept {
            iterator temp(*this);
            temp -= diff;
            return temp;
        }

        cb_iterator operator + (const cb_iterator other) const noexcept {
            iterator temp(*this);
            temp += other.offset();
            return temp;
        }

        difference_type operator - (const cb_iterator other) const noexcept {
            return offset() - other.offset();
        }

        reference operator[] (const size_type ind) noexcept {
            pointer temp(i_ptr);
            addptr(temp, ind);
            return *temp;
        }
    };

    typedef cb_iterator<value_type, cb_meta::cb_nonconst_traits<alloc_type>> iterator;
    typedef cb_iterator<value_type, cb_meta::cb_const_traits<alloc_type>>    const_iterator;
    typedef std::reverse_iterator<iterator>       reverse_iterator;
    typedef std::reverse_iterator<const_iterator> const_reverse_iterator;

    //////// ITERATOR FUNCTIONS

    iterator begin() noexcept {
        return iterator(b_left, 0, b_begin, b_end, b_left);
    }
    iterator end()   noexcept {
        return iterator(b_right, b_size, b_begin, b_end, b_left);
    }

    const_iterator cbegin() const noexcept {
        return const_iterator(b_left, 0, b_begin, b_end, b_left);
    }
    const_iterator cend()   const noexcept {
        return const_iterator(b_right, b_size, b_begin, b_end, b_left);
    }

    reverse_iterator rbegin() noexcept {
        return std::reverse_iterator(end());
    }
    reverse_iterator rend()   noexcept {
        return std::reverse_iterator(begin());
    }

    const_reverse_iterator crbegin() const noexcept {
        return std::reverse_iterator(cend());
    }
    const_reverse_iterator crend() const noexcept {
        return std::reverse_iterator(cbegin());
    }

    //////// ELEMENT ACCESS
    reference at(const size_type ind) {
        assert(ind < size());
        return *addptr(b_left, ind);
    }
    reference inline operator[] (const size_type ind) {
        return at(ind);
    }

    reference rat(const size_type ind) {
        assert(ind < size());
        return *decptr(b_right, ind);
    }

    reference front() {
        assert(!empty());
        return *b_left;
    }

    reference back() {
        assert(!empty());
        pointer temp(b_right);
        decptr(temp);
        return *(temp);
    }

    //////// CAPACITY

    inline bool empty() {
        return b_size == 0;
    }

    inline bool full() {
        return size() == capacity();
    }

    inline size_type size() {
        return b_size;
    }

    inline size_type capacity() {
        return b_end - b_begin;
    }

private:
    // Reallocates the buffer to a new place with a fixed size
    // Requires that sz is GREATER OR EQUAL than b_size
    // Thus, deleting of excess elements should be handled before calling it
    void reallocate_buf(const size_type sz) {
        pointer newbuff = (pointer)malloc(sz*sizeof(value_type));

        pointer copyptr = b_left;
        for (size_type i = 0; i < size(); i++) {
            moveptr(copyptr, &newbuff[i]);
            incptr(copyptr);
        }

        free(b_begin);
        b_begin = newbuff;
        b_end   = b_begin + sz;
        b_left  = b_begin;
        b_right = (full()) ? (b_left) : (b_left  + size());
    }

public:
    void set_capacity(const size_type sz) {
        for (auto i = sz; i < size(); i++) {
            pop_back();
        }
        reallocate_buf(sz);
    }
    void rset_capacity(const size_type sz) {
        for (auto i = sz; i < size(); i++) {
            pop_front();
        }
        reallocate_buf(sz);
    }

    //////// BUFFER MODIFICATION

    void pop_front() {
        assert(!empty());
        deleteptr(b_left);
        incptr(b_left);
        --b_size;
    }

    void pop_back() {
        assert(!empty());
        decptr(b_right);
        deleteptr(b_right);
        --b_size;
    }
    // Adds to the front of the circular buffer
    // RESIZES IF NEEDED
    void push_front(const value_type val) {
        if (full())
            set_capacity((b_size == 0) ? 1 : b_size*2);
        write_front(val);
    }
    // Adds to the back of the circular buffer
    // RESIZES IF NEEDED
    void push_back(const value_type val) {
        if (full())
            set_capacity((b_size == 0) ? 1 : b_size*2);
        write_back(val);
    }

    // Adds to the front of the circular buffer and
    // DOESN'T RESIZE, WILL OVERWRITE IF FULL
    void write_front(const value_type val) {
        decptr(b_left);
        // Synopsis: decrements b_left, then writes to it
        if (full()) {
            if (empty())
                return;
            *b_left = val;
            b_right = b_left;
        } else {
            *b_left = val;
            ++b_size;
        } 
    }
    // Adds to the front of the circular buffer and
    // DOESN'T RESIZE, WILL OVERWRITE IF FULL
    void write_back(const value_type val) {
        // Synopsis: write to b_right, then increment it
        if (full()) {
            if (empty())
                return;
            *b_right = val;
            incptr(b_right);
            b_left = b_right;
        } else {
            *b_right = val;
            incptr(b_right);
            ++b_size;
        } 
    }

    //////// OTHER STUFF

    inline bool linear() {
        return (b_left < b_right);
    }

    pointer linearize() {
        reallocate_buf(size());
        return b_begin;
    }
};

}