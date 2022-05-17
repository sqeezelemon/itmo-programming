#pragma once
#include <memory>

#define SEGALLOC_DEBUG 1

#if SEGALLOC_DEBUG
#define SEGALLOC_DPRINT(...) printf(__VA_ARGS__);
#else
#define SEGALLOC_DPRINT(x)
#endif

namespace alc {


// Memory-map for a bucket
// Relies on outside objects for freeing it's buffer pointer
template <class T>
class __bucket_manager {
//////// TYPEDEFS
public:
    typedef T                 value_type;
    typedef value_type*       pointer;
    typedef const value_type* const_pointer;
    typedef value_type&       reference;
    typedef const value_type& const_reference;
    typedef std::size_t       size_type;
    typedef std::ptrdiff_t    difference_type;

//////// INTERNAL VALUES
private:
    // Start of the data buffer, aka the address of the first block
    pointer   start_ptr;
    // Amount of blocks
    size_type block_count;
    // Bytesize of a single block
    size_type block_size;
    // Amount of available blocks
    size_type available;
    // Bitmap for block availability
    // true == block available
    uint8_t*  table;

//////// INTERNAL FUNCTIONS
    // Sets the corresponding bit in the table to val
    void set_table(size_type ind, bool val) {
        table[ind/8] = (
            table[ind/8] & ~(0x80 >> ind%8) // 1 - zero out the index
            | (val << (7-ind%8))            // 2 - set bit
        );
    }
    // Gets the bit from the table
    bool get_table(size_type ind) const {
        return ((table[ind/8] >> (7 - ind%8)) & 1);
    }

    // One-past-end pointer to the end of the buffer
    pointer end_ptr() const noexcept {
        return block_pointer(block_count);
    }

    // Pointer to the start of a block
    pointer block_pointer(const size_type& index) const noexcept {
        return (start_ptr + index*block_size);
    }

    #if SEGALLOC_DEBUG
    void table_debug_print() {
        printf("BLOCK\tSTART PTR\tAVAILABLE?\n");
        for (int i = 0; i < block_count; i++)
            printf("%d\t%p\t%d\n", i, start_ptr+(i*block_size), get_table(i));
    }
    #endif

public:

    //////// INIT / DEINIT

    __bucket_manager():
        start_ptr(nullptr), block_size(0), block_count(0), available(0), table(nullptr) {}

    __bucket_manager(const pointer& ptr, const size_type& b_count, const size_type& b_size):
        start_ptr(ptr), block_size(b_size), block_count(b_count), available(b_count) {
        table = (uint8_t*)malloc((block_count+7)/8);

        SEGALLOC_DPRINT("__bucket_manager::__bucket_manager - allocated table at %p\n", table);
        
        for (int i = 0; i < block_count; i++)
            set_table(i, true);
    }

    __bucket_manager(const __bucket_manager<T>& other):
        start_ptr(other.start_ptr), block_size(other.size),
        block_count(other.block_count), available(other.block_count) {
            SEGALLOC_DPRINT("__bucket_manager::copy_constructor - copying table from %p\n", other.table);
            table = (uint8_t*)malloc((block_count+7)/8);
            SEGALLOC_DPRINT("__bucket_manager::operator= - allocated table at %p\n", table);
            for (int i = 0; i < (block_count+7)/8; i++)
                table[i] = other.table[i];
        }
    
    ~__bucket_manager() {
        if (table != nullptr) {
            SEGALLOC_DPRINT("__bucket_manager::~__bucket_manager - freeing table at %p\n", table);
            free(table);
        }
    }

    //////// SECONDARY FUNCTIONS

    bool full() const noexcept {
        return (available == 0);
    }

    size_type max_size() const noexcept {
        return (block_size/value_size() * (available != 0));
    }

    // Function for ease of working with T == void
    constexpr static size_type value_size() {
        if (std::is_same<T, void>::value) {
            return 1;
        } else {
            return sizeof(value_type);
        }
    }

