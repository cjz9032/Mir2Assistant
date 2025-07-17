#pragma once
#include <Windows.h>
#include "utils.h" // 包含utils.h以获取DelphiString定义

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