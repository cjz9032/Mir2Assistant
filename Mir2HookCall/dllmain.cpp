#include "pch.h"
#include "account_info.h"
#include <stdio.h>
#include "MinHook.h"
#include <Windows.h>
#include "login.h"
#include <random>
#include <string>


// 生成随机字符串的函数
std::wstring GenerateRandomString(int length) {
    const wchar_t charset[] = L"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    const int charset_size = sizeof(charset) / sizeof(charset[0]) - 1;
    
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> dis(0, charset_size - 1);
    
    std::wstring result;
    result.reserve(length);
    
    for (int i = 0; i < length; ++i) {
        result += charset[dis(gen)];
    }
    
    return result;
}

typedef void (*OriginalFuncType)(void);
OriginalFuncType originalFunc1 = NULL;
// DWORD g_BaseAddr = 0;
DWORD g_BaseAddr = 0x00400000;

OriginalFuncType originalFunc3 = NULL;
OriginalFuncType originalFunc4 = NULL;
OriginalFuncType originalFunc5 = NULL;
OriginalFuncType originalFunc6 = NULL; 
OriginalFuncType originalFunc7 = NULL;
OriginalFuncType originalFunc8 = NULL;
OriginalFuncType originalFunc9 = NULL;
OriginalFuncType originalFunc10 = NULL;
OriginalFuncType originalFunc11 = NULL;
OriginalFuncType originalFunc111 = NULL;
OriginalFuncType originalFunc112 = NULL;
OriginalFuncType originalFunc12 = NULL;
OriginalFuncType originalFuncRunHP = NULL;
OriginalFuncType originalFuncExitBattle = NULL;
OriginalFuncType originalFuncAutoGroup1 = NULL;
OriginalFuncType originalFuncAutoGroup2 = NULL;
OriginalFuncType originalFuncAutoAgree = NULL;


