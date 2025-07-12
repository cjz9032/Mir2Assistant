#include "pch.h"
#include <stdio.h>
#include "MinHook.h"
#include <Windows.h>
#include "login.h"


typedef void (*OriginalFuncType)(void);
OriginalFuncType originalFunc1 = NULL;
DWORD g_BaseAddr = 0;

OriginalFuncType originalFunc2 = NULL;
OriginalFuncType originalFunc3 = NULL;
OriginalFuncType originalFunc4 = NULL;
OriginalFuncType originalFunc5 = NULL;

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

// 添加线程函数声明
DWORD WINAPI DelayedExtraActionThread(LPVOID lpParam);

// 声明函数在汇编代码之前
void CreateDelayedThread();

__declspec(naked) void HookFunction2()
{
    __asm {
        // 选择服务器 
        pushad
        pushfd

        mov         ebx, g_BaseAddr
        add         ebx, 0x279EBC
        mov         eax, [ebx]
        mov         eax, [eax]
        
        mov         ebx, g_BaseAddr
        add         ebx, 0x3526C0
        mov         edx, [ebx]
        
        mov         ebx, g_BaseAddr
        add         ebx, 0x00242A48
        call        ebx
  
        popfd
        popad
        // 在汇编代码外调用C++函数
        push        eax             // 保存eax
        call        CreateDelayedThread
        pop         eax             // 恢复eax
    
        jmp originalFunc2

    }
}


// 修改Delphi字符串结构，确保引用计数在正确位置
static struct {
    DWORD refCount;  // 引用计数，位于数据指针-8的位置
    DWORD length;    // 字符数量
    WCHAR data[8];   // 足够存放"adad"的Unicode字符
} g_ActString = { -1, 4, L"adad" }; // 设置引用计数为-1，表示静态字符串

// 同样修改密码字符串结构
static struct {
    DWORD refCount;  // 引用计数
    DWORD length;    // 字符数量
    WCHAR data[8];   // 足够存放"adad"的Unicode字符
} g_PasswordString = { -1, 4, L"adad" }; // 设置引用计数为-1，表示静态字符串

__declspec(naked) void HookFunction3()
{
    __asm {
        // pushad
        // pushfd

        // 原始部分内容
        // 0056A86B        lea         edx,[ebp-10]
        // 0056A86E        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
        // 0056A873        mov         eax,dword ptr [eax]
        // 0056A875        mov         eax,dword ptr [eax+8D4]
        // 0056A87B        mov         eax,dword ptr [eax+27C]
        
        // 插入hook 把eax指向这么数据的一个地址
        // 长度4: 04 00 00 00
        // 内容(adad): 61 00 64 00 61 00 64 00

        // 0056A881        call        LowerCase
  
      
        // 直接将eax指向我们的Delphi字符串
        lea eax, g_ActString.data
        // popfd
        // popad
        // 跳转到原始函数
        jmp originalFunc3
    }
}

__declspec(naked) void HookFunction4()
{
    __asm {
     /*   0056A891        mov         eax, [0067A728]; ^ gvar_0074350C:TFrmDlg
        0056A896        mov         eax, dword ptr[eax]
        0056A898        mov         eax, dword ptr[eax + 8D8]
        0056A89E        mov         edx, dword ptr[eax + 27C]
        0056A8A4        mov         eax, dword ptr[ebp - 4]
        0056A8A7        call        005695A0*/
        // 直接将eax指向我们的Delphi密码字符串
        lea edx, g_PasswordString.data

        // 跳转到原始函数
        jmp originalFunc4
    }
}

// 创建延迟线程的函数
void CreateDelayedThread() {
    // 创建线程执行延迟操作
    HANDLE hThread = CreateThread(NULL, 0, DelayedExtraActionThread, NULL, 0, NULL);
    if (hThread) {
        CloseHandle(hThread);  // 关闭句柄，线程会继续执行
    }
}

// 线程函数，执行延迟操作
DWORD WINAPI DelayedExtraActionThread(LPVOID lpParam) {
    // 延迟3秒
    Sleep(6000);
    
    // 执行额外操作
    DWORD extraPtr = *(DWORD*)(g_BaseAddr + 0x27A018);
    DWORD extraObj = *(DWORD*)(extraPtr);
    
    typedef void (*ExtraFunc)(DWORD obj);
    ExtraFunc extraFunc = (ExtraFunc)(g_BaseAddr + 0x16D10C);
    extraFunc(extraObj);
    
    return 0;
}

__declspec(naked) void HookFunction5()
{
    __asm {
        // 重输入账号 
        pushad
        pushfd

        call loginFirst

     

        mov esi, 0x0064AAA8
        jmp esi

        popfd
        popad

    }
}

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


    DWORD targetAddress1 = 0x1DF76C + (DWORD)GetModuleHandle(L"ZC.H");
    if (MH_CreateHook((LPVOID)targetAddress1, HookFunction, (LPVOID*)&originalFunc1) != MH_OK)
    {
        printf("hook 1 fail\n");
        return false;
    }
    // 自动跳
    DWORD targetAddress2 = 0x24ADB9 + (DWORD)GetModuleHandle(L"ZC.H");
    if (MH_CreateHook((LPVOID)targetAddress2, HookFunction2, (LPVOID*)&originalFunc2) != MH_OK)
    {
        printf("hook 2 fail\n");
        return false;
    }

    // 自动填充账号  
    DWORD targetAddress3 = 0x16A881 + (DWORD)GetModuleHandle(L"ZC.H");
    if (MH_CreateHook((LPVOID)targetAddress3, HookFunction3, (LPVOID*)&originalFunc3) != MH_OK)
    {
        printf("hook 3 fail\n");
        return false;
    }

    // 自动填充密码
    DWORD targetAddress4 = 0x0016A8A4 + (DWORD)GetModuleHandle(L"ZC.H");  // 根据注释中的地址
    if (MH_CreateHook((LPVOID)targetAddress4, HookFunction4, (LPVOID*)&originalFunc4) != MH_OK)
    {
        printf("hook 4 fail\n");
        return false;
    }

    // 登录锁定
    DWORD targetAddress5 = 0x24AAA3 + (DWORD)GetModuleHandle(L"ZC.H");  // 根据注释中的地址
    if (MH_CreateHook((LPVOID)targetAddress5, HookFunction5, (LPVOID*)&originalFunc5) != MH_OK)
    {
        printf("hook 5 fail\n");
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