    //////// ALLOCATION

    pointer allocate(const size_type& n) {
        SEGALLOC_DPRINT("__bucket_manager::allocate - requested %lu Bytes (max %lu)", n, max_size());
        if (available == 0 || n > max_size())
            throw std::bad_alloc();
        
        size_type block_index;
        for (block_index = 0; block_index < block_count; block_index++) {
            if (table[block_index/8] == 0) {
                block_index += 8;
                continue;
            }
            if (get_table(block_index))
                break;
        }
        set_table(block_index, false);
        --available;

        #if SEGALLOC_DEBUG
        printf("ALLOCATED block %lu (%p)\n", block_index, block_pointer(block_index));
        table_debug_print();
        #endif

        return block_pointer(block_index);
    }

    void deallocate(const pointer& p, const size_type& n) {
        if (
            (p - start_ptr) % block_size != 0      // Pointer is not aligned
            || (p < start_ptr) || (p >= end_ptr()) // Pointer is outside of bounds
        ) {
            return;
        }
        size_type block_index = (p-start_ptr)/block_size;

        // Check if pointer (block) is already free
        if (get_table(block_index))
            return;

        // Free pointer
        set_table(block_index, true);
        ++available;

        #if SEGALLOC_DEBUG
        printf("DEALLOCATED block %lu (%p)\n", block_index, p);
        table_debug_print();
        #endif
    }

    //////// OPERATORS

    friend bool operator== (const __bucket_manager& l, const __bucket_manager& r) {
        return (
            l.start_ptr   == r.start_ptr &&
            l.block_count == r.block_count &&
            l.block_size  == r.block_size &&
            l.available   == r.available &&
            l.table       == r.table
        );
    }

    friend bool operator!= (const __bucket_manager& l, const __bucket_manager& r) {
        return !(l==r);
    }

    __bucket_manager& operator= (const __bucket_manager& other) {
        start_ptr   = other.start_ptr;
        block_count = other.block_count;
        block_size  = other.block_size;
        available   = other.available;

        SEGALLOC_DPRINT("__bucket_manager::operator= - freeing table at %p\n", table);
        free(table);
        table       = (uint8_t*)malloc((block_count+7)/8);
        SEGALLOC_DPRINT("__bucket_manager::operator= - allocated table at %p\n", table);
        for (int i = 0; i < (block_count+7)/8; i++)
            table[i] = other.table[i];
        
        return *this;
    }
};


// Allocates same-size blocks that it then can give out
// T - the type for the allocator
// block_size - the amount of T objects inside a block (NOT BYTESIZE!!!)
// block_count - Amount of blocks
template <class T, size_t block_count, size_t block_size>
class block_allocator: std::allocator<T> {
//////// TYPEDEFS
public:
    typedef T                 value_type;
    typedef value_type*       pointer;
    typedef const value_type* const_pointer;
    typedef value_type&       reference;
    typedef const value_type& const_reference;
    typedef std::size_t       size_type;
    typedef std::ptrdiff_t    difference_type;

private:
    typedef __bucket_manager<T> bucket_manager;
    bucket_manager manager;
    pointer        data;

    size_type value_size() const noexcept {
        return bucket_manager::value_size();
    }

public:
    block_allocator() {
        size_type alloc_size = block_count*block_size*value_size();
        data = (pointer)malloc(alloc_size);
        SEGALLOC_DPRINT("block_allocator::block_allocator - allocated %lu Bytes at %p\n", alloc_size, data);
        manager = bucket_manager(data, block_count, block_size*value_size());
    }

    ~block_allocator() {
        free(data);
    }

    inline pointer allocate(const size_type& n) {
        return manager.allocate(n);
    }

    inline void deallocate(const pointer& p, const size_type& n) {
        manager.deallocate(p,n);
    }

    inline size_type max_size() const {
        return manager.max_size();
    }
};

}