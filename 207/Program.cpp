#include <string>
#include <iostream>
#include "Delegate.h"

void foo1(const std::string& msg)
{
    std::cout << "\tfunction foo(" << msg << ")" << std::endl;
}

void foo2()
{
    std::cout << "\tfunction foo()" << std::endl;
}

void bar1(const std::string& msg)
{
    std::cout << "\tfunction bar(" << msg << ")" << std::endl;
}

void bar2()
{
    std::cout << "\tfunction bar()" << std::endl;
}

class Foo
{
public:
    void member_function1(const std::string& msg)
    {
        std::cout << "\tmember function of Foo class(" << msg << ")" << std::endl;
    }

    void member_function2()
    {
        std::cout << "\tmember function of Foo class()" << std::endl;
    }
};

class Bar
{
public:
    void member_function1(const std::string& msg)
    {
        std::cout << "\tmember function of Bar class(" << msg << ")" << std::endl;
    }

    void member_function2()
    {
        std::cout << "\tmember function of Bar class()" << std::endl;
    }
};

int main()
{
    Foo fooObj;
    Bar barObj;

    Delegate<> delegate_noparmeter;
    // Delegate delegate; C++17 부터는 인자가 없으면 <>를 아예 빼도 됨

    std::cout << "// delegate without parameter" << std::endl;
    delegate_noparmeter += foo2;
    delegate_noparmeter += bar2;
    delegate_noparmeter();
    std::cout << std::endl;

    Delegate<const std::string&> delegate;

    std::cout << "// add 'foo' function" << std::endl;
    delegate += foo1;
    delegate("Hello World");
    std::cout << std::endl;

    std::cout << "// add 'bar' function" << std::endl;
    delegate += bar1;
    delegate("Hello World");
    std::cout << std::endl;

    std::cout << "// add 'Foo::member_function' function" << std::endl;
    delegate += std::bind(&Foo::member_function1, &fooObj, std::placeholders::_1);
    delegate("Hello World");
    std::cout << std::endl;

    std::cout << "// add 'Bar::member_function' function" << std::endl;
    delegate += std::bind(&Bar::member_function1, &barObj, std::placeholders::_1);
    delegate("Hello World");
    std::cout << std::endl;

    std::cout << "// remove 'Bar::member_function' function" << std::endl;
    delegate -= std::bind(&Bar::member_function1, &barObj, std::placeholders::_1);
    delegate("Hello World");
    std::cout << std::endl;

    std::cout << "// remove 'foo' function" << std::endl;
    delegate -= foo1;
    delegate("Hello World");
    std::cout << std::endl;

    std::cout << "// remove 'bar' function" << std::endl;
    delegate -= bar1;
    delegate("Hello World");
    std::cout << std::endl;

    std::cout << "// remove 'Foo::member_function' function" << std::endl;
    delegate -= std::bind(&Foo::member_function1, &fooObj, std::placeholders::_1);
    delegate("Hello World");
    std::cout << std::endl;

    auto lambda = [](const std::string& msg) {
        std::cout << "\tlambda:" << msg << std::endl;
    };
    std::cout << "// add 'lambda' function" << std::endl;
    delegate += lambda;
    delegate("Hello World");
    std::cout << std::endl;

    std::cout << "// remove 'lambda' function" << std::endl;
    delegate -= lambda;
    delegate("Hello World");
    std::cout << std::endl;
}