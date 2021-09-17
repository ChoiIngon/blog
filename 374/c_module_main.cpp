// dllmain.cpp : DLL 애플리케이션의 진입점을 정의합니다.
// g++ -shared -fPIC -o libhelloworld_dll.so ./dllmain.cpp
#ifdef _MSC_VER
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

#include <iostream>
#include <vector>
#include <numeric>

extern "C"
{
    EXPORT int add(int a, int b)
    {
        std::cout << "call function " << __FUNCTION__ << "(" << a << ", " << b << ")" << std::endl;
        return a + b;
    }

    EXPORT void sub(double a, double b, double* result)
    {
        std::cout << "call function " << __FUNCTION__ << "(" << a << ", " << b << ")" << std::endl;
        *result = a - b;
    }

    // array parameter 사용
    EXPORT int accumulate(int* input, int size)
    {
        std::cout << "call function " << __FUNCTION__ << "(";
        for(int i=0; i<size; i++)
        {
            std::cout << *(input+i);
            if(i < size-1)
            {
                std::cout << ", ";
            }
        }
        std::cout << ")" << std::endl;

        std::vector<int> v(input, input + size);
        int result = std::accumulate(v.begin(), v.end(), 0u);
        return result;
    }

    struct Rect
    {
        int x;
        int y;
        int width;
        int height;
    };

    // 구조체 parameter
    EXPORT int getarea(Rect* r)
    {
        std::cout << "call function " << __FUNCTION__ << "(width:" << r->width << ", height:" << r->height << ")" << std::endl;
        return r->width * r->height;
    }
}