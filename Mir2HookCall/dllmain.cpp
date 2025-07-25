#include "pch.h"
#include "account_info.h"
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
OriginalFuncType originalFunc6 = NULL; 
OriginalFuncType originalFunc7 = NULL;
OriginalFuncType originalFunc8 = NULL;

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
DWORD WINAPI DelayedStartGameThread(LPVOID lpParam);

// 声明函数在汇编代码之前
void CreateDelayedStartGame();

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
  
 
        // 在汇编代码外调用C++函数
     
        // call        CreateDelayedStartGame

        popfd
        popad
    
        jmp originalFunc2

    }
}

// 新增：定义全局变量保存 data 成员地址
DWORD g_ActStringDataAddr = 0; // 先声明，不初始化
DWORD g_PwdStringDataAddr = 0; // 为密码字符串添加变量

__declspec(naked) void HookFunction3()
{
    __asm {
        mov eax, g_ActStringDataAddr
        jmp originalFunc3
    }
}

__declspec(naked) void HookFunction4()
{
    __asm {
        // 使用全局变量而不是直接访问
        mov edx, g_PwdStringDataAddr
        jmp originalFunc4
    }
}

// 创建延迟线程的函数
void CreateDelayedStartGame() {
    // 创建线程执行延迟操作
    HANDLE hThread = CreateThread(NULL, 0, DelayedStartGameThread, NULL, 0, NULL);
    if (hThread) {
        CloseHandle(hThread);  // 关闭句柄，线程会继续执行
    }
}

// 线程函数，执行延迟操作
DWORD WINAPI DelayedStartGameThread(LPVOID lpParam) {
    Sleep(7000);
    
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

// 在 InitHook 函数中，使用临时变量避免直接引用 g_ActString
// 在文件顶部添加函数声明
extern "C" DWORD GetActStringDataAddr();
extern "C" DWORD GetPwdStringDataAddr();

__declspec(naked) void HookFunction6()
{
    __asm {
        // ZC.H+252036 - A1 3C9B6700           - mov eax,[ZC.H+279B3C] { (00677244) } 110 移动速度
        // ZC.H+25203B - 8B 95 B0FBFFFF        - mov edx,[ebp-00000450]
        // ZC.H+252041 - 89 10                 - mov [eax],edx
        // ZC.H+252050 - A1 789B6700           - mov eax,[ZC.H+279B78] { (0067724C) } -- 攻速 1400
        // ZC.H+252055 - 8B 95 B8FBFFFF        - mov edx,[ebp-00000448]
        // ZC.H+25205B - 89 10                 - mov [eax],edx
        pushad
        pushfd

        mov     eax, g_BaseAddr
        add     eax, 0x279B3C
        mov     ecx, [eax]
        mov     dword ptr [ecx], 90

        mov     eax, g_BaseAddr
        add     eax, 0x279B78
        mov     ecx, [eax]
        mov     dword ptr [ecx], 1000
        popfd
        popad
        
        jmp originalFunc6
    }
}

__declspec(naked) void HookFunction7()
{
    __asm {
        // ZC.H+24A840 - 8A 85 48FEFFFF        - mov al,[ebp-000001B8]
        // ZC.H+24A846 - A2 EC647500           - mov [ZC.H+3564EC],al { (0) } -->超负重指针
        // ZC.H+24A84B - 83 3D 64277500 00     - cmp dword ptr [ZC.H+352764],00 { (06D0D6A0),0 }

        mov eax, g_BaseAddr
        add eax, 0x3564EC        // 使用基址+偏移
        mov byte ptr [eax], 0x1  // 使用正确的数据大小
        jmp originalFunc7
    }
}

// 跳过弹窗的hook函数
__declspec(naked) void SkipPopup()
{
    __asm {
        pop eax
        add eax, 5
        jmp eax
    }
}

// 定义要跳过的地址结构
struct SkipHookInfo {
    const wchar_t* moduleName;
    DWORD offset;
    void* hookFunc;
    void** originalFunc;
    const char* hookName;
};

bool InstallHooks()
{
    InitBaseAddr();
    
    // 直接调用函数，不需要再次声明
    g_ActStringDataAddr = GetActStringDataAddr();
    g_PwdStringDataAddr = GetPwdStringDataAddr();
    
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

    // 6 基础属性变速等
    DWORD targetAddress6 = 0x2520D2 + (DWORD)GetModuleHandle(L"ZC.H"); // TODO: 替换为你的目标地址
    if (MH_CreateHook((LPVOID)targetAddress6, HookFunction6, (LPVOID*)&originalFunc6) != MH_OK)
    {
        printf("hook 6 fail\n");
        return false;
    }
    // 7 又一些开关, 超负重, 不确定是否有其他
    DWORD targetAddress7 = 0x24A84B + (DWORD)GetModuleHandle(L"ZC.H");
    if (MH_CreateHook((LPVOID)targetAddress7, HookFunction7, (LPVOID*)&originalFunc7) != MH_OK)
    {
        printf("hook 7 fail\n");
        return false;
    }

    // 定义所有要跳过的弹窗
    SkipHookInfo skipHooks[] = {
        // 00650312        mov         edx,654720;'物品被卖出。'
        {L"ZC.H", 0x250317, SkipPopup, (void**)&originalFunc8, "skip_popup1"},  // hook在这里
        // 0065034C        mov         edx,654760;'金币不足。'
        {L"ZC.H", 0x250351, SkipPopup, (void**)&originalFunc8, "skip_popup2"},  // hook在这里
        // 0065015B        mov         edx,65466C;'你不能修理这个物品'
        {L"ZC.H", 0x250160, SkipPopup, (void**)&originalFunc8, "skip_popup3"}  // hook在这里
    };

    // 批量安装所有跳过钩子
    for (const auto& hook : skipHooks)
    {
        DWORD targetAddress = hook.offset + (DWORD)GetModuleHandle(hook.moduleName);
        if (MH_CreateHook((LPVOID)targetAddress, (LPVOID)hook.hookFunc, hook.originalFunc) != MH_OK)
        {
            printf("hook %s fail\n", hook.hookName);
            return false;
        }
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
        InstallHooks();
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


