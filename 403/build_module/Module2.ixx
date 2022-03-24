module;
#include <iostream>

export module Module2;

namespace Template {
    export template <class T>
        struct Foo
    {
        void print(T t)
        {
            std::cout << t << std::endl;
        }
    };
}