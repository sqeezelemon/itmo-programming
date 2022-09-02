#pragma once
#include <memory>
// Only for std::sort because I'm lazy
#include <algorithm>

#define SEGALLOC_DEBUG 1

#if SEGALLOC_DEBUG
#define SEGALLOC_DPRINT(...) printf(__VA_ARGS__);
#else
#define SEGALLOC_DPRINT(...)
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
    // Start of the data buffer, aka the address of the first block
    pointer   start_ptr;
    // Amount of blocks
    size_type block_count;
    // Byte size of a single block
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
        return (start_ptr + index*block_capacity());
    }

    // Returns how much of T objects a block can hold
    size_type block_capacity() const noexcept {
        return block_size/value_size();
    }

    bool contains(const pointer p) {
        return (p >= start_ptr && p < end_ptr());
    }

    //////// INIT / DEINIT

    __bucket_manager():
        start_ptr(nullptr), block_size(0), block_count(0), available(0), table(nullptr) {}

    __bucket_manager(const pointer& ptr, const size_type& b_count, const size_type& b_size):
        start_ptr(ptr), block_size(b_size), block_count(b_count), available(b_count) {
        table = (uint8_t*)malloc((block_count+7)/8);

        SEGALLOC_DPRINT("__bucket_manager(%p) - Created table @ %p\n", this, table);

        for (int i = 0; i < block_count; i++)
            set_table(i, true);
    }

    __bucket_manager(const __bucket_manager<T>& other):
        start_ptr(other.start_ptr), block_size(other.block_size),
        block_count(other.block_count), available(other.block_count) {
            // Copy table
            table = (uint8_t*)malloc((block_count+7)/8);
            SEGALLOC_DPRINT("__bucket_manager(%p) - Copying table @ %p -> %p\n", this, other.table, table);
            for (int i = 0; i < (block_count+7)/8; i++)
                table[i] = other.table[i];
        }
    
    ~__bucket_manager() {
        if (table != nullptr) {
            SEGALLOC_DPRINT("__bucket_manager(%p) - Freeing table @ %p\n", this, table);
            free(table);
        }
    }

    //////// SECONDARY FUNCTIONS

    bool full() const noexcept {
        return (available == 0);
    }

    // returns the amount if T objects (NOT BYTESIZE!!!) that could fit inside a block
    size_type max_size() const noexcept {
        return (block_capacity() * (available != 0));
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
        // Check that we have the space
        if (available == 0 || n > max_size())
            throw std::bad_alloc();
        
        // Find an available block
        size_type block_index;
        for (block_index = 0; block_index < block_count; block_index++) {
            if (table[block_index/8] == 0) {
                block_index += 8;
                continue;
            }
            if (get_table(block_index))
                break;
        }

        // Boring stuff
        set_table(block_index, false);
        --available;

        #if SEGALLOC_DEBUG
        printf("__bucket_manager(%p) - Allocated block %lu\n", this, block_index);
        printf("BLOCK\tOFFSET\tPOINTER\tAVAILABLE?\n");
        for (int i = 0; i < block_count; i++)
            printf("%d\t%lu\t%p\t%d\n", i, (uint8_t*)block_pointer(i)-(uint8_t*)start_ptr, block_pointer(i), get_table(i));
        #endif

        return block_pointer(block_index);
    }

    void deallocate(const pointer& p, const size_type& n) {
        if (
            (p - start_ptr) % block_capacity() != 0 // Pointer is not aligned
            || !contains(p)  // Pointer is outside of bounds
        ) {
            SEGALLOC_DPRINT("__bucket_manager(%p) - Failed to deallocate pointer:\n", this);
            SEGALLOC_DPRINT("START:\t%p\t%lu\n", start_ptr, (uint8_t*)p-(uint8_t*)start_ptr);
            SEGALLOC_DPRINT("PTR:\t%p\t%d\n",   p,         0);
            SEGALLOC_DPRINT("END:\t%p\t%lu\n",   end_ptr(), (uint8_t*)p-(uint8_t*)end_ptr());
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
        printf("__bucket_manager(%p) - Deallocated block %lu\n", this, block_index);
        printf("BLOCK\tOFFSET\tPOINTER\tAVAILABLE?\n");
        for (int i = 0; i < block_count; i++)
            printf("%d\t%lu\t%p\t%d\n", i, (uint8_t*)block_pointer(i)-(uint8_t*)start_ptr, block_pointer(i), get_table(i));
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
        
        // Copy table
        free(table);
        table = (uint8_t*)malloc((block_count+7)/8);
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

// Struct for defining a block, used in bucket_allocator
template <size_t count, size_t size>
struct bucket_traits {
    constexpr static size_t block_count() noexcept {
        return count;
    }

    constexpr static size_t block_size() noexcept {
        return size;
    }
};


// Allocates buckets with blocks of different sizes
// It is recommended that alc::bucket_traits are used,
// however, any literal type which has these 2 functions is allowed:
// * size_t ::block_count()  - amount of blocks inside the bucket
// * size_t ::block_size()   - amount of items in each block (NOT BYTESIZE!!!!)
template<class T, typename... buckets>
class bucket_allocator: std::allocator<T> {
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
    // Bucket managers for every bucket
    bucket_manager* managers;
    // Pointer to the start of allocator's internal buffer
    pointer         data;
    // How much T objects can the biggest available block hold
    size_type       max_block;

    size_type value_size() const noexcept {
        return bucket_manager::value_size();
    }

    size_type buckets_count() const noexcept {
        return sizeof...(buckets);
    }

public:

    //////// INIT / DEINIT

    bucket_allocator() {
        // Step 1: create bucket_manager's
        managers = (bucket_manager*)malloc(sizeof...(buckets)*sizeof(bucket_manager));
        bucket_manager* curr_manager = managers;

        // Loads of weird syntax to iterate over parameter pack
        size_t counts[] = {buckets::block_count()...};
        size_t sizes[]  = {buckets::block_size()...};
        size_t alloc_size = 0;
        for (int i = 0; i < sizeof...(buckets); i++) {
            alloc_size += sizes[i]*counts[i];
            new (curr_manager) bucket_manager(nullptr, counts[i], sizes[i]*value_size());
            ++curr_manager;
        }


        // Step 2: sort bucket managers for easier time
        auto manager_comp = [] (const bucket_manager& l, const bucket_manager& r) {
            return l.max_size() > r.max_size();
        };
        std::sort(managers, managers+sizeof...(buckets), manager_comp);

        // Step 3: allocate and set pointers
        // Allocating the buffer
        alloc_size *= value_size();
        data = (pointer)malloc(alloc_size);
        pointer currptr = data;
        SEGALLOC_DPRINT("bucket_allocator(%p) - Allocated %lu Byte buffer @ %p\n", this, alloc_size, data);

        // Setting pointers accordingly
        for (int i = 0; i < buckets_count(); i++) {
            SEGALLOC_DPRINT("bucket_allocator(%p) - Gave managers[%d] pointer %p (%lu Bytes from start)\n", this, i, currptr, (uint8_t*)currptr-(uint8_t*)data);
            managers[i].start_ptr = currptr;
            currptr += managers[i].block_capacity()*managers[i].block_count;
        }

        max_block = managers[0].max_size();
    }

    ~bucket_allocator() {
        for (int i = 0; i < buckets_count(); i++)
            managers[i].~__bucket_manager();
        free(data);
        free(managers);
    }

private:
    // Searches for the most appropriate bucket for the size
    int search_bucket(size_type sz) {
        int l = 0;
        int r = buckets_count() - 1;
        while (l < r) {
            int mid = (l+r)/2;
            if (managers[mid].block_capacity() == sz) {
                return mid;
            } else if (managers[mid].block_capacity() < sz) {
                r = --mid;
            } else {
                l = ++mid;
            }
        }
        return l;
    }

    // Locates the bucket in which the pointer is
    size_type locate_bucket(pointer p) {
        int l = 0;
        int r = buckets_count() - 1;
        while (l < r) {
            int mid = (l+r)/2;
            if (managers[mid].contains(p)) {
                return mid;
            } else if (p > managers[mid].start_ptr) {
                l = ++mid;
            } else {
                r = mid;
            }
        }
        return l;
    }

    // One-past-end pointer to the end of the buffer
    pointer data_end() {
        return managers[buckets_count()-1].start_ptr + managers[buckets_count()-1].block_capacity()*managers[buckets_count()-1].block_count;
    }

public:
    
    pointer allocate(size_type n) {
        if (max_size() < n)
            throw std::bad_alloc();
        // Locate closest by size
        int bucket_index = search_bucket(n);

        // Move to bigger allocs until a free one is find
        while (
            (managers[bucket_index].full() || (managers[bucket_index].block_capacity() < n))
               && bucket_index >= 0
            ) {
            --bucket_index;
        }
        
        // Ideally should never happen
        if (bucket_index < 0)
            throw std::bad_alloc();
        
        SEGALLOC_DPRINT("bucket_allocator(%p) - Allocating %lu Bytes in bucket %d\n", this, n, bucket_index);
        pointer ptr = managers[bucket_index].allocate(n);

        // Check if we might have a new max_block
        if (managers[bucket_index].block_capacity() == max_block && managers[bucket_index].full()) {

            // Check left
            for (int i = bucket_index-1; i >= 0; i--) {
                // Means there's another bucket of size max_size
                if (!managers[i].full())
                    return ptr;
            }

            // Check right
            for (int i = bucket_index+1; i < buckets_count(); i++) {
                // Means there's another chunk of size max_size
                if (!managers[i].full()) {
                    max_block = managers[i].block_capacity();
                    return ptr;
                }
            }
            max_block = 0;
        }

        return ptr;
    }
    
    void deallocate(pointer p, size_type n) {
        // Check that it actually belongs in the allocator
        if (p < data || p >= data_end()) {
            return;
        }

        // Deallocating
        int bucket_index = locate_bucket(p);
        SEGALLOC_DPRINT("bucket_allocator(%p) - Deallocating %p in bucket %d (%lu Bytes from start)\n", this, p, bucket_index, (uint8_t*)p-(uint8_t*)data);
        managers[bucket_index].deallocate(p, n);

        // Check if we got a new max_block
        if (!managers[bucket_index].full() && managers[bucket_index].block_capacity() > max_block) {
            max_block = managers[bucket_index].block_capacity();
        }
    }


    inline size_type max_size() const noexcept {
        return max_block;
    }
};

}