#include "pch.h"
#include "goRun.h"
#include <string>

//extern "C" __declspec(dllexport) void run(int x, int y, int dir, int typePara, int para, int addr);

//走路跑路
void run(int x, int y, int dir, int typePara, int para, int addr)
{
	__asm {
		pushad                  // 保存所有通用寄存器
		pushfd                  // 保存标志寄存器

		mov eax, y
		push eax
		mov edi, dir
		push edi
		push 0
		push 0
		push 0
		push 0
		mov eax, para
		mov eax, [eax]
		mov ecx, x
		mov dx, word ptr[typePara]
		mov ebx, addr
		call ebx

				// 恢复所有寄存器
		popfd                   // 恢复标志寄存器
		popad                   // 恢复所有通用寄存器
	}
}

void sendMsgLimited(int x, int y, int dir, int typePara, int para, int addr)
{
	__asm {
		pushad                  // 保存所有通用寄存器
		pushfd                  // 保存标志寄存器

		mov eax, y
		push eax
		mov edi, dir
		push edi
		push 0
		push 0
		push 0
		push 0
		mov eax, para
		mov eax, [eax]
		mov ecx, x
		mov dx, word ptr[typePara]
		mov ebx, addr
		call ebx
		popfd                 
		popad              
	}
}

void longHitSendMessage(int x, int y, int dir, int typePara, int para, int addr)
{
	__asm {
		pushad                 
		pushfd  
		// 先判断能不能
		// CanNextAction
		// mov eax, [FRMMAIN_ADDR]
		// mov eax, [eax]
		// mov esi, MIR_CAN_NEXT_ACT_CALL
		// call esi
		// cmp eax, 0
		// je end
		
		// CanNextHit
		// mov eax, [FRMMAIN_ADDR]
		// mov eax, [eax]
		// mov esi, MIR_CAN_NEXT_HIT_CALL
		// call esi
		// cmp eax, 0
		// je end

		mov eax, y
		push eax
		mov edi, dir
		push edi
		push 0
		push 0
		push 0
		push 0
		mov eax, para
		mov eax, [eax]
		mov ecx, x
		mov dx, word ptr[typePara]
		mov ebx, addr
		call ebx
	end:
        popfd                 
        popad              
	}
}




void GoRun::process(int code, int* data)
{
	switch (code)
	{
	case 1000:
		sendMsgLimited(data[0], data[1], data[2], data[3], data[4], data[5]);
	case 1001:
		run(data[0], data[1], data[2], data[3], data[4], data[5]);
	case 1002:
		longHitSendMessage(data[0], data[1], data[2], data[3], data[4], data[5]);
		break;
	default:
		break;

	}
}
