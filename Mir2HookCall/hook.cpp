#include "pch.h"
#include <windows.h>
#include <string>
#include "goRun.h"
#include "login.h"
#include "npc.h"
#include "sys.h"

extern "C" __declspec(dllexport) LRESULT HookProc(int nCode, WPARAM wParam, LPARAM lParam);

LRESULT HookProc(int nCode, WPARAM wParam, LPARAM lParam) {

	if (nCode == HC_ACTION)
	{
		PCWPSTRUCT pMsg = (PCWPSTRUCT)lParam;
		if (pMsg->message == WM_COPYDATA && pMsg->wParam == 20250129)
		{
			wchar_t debugMsg[100];
			swprintf_s(debugMsg, L"SetAccountInfo");
			OutputDebugStringW(debugMsg);


			PCOPYDATASTRUCT pCDS = (PCOPYDATASTRUCT)pMsg->lParam;
			int code = pCDS->dwData;
			int length = pCDS->cbData;
			int* data = (int*)pCDS->lpData;
			if (code >= 9000) {
		/*		char testMsg[100] = { 0 };

				MessageBoxA(NULL, testMsg, "测试", MB_OK);*/

				Sys::process(code, data);
			}
			else if (code >= 3000) {//npc
				Npc::process(code, data);
			}
			else if (code >= 1000) {//走路跑路寻路
				GoRun::process(code, data);
			}
			else if (code < 1000) {//走路跑路寻路
				Login::process(code, data);
			}
		}

	}
	return CallNextHookEx(0, nCode, wParam, lParam);
}