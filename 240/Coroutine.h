#ifndef _COROUTINE_H_
#define _COROUTINE_H_

#include <coroutine>
#include <memory>

template <class T, class INITIAL_SUSPEND = std::suspend_always>
class Coroutine
{
private:
    class Impl;

    struct promise_base
    {
        INITIAL_SUSPEND initial_suspend()
        {
            return INITIAL_SUSPEND{};
        }

        std::suspend_always final_suspend() noexcept
        {
            return {};
        }

        void unhandled_exception()
        {
            throw std::exception("unhandled exception");
        }
    };

    template <class R>
    struct promise_type_impl : public promise_base
    {
        R value;

        Coroutine get_return_object()
        {
            return Coroutine{ std::make_shared<Impl>(std::coroutine_handle<promise_type_impl>::from_promise(*this)) };
        }

        std::suspend_always yield_value(R&& value)
        {
            this->value = value;
            return {};
        }

        std::suspend_always yield_value(const R& value)
        {
            this->value = value;
            return {};
        }

        void return_value(R&& value)
        {
            this->value = value;
        }

        void return_value(const R& value)
        {
            this->value = value;
        }

    };

    template <>
    struct promise_type_impl<void> : public promise_base
    {
        Coroutine get_return_object()
        {
            return Coroutine{ std::make_shared<Impl>(std::coroutine_handle<promise_type_impl>::from_promise(*this)) };
        }

        void return_void()
        {
        }
    };

public:
    typedef promise_type_impl<typename T> promise_type;

public:
    Coroutine()
        : impl(nullptr)
    {
    }

    Coroutine(std::shared_ptr<Impl> impl)
        : impl(impl)
    {
    }

    Coroutine(const Coroutine& other)
        : impl(other.impl)
    {
    }

    bool operator()() const
    {
        return resume();
    }

    bool resume() const
    {
        if (true == done())
        {
            return false;
        }

        impl->handle.resume();

        return true;
    }

    promise_type& promise()
    {
        return impl->handle.promise();
    }

    bool done() const
    {
        if (nullptr == impl)
        {
            return true;
        }

        return !impl->handle || impl->handle.done();
    }

    std::coroutine_handle<promise_type> corotine_handle() const
    {
        return impl->handle;
    }

    Coroutine& operator = (const Coroutine& other)
    {
        impl = other.impl;
        return *this;
    }

    struct iterator
    {
        explicit iterator(Coroutine* coroutine)
            : coroutine(coroutine)
            , done(true)
        {
        }

        const T& operator* () const
        {
            return coroutine->promise().value;
        }

        iterator& operator++()
        {
            done = !coroutine->resume();
            return *this;
        }

        bool operator == (std::default_sentinel_t)
        {
            if(true == done && true == coroutine->done())
            {
                return true;
            }
            return false;
        }
    private:
        Coroutine* coroutine;
        bool done;
    };

    iterator begin()
    {
        if (nullptr == impl)
        {
            return iterator{ nullptr };
        }

        if (impl->handle)
        {
            impl->handle.resume();
        }

        return iterator{ this };
    }

    std::default_sentinel_t end()
    {
        return {};
    }
private:
    class Impl
    {
    public:
        Impl(std::coroutine_handle<promise_type> handle)
            : handle(handle)
        {
        }

        ~Impl()
        {
            if (true == (bool)handle)
            {
                handle.destroy();
            }
        }

        std::coroutine_handle<promise_type> handle;
    };

private:
    std::shared_ptr<Impl> impl;
};

#endif