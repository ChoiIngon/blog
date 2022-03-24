module;
#include <iostream>

export module Module1;

namespace Template {
    export template <typename T, typename T2>
        auto sum(T fir, T2 sec) {
        return fir + sec;
    }
}