__declspec(naked) void HookFunction()
{
    __asm {
        pushfd

        cmp eax, 0x1F
        jne call_original
        popfd
        mov eax, originalFunc1
        add eax, 6
        jmp eax
    call_original:
        popfd
        jmp originalFunc1
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


__declspec(naked) void HookFunction5()
{
    __asm {
        // 重输入账号 
        pushad
        pushfd

        call loginFirst

        mov esi, originalFunc5
        add esi, 5
        jmp esi

        popfd
        popad

    }
}

// void InitBaseAddr() {
//     g_BaseAddr = g_BaseAddr;
// }


// 在 InitHook 函数中，使用临时变量避免直接引用 g_ActString
// 在文件顶部添加函数声明
extern "C" DWORD GetActStringDataAddr();
extern "C" DWORD GetPwdStringDataAddr();

// __declspec(naked) void HookFunction6()
// {
//     __asm {
//         pushad
//         pushfd

//         mov     eax, g_BaseAddr
//         add     eax, 0x279B3C
//         mov     ecx, [eax]
//         mov     dword ptr [ecx], 100

//         mov     eax, g_BaseAddr
//         add     eax, 0x279B78
//         mov     ecx, [eax]
//         mov     dword ptr [ecx], 1500
//         popfd
//         popad
        
//         jmp originalFunc6
//     }
// }

// __declspec(naked) void HookFunction7()
// {
//     __asm {
//         mov eax, g_BaseAddr
//         add eax, 0x3564EC        
//         mov byte ptr [eax], 0x1 
//         jmp originalFunc7
//     }
// }

// 跳过弹窗的hook函数
// 00650312        mov         edx,654720;'物品被卖出。'
// 0065034C        mov         edx,654760;'金币不足。'
// 0065015B        mov         edx,65466C;'你不能修理这个物品'

// 随机字符串生成
DelphiString g_BiosString = { -1, 30 };  // 初始化基本结构

void InitializeBiosString() {
    std::wstring randomStr = GenerateRandomString(30);  // 生成30个字符
    g_BiosString.length = 30;
    wcscpy_s(g_BiosString.data, randomStr.c_str());
}

// BIOS信息伪造
// 最直接找lom2key67me3934od7sdn3
__declspec(naked) void BiosFake()
{
    __asm {
        lea eax, g_BiosString
        add eax, 8  // 跳过 refCount(4字节) 和 length(4字节)，直接指向 data
        jmp originalFunc12
    }
}

__declspec(naked) void RunHP()
{
    __asm {
        mov eax, 1
        sub eax, 0
        mov eax, originalFuncRunHP
        add eax, 5
        jmp eax
    }
}

__declspec(naked) void ExitBattle()
{
    __asm {
        mov eax, originalFuncExitBattle
        add eax, 2
        jmp eax
    }
}

__declspec(naked) void AutoGroup1()
{
    __asm {
        mov eax, originalFuncAutoGroup1
        add eax, 5
        jmp eax
    }
}

__declspec(naked) void AutoGroup2()
{
    __asm {
        mov eax, originalFuncAutoGroup2
        add eax, 5
        jmp eax
    }
}

__declspec(naked) void AutoAgree()
{
    __asm {
        mov eax, originalFuncAutoAgree
        add eax, 8
        jmp eax
    }
}


__declspec(naked) void SkipPopup()
{
    __asm {
        mov edx, MIR_SKP_DLG_END_ADDR
        jmp edx
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
    // InitBaseAddr();
    
    // 直接调用函数，不需要再次声明
    g_ActStringDataAddr = GetActStringDataAddr();
    g_PwdStringDataAddr = GetPwdStringDataAddr();
    
    if (MH_Initialize() != MH_OK)
    {
        printf("MinHook init failed\n");
        return false;
    }

    // 不倒翁
    if (MIR_BU_DAO_HOOK) {
        DWORD targetAddress1 = MIR_BU_DAO_ADDR + g_BaseAddr;
        if (MH_CreateHook((LPVOID)targetAddress1, HookFunction, (LPVOID*)&originalFunc1) != MH_OK)
        {
            printf("hook 1 fail\n");
            return false;
        }
    }
    // 自动填充账号  
    DWORD targetAddress3 = MIR_LOGIN_ACT_HOOK + g_BaseAddr;
    if (MH_CreateHook((LPVOID)targetAddress3, HookFunction3, (LPVOID*)&originalFunc3) != MH_OK)
    {
        printf("hook 3 fail\n");
        return false;
    }

    // 自动填充密码
    DWORD targetAddress4 = MIR_LOGIN_PWD_HOOK + g_BaseAddr;  // 根据注释中的地址
    if (MH_CreateHook((LPVOID)targetAddress4, HookFunction4, (LPVOID*)&originalFunc4) != MH_OK)
    {
        printf("hook 4 fail\n");
        return false;
    }

    // 登录锁定 
    // 目前 特征相同 
    // 消息列表找 '这个帐号正在使用，或者是被异常的终止锁定了\请稍后再试。'

    DWORD targetAddress5 = MIR_HK5_ADDR + g_BaseAddr;  // 根据注释中的地址
    if (MH_CreateHook((LPVOID)targetAddress5, HookFunction5, (LPVOID*)&originalFunc5) != MH_OK)
    {
        printf("hook 5 fail\n");
        return false;
    }

    // 通过游戏改写了 不需要这里
    // 0065EC5C 移动
    // 0065EC64 攻速
    // 0072F784 超负重
    // 血量 通过组队找把
    // 6 基础属性变速等
    // DWORD targetAddress6 = 0x2520D2 + g_BaseAddr; // TODO: 替换为你的目标地址
    // if (MH_CreateHook((LPVOID)targetAddress6, HookFunction6, (LPVOID*)&originalFunc6) != MH_OK)
    // {
    //     printf("hook 6 fail\n");
    //     return false;
    // }
    // // 7 又一些开关, 超负重, 不确定是否有其他
    // DWORD targetAddress7 = 0x24A84B + g_BaseAddr;
    // if (MH_CreateHook((LPVOID)targetAddress7, HookFunction7, (LPVOID*)&originalFunc7) != MH_OK)
    // {
    //     printf("hook 7 fail\n");
    //     return false;
    // }

    // 定义所有要跳过的弹窗
    SkipHookInfo skipHooks[] = {

        {L"xxx", MIR_SKP_DLG1_ADDR, SkipPopup, (void**)&originalFunc8, "skip_popup1"},  // hook在这里
        {L"xxx", MIR_SKP_DLG2_ADDR, SkipPopup, (void**)&originalFunc9, "skip_popup2"},  // hook在这里
        {L"xxx", MIR_SKP_DLG3_ADDR, SkipPopup, (void**)&originalFunc10, "skip_popup3"},  // hook在这里
        {L"xxx", MIR_SKP_DLG4_ADDR, SkipPopup, (void**)&originalFunc11, "skip_popup4"},  // hook在这里
        {L"xxx", MIR_SKP_DLG5_ADDR, SkipPopup, (void**)&originalFunc111, "skip_popup5"},  // hook在这里
        {L"xxx", MIR_SKP_DLG6_ADDR, SkipPopup, (void**)&originalFunc112, "skip_popup6"}  // hook在这里
    };

    // 批量安装所有跳过钩子
    for (const auto& hook : skipHooks)
    {
        DWORD targetAddress = hook.offset + g_BaseAddr;
        if (MH_CreateHook((LPVOID)targetAddress, (LPVOID)hook.hookFunc, hook.originalFunc) != MH_OK)
        {
            printf("hook %s fail\n", hook.hookName);
            return false;
        }
    }

    // 12 BIOS信息伪造
    DWORD targetAddress12 = MIR_HK_BIOS_ADDR + g_BaseAddr;
    if (MH_CreateHook((LPVOID)targetAddress12, BiosFake, (LPVOID*)&originalFunc12) != MH_OK)
    {
        printf("hook 12 fail\n");
        return false;
    }

    // 13 跑10血
    DWORD targetAddressHP = MIR_HK_RUN_HP_ADDR + g_BaseAddr;
    if (MH_CreateHook((LPVOID)targetAddressHP, RunHP, (LPVOID*)&originalFuncRunHP) != MH_OK)
    {
        printf("hook 13 fail\n");
        return false;
    }

    // 14 无视战斗小退
    DWORD targetAddress14 = MIR_HK_EXIT_BATTLE_ADDR + g_BaseAddr;
    if (MH_CreateHook((LPVOID)targetAddress14, ExitBattle, (LPVOID*)&originalFuncExitBattle) != MH_OK)
    {
        printf("hook 14 fail\n");
        return false;
    }

     // 15 自动接受组1
     DWORD targetAddress15 = MIR_HK_AUTO_GROUP_1_ADDR + g_BaseAddr;
     if (MH_CreateHook((LPVOID)targetAddress15, AutoGroup1, (LPVOID*)&originalFuncAutoGroup1) != MH_OK)
     {
         printf("hook 14 fail\n");
         return false;
     }
     // 16 自动接受组2
     DWORD targetAddress16 = MIR_HK_AUTO_GROUP_2_ADDR + g_BaseAddr;
     if (MH_CreateHook((LPVOID)targetAddress16, AutoGroup2, (LPVOID*)&originalFuncAutoGroup2) != MH_OK)
     {
         printf("hook 16 fail\n");
         return false;
     }
     // 17 小退自动同意
    DWORD targetAddress17 = MIR_HK_AUTO_AGREE_ADDR + g_BaseAddr;
    if (MH_CreateHook((LPVOID)targetAddress17, AutoAgree, (LPVOID*)&originalFuncAutoAgree) != MH_OK)
    {
        printf("hook 17 fail\n");
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
        InitializeBiosString();  // 初始化随机字符串
        InstallHooks();
        break;
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        UninitHook();
        break;
    }
    return TRUE;
}


