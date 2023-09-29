// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include <cstdio>
#include<iostream>
#include<Windows.h>
using namespace std;
typedef unsigned char uch;
BYTE hookcode[10] = { 0x39, 0xd2 };
BYTE rawcode[10];
void hook()
{
    DWORD64 codeAdress;
    DWORD64 codeOffset = 0x18e7; // test edx, edx
    HMODULE hModule = GetModuleHandle(nullptr);
    DWORD_PTR baseAddress = reinterpret_cast<DWORD_PTR>(hModule);
    codeAdress = (DWORD64)baseAddress + codeOffset;

    printf("\nhook code address : % llXH\n", codeAdress);

    // 获取进程句柄
    HANDLE hProcess = GetCurrentProcess();

    // 修改内存保护权限
    VirtualProtect((LPVOID)codeAdress, 2, PAGE_EXECUTE_READWRITE, 0);

    //SIZE_T numberOfBytesWritten;

    // 读取原test shellcode，进行确认
    if(ReadProcessMemory(hProcess, (LPVOID)codeAdress, rawcode, 2, NULL))
    {
        for (int i = 1; i >= 0; i--)printf(" 0x%02x ", rawcode[i]);
        printf("\n");
    }
    else
    {
        printf(" read failed !\n");
    }
    
    
    // hook test code
    if (WriteProcessMemory(hProcess, (LPVOID)codeAdress, hookcode, 2, NULL))
    {
        printf(" hook success !\n");
    }
    else
    {
        printf(" hook failed !\n");
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
    {
        printf("Succeed to inject process!\n");
        MessageBoxA(0, "注入进程成功！", "", 0);
        hook();
        break;
    }
    case DLL_THREAD_ATTACH:
    {
        break;
    }
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}


