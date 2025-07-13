#pragma once
#include <Windows.h>

// Delphi字符串结构体定义
struct DelphiString {
    DWORD refCount;
    DWORD length;
    WCHAR data[32];
};

#ifdef __cplusplus
extern "C" {
#endif

__declspec(dllexport) void SetAccountInfo(const wchar_t* account, const wchar_t* password);

#ifdef __cplusplus
}
#endif

// 只声明，不定义
extern DelphiString g_ActString;
extern DelphiString g_PwdString;