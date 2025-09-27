#include <string>
#include <iostream>
#include "Delegate.h"

std::size_t free_function(const std::string& msg)
{
    std::cout << "\tgreeting message \'" << msg << "\' from " << __FUNCTION__ << std::endl;
    return msg.length();
}

class Foo
{
public:
    std::size_t member_function(const std::string& msg)
    {
        std::cout << "\tgreeting message \'" << msg << "\' from " << __FUNCTION__ << std::endl;
        return msg.length();
    }

    static std::size_t static_function(const std::string& msg)
    {
        std::cout << "\tgreeting message \'" << msg << "\' from " << __FUNCTION__ << std::endl;
        return msg.length();
    }
};

int main()
{
    Delegate<std::size_t(const std::string&)> delegate;

    std::cout << "push_back 'free_function' function" << std::endl;
    delegate += free_function;

    std::cout << "push_back 'lambda_function' function" << std::endl;
    auto lambda_function = [](const std::string& msg)
    {
        std::cout << "\tgreeting message \'" << msg << "\' from " << __FUNCTION__ << std::endl;
        return msg.length();
    };
    delegate += lambda_function;

    std::cout << "push_back 'Foo::member_function' function" << std::endl;
    Foo foo;
    delegate.push_back(&foo, &Foo::member_function);
    
    std::cout << "push_back 'Foo::static_function' function" << std::endl;
    delegate += &Foo::static_function;

    std::cout << "size of delegate: " << delegate.size() << std::endl;
    std::cout << std::endl;

    std::string greeting = "Hello World";
    std::cout << "call operator(" << greeting << ") " << std::endl;
    delegate(greeting);
    std::cout << std::endl;
    
    std::cout << "erase 'free_function' function" << std::endl;
    delegate -= free_function;

    std::cout << "erase 'lambda_function' function" << std::endl;
    delegate -= lambda_function;

    std::cout << "erase 'Foo::member_function' function" << std::endl;
    delegate.erase(&foo, &Foo::member_function);
 
    std::cout << "erase 'Foo::static_function' function" << std::endl;
    delegate -= &Foo::static_function;

    std::cout << std::endl;
    std::cout << "size of delegate: " << delegate.size() << std::endl;
    
    return 0;
}