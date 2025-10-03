#pragma once

#include <list>
#include <functional>

template <class T>
class Delegate;

template <int>
struct variadic_placeholder {};

namespace std {
    template <int N>
    struct is_placeholder<variadic_placeholder<N>>
        : integral_constant<int, N + 1>
    {
    };
}

template <class R, class... Args>
class Delegate<R(Args...)>
{
public:
    template<class Class>
    Delegate& push_back(R(Class::*memfunc_ptr)(Args...), Class* obj)
    {
        this->push_back(this->variadic_bind(memfunc_ptr, obj));
        return *this;
    }

    template<class Class>
    Delegate& erase(R(Class::*memfunc_ptr)(Args...), Class* obj)
    {
        this->erase(this->variadic_bind(memfunc_ptr, obj));
        return *this;
    }

    Delegate& push_back(std::function<R(Args...)> const& func)
    {
        functions.push_back(func);
        return *this;
    }

    Delegate& erase(std::function<R(Args...)> const& func)
    {
        R(* const* func_ptr)(Args...) = func.template target<R(*)(Args...)>();

        if (nullptr == func_ptr) // member function
        {
            const std::size_t func_hash = func.target_type().hash_code();

            for (auto itr = functions.begin(); itr != functions.end(); ++itr)
            {
                if (func_hash == (*itr).target_type().hash_code())
                {
                    functions.erase(itr);
                    break;
                }
            }
        }
        else
        {
            for (auto itr = functions.begin(); itr != functions.end(); ++itr)
            {
                R(* const* target_ptr)(Args...) = itr->template target<R(*)(Args...)>();
                if (nullptr != target_ptr && *target_ptr == *func_ptr)
                {
                    functions.erase(itr);
                    break;
                }
            }
        }

        return *this;
    }

    Delegate& operator += (std::function<R(Args...)> const& func)
    {
        return push_back(func);
    }

    Delegate& operator -= (std::function<R(Args...)> const& func)
    {
        return erase(func);
    }

    R operator()(Args... args)
    {
        if (true == functions.empty())
        {
            return R{};
        }

        auto end = functions.end();
        --end; // Points to the last element
        for (auto itr = functions.begin(); itr != end; ++itr)
        {
            (*itr)(args...);
        }

        return (*end)(args...);
    }

    auto begin() noexcept
    {
        return functions.begin();
    }

    auto end() noexcept
    {
        return functions.end();
    }

    void clear() noexcept
    {
        functions.clear();
    }

    auto size() const noexcept
    {
        return functions.size();
    }
private:
    template <class Class, size_t... Is>
    auto variadic_bind(std::index_sequence<Is...>, R(Class::* memfunc_ptr)(Args...), Class* obj) {
        return std::bind(memfunc_ptr, obj, variadic_placeholder<Is>{}...);
    }

    template <typename Class>
    auto variadic_bind(R(Class::* memfunc_ptr)(Args...), Class* obj) {
        return variadic_bind(std::make_index_sequence<sizeof...(Args)>{}, memfunc_ptr, obj);
    }

    std::list<std::function<R(Args...)>> functions;
};