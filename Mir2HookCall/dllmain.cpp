#include "pch.h"
#include <stdio.h>
#include "MinHook.h"

typedef void (*OriginalFuncType)(void);
OriginalFuncType originalFunc1 = NULL;
DWORD g_BaseAddr = 0;

OriginalFuncType originalFunc2 = NULL;

__declspec(naked) void HookFunction()
{
    __asm {
        pushfd

        cmp eax, 0x1F
        jne call_original

        xor eax, eax

    call_original:
        popfd
        jmp originalFunc1
    }
}

// bool SafeWriteByte(void* ptr, size_t offset, BYTE value)
// {
//     __try
//     {
//         BYTE* p = (BYTE*)ptr;
//         p[offset] = value;
//         return true;
//     }
//     __except (EXCEPTION_EXECUTE_HANDLER)
//     {
//         return false;
//     }
// }

// void HookFunction2_C(int ecx)
// {
//     DWORD base = *(DWORD*)((DWORD)GetModuleHandle(L"ZC.H") + 0x3524C8);
//     //[[[ZC.H + 3524C8]+ 000034C8] + 4] + 4
//     base += 0x34C8;
//     DWORD* pArray = *(DWORD**)base;
//     DWORD* pItem = *(DWORD**)((BYTE*)pArray + 4);
//     pItem = (DWORD*)((BYTE*)pItem + ecx * 4);

//     SafeWriteByte(pItem, 0x158, 1);
// }




// __declspec(naked) void HookFunction222()
// {
//     __asm {
//         pushad
//         pushfd

//         mov eax, g_BaseAddr
//         add eax, 0x3524C8
//         mov eax, dword ptr [eax]
//         add eax, 0x34C8
//         mov eax, dword ptr [eax]
//         mov edx, dword ptr [eax + 8]

//         xor ecx, ecx                     

//         loop_start:
//         cmp ecx, edx
//             jge loop_end

//             push ecx
//             call HookFunction2_C
//             add esp, 4

//             inc ecx
//             jmp loop_start

//         loop_end:
//             popfd
//             popad
//             jmp originalFunc2
//     }
// }


// __declspec(naked) void HookFunction2()
// {
//     __asm {
//         pushad
//         pushfd

//         mov eax, g_BaseAddr
//         add eax, 0x3524C8
//         mov eax, dword ptr [eax]
//         add eax, 0x34C8
//         mov eax, dword ptr [eax]
//         mov edx, dword ptr [eax + 8]

//         xor ecx, ecx

//     loop_start:
//         cmp ecx, edx
//         jge loop_end

//         // [[[ZC.H+3524C8]+000034C8]+4]+4
//         mov eax, g_BaseAddr
//         add eax, 0x3524C8
//         mov eax, dword ptr [eax]
//         add eax, 0x34C8
//         mov eax, dword ptr [eax]
//         add eax, 4
//         mov eax, dword ptr[eax]
//         mov eax, dword ptr [eax + ecx*4]
        
//         // 安全检查
//         test eax, eax
//         jz skip_write
        
//         mov byte ptr [eax + 0x158], 1

//     skip_write:
//         inc ecx
//         jmp loop_start

//     loop_end:
//         popfd
//         popad
//         jmp originalFunc2
//     }
// }
//__declspec(naked) void HookFunction2()
//{
//    __asm {
//        push eax
//        mov eax, g_BaseAddr
//        add eax, 0x352764
//        mov eax, dword ptr [eax]
//        mov byte ptr [eax + 0x158], 1
//        pop eax
//        jmp originalFunc2
//    }
//}

void InitBaseAddr() {
    g_BaseAddr = (DWORD)GetModuleHandle(L"ZC.H");
}

bool InitHook()
{
    InitBaseAddr();
    
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
    // 显血 自己 目前也不需要了
    /*DWORD targetAddress2 = 0x24C62C + (DWORD)GetModuleHandle(L"ZC.H");
    if (MH_CreateHook((LPVOID)targetAddress2, HookFunction2, (LPVOID*)&originalFunc2) != MH_OK)
    {
        printf("hook 2 fail\n");
        return false;
    }*/

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
        break;  // 添加 break
    case DLL_PROCESS_DETACH:
        UninitHook();  // 移到这里
        break;
    }
    return TRUE;
}

