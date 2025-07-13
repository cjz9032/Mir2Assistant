#include "pch.h"
#include "account_info.h"
#include <string.h>
#include <wchar.h> // 添加这一行

// 这里是定义（只有这里有初始化！）
DelphiString g_ActString = { -1, 4, L"adad" };
DelphiString g_PwdString = { -1, 4, L"adad" };

// 添加这些辅助函数
extern "C" DWORD GetActStringDataAddr() {
    return (DWORD)&g_ActString.data;
}

extern "C" DWORD GetPwdStringDataAddr() {
    return (DWORD)&g_PwdString.data;
}

extern "C" __declspec(dllexport) void SetAccountInfo(const wchar_t* account, const wchar_t* password) {
    //wchar_t debugMsg[100];
    
    // 初始化调试消息

    if (account && wcslen(account) > 0) {
        size_t len = wcslen(account);
        if (len > 15) len = 15;
        g_ActString.length = (DWORD)len;
        wcscpy_s(g_ActString.data, 32, account);
       /* swprintf_s(debugMsg, L"SetAccountInfo: 账号=%s, 长度=%d\n", account, (int)len);
        OutputDebugStringW(debugMsg);*/
    }

    if (password && wcslen(password) > 0) {
        size_t len = wcslen(password);
        if (len > 15) len = 15;
        g_PwdString.length = (DWORD)len;
        wcscpy_s(g_PwdString.data, 32, password);
        //swprintf_s(debugMsg, L"SetAccountInfo: 密码长度=%d\n", (int)len);
        //OutputDebugStringW(debugMsg);
    }
}