#include "pch.h"
#include "login.h"
#include <string>


void loginFirst()
{
	__asm {
		pushad
		pushfd
		// FState.TFrmDlg.DLoginOkClick
		// 0059BDF4        push        ebp
		// 0059BE03        mov         eax,[0067A23C];^gvar_007524C0:TLoginScene
		// 0059BE08        mov         eax,dword ptr [eax]
		// 0059BE0A        call        0056C488 login ok
		mov         eax,[0x0067A23C]
		mov         eax,dword ptr [eax]
		mov ebx, 0x0056C488
		call ebx 

		popfd
		popad
		
	}
}

void Login::process(int code, int* data)
{
	switch (code)
	{
	case 1:
		loginFirst();
		break;
	default:
		break;

	}
}
