#pragma once

#include <iostream>

namespace Template {
    template <class T>
    struct Foo
    {
        void print(T t)
        {
            std::cout << t << std::endl;
        }
    };
}