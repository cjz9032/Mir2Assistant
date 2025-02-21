#include "pch.h"
#include <windows.h>
#include <string>
#include "goRun.h"
#include "skill.h"
#include "npc.h"

extern "C" __declspec(dllexport) LRESULT HookProc(int nCode, WPARAM wParam, LPARAM lParam);

LRESULT HookProc(int nCode, WPARAM wParam, LPARAM lParam) {

	if (nCode == HC_ACTION)
	{
		PCWPSTRUCT pMsg = (PCWPSTRUCT)lParam;
		if (pMsg->message == WM_COPYDATA && pMsg->wParam == 20250129)
		{
			PCOPYDATASTRUCT pCDS = (PCOPYDATASTRUCT)pMsg->lParam;
			int code = pCDS->dwData;
			int length = pCDS->cbData;
			int* data = (int*)pCDS->lpData;
			if (code == 9999) {//ִ����������
				void (*func)() = reinterpret_cast<void(*)()>(pCDS->lpData);
				func();
			}
			else if (code >= 3000) {//npc
				npc::process(code, data);
			}
			else if (code >= 2000) {//����
				skill::process(code, data);
			}
			else if (code >= 1000) {//��·��·Ѱ·
				goRun::process(code, data);
			}
		}

	}
	return CallNextHookEx(0, nCode, wParam, lParam);
}