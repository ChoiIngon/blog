// dllmain.cpp : DLL 애플리케이션의 진입점을 정의합니다.
#include "pch.h"

#ifdef _MSC_VER
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

#include <iostream>
#include <string>

extern "C"
{
    EXPORT void ExportFunctionFromDLL(const std::string& callFrom)
    {
        std::cout << "DLL export function called From " << callFrom << std::endl;
        HINSTANCE hExe = GetModuleHandle(NULL);

        typedef void (*FuncPtrT)(const std::string&);
        FuncPtrT pFunc = (FuncPtrT)::GetProcAddress(hExe, "ExportFunctionFromEXE");
        if (pFunc)
        {
            (*pFunc)("DLL");
        }
        else
        {
            std::cerr << "Not found proper " << callFrom << " function" << std::endl;
        }
    }
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

