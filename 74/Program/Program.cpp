#ifdef _MSC_VER
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

#include <iostream>
#include <string>
#include <Windows.h>

extern "C"
{
    EXPORT void ExportFunctionFromEXE(const std::string& callFrom)
    {
        std::cout << "Exe export function called From " << callFrom << std::endl;
    }
}

int main()
{
    HMODULE hModule = LoadLibrary(L"DLL.dll");
    if(NULL == hModule)
    {
        return -1;
    }

    typedef void (*FuncPtrT)(const std::string&);
    FuncPtrT pFunc = (FuncPtrT)::GetProcAddress(hModule, "ExportFunctionFromDLL");
    if(NULL == pFunc)
    {
        return -1;
    }

    pFunc("DLL");

    return 0;
}