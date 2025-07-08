// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include <stdio.h>
#include "MinHook.h"

typedef void (*OriginalFuncType)(void);
OriginalFuncType originalFunc1 = NULL;

OriginalFuncType originalFunc2 = NULL;

__declspec(naked) void HookFunction()
{
    __asm {
        pushad
        pushfd

        cmp eax, 0x1F
        jne call_original

        xor eax, eax

    call_original:
        popfd
        popad
        jmp originalFunc1
    }
}

__declspec(naked) void HookFunction2()
{
    __asm {
        pushad
        pushfd

        cmp eax, 0x1F
        jne call_original2

        xor eax, eax

    call_original2:
        popfd
        popad
        jmp originalFunc2
    }
}

bool InitHook()
{
    if (MH_Initialize() != MH_OK)
    {
        printf("MinHook init failed\n");
        return false;
    }

    // 不倒翁
    // ZC.H+1DF763 - 88 42 0C              - mov [edx+0C],al
    // ZC.H+1DF766 - 8B 45 C8              - mov eax,[ebp-38]
    // ZC.H+1DF769 - 8B 55 FC              - mov edx,[ebp-04]
    // ZC.H+1DF76C - 89 82 78010000        - mov [edx+00000178],eax
    // ZC.H+1DF772 - 8B 45 FC              - mov eax,[ebp-04]
    // ZC.H+1DF775 - 66 BE FFFF            - mov si,FFFF { 65535 }
    // ZC.H+1DF779 - E8 AE60E2FF           - call ZC.H+582C

    DWORD targetAddress1 = 0x1DF76C + (DWORD)GetModuleHandle(L"ZC.H");
    if (MH_CreateHook((LPVOID)targetAddress1, HookFunction, (LPVOID*)&originalFunc1) != MH_OK)
    {
        printf("hook 1 fail\n");
        return false;
    }

    // 显血
    // ZC.H + 3B1E3 - 8B 03 - mov eax, [ebx]
    // ZC.H + 3B1E5 - E8 FAFEFFFF - call ZC.H + 3B0E4
    // ZC.H + 3B1EA - 8B 43 04 - mov eax, [ebx + 04]
    // ZC.H + 3B1ED - 8B 04 B0 - mov eax, [eax + esi * 4]
    // ZC.H + 3B1F0 - 5E - pop esi
    // ZC.H + 3B1F1 - 5B - pop ebx
    // ZC.H + 3B1F2 - C3 - ret
    DWORD targetAddress2 = 0x3B1ED + (DWORD)GetModuleHandle(L"ZC.H");
    if (MH_CreateHook((LPVOID)targetAddress2, HookFunction2, (LPVOID*)&originalFunc2) != MH_OK)
    {
        printf("hook 2 fail\n");
        return false;
    }

    if (MH_EnableHook(MH_ALL_HOOKS) != MH_OK)
    {
        printf("enable all hooks fail\n");
        return false;
    }

    printf("all hooks installed\n");
    return true;
}

void UninitHook()
{
    // 禁用并清理钩子
    MH_DisableHook(MH_ALL_HOOKS);
    MH_Uninitialize();
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        InitHook();
        break;  // 添加 break
    case DLL_THREAD_ATTACH:
        break;  // 添加 break
    case DLL_THREAD_DETACH:
        // UninitHook();  // 移到这里
        break;  // 添加 break
    case DLL_PROCESS_DETACH:
        UninitHook();  // 移到这里
        break;
    }
    return TRUE;
}

