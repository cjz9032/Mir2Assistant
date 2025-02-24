#include "pch.h"
#include "npc.h"



//点击NPC
void clickNPC(int npcId, int findPathPara, int clickNPCCallAddr)
{
	__asm {
		mov edx, npcId
		mov eax, findPathPara
		mov eax, [eax]
		mov ebx, clickNPCCallAddr
		call ebx
	}
}

//二级对话 para1买物参数, para2小退参数
void talk2(int para1, int para2, int talk2Addr, char* cmd)
{
	__asm {
		mov eax, para1
		mov eax, [eax]
		mov edx, [eax]
		mov eax, para2
		mov eax, [eax]
		mov eax, [eax]
		mov ecx, cmd
		mov ebx, talk2Addr
		call ebx
	}
}


void npc::process(int code, int* data)
{
	switch (code)
	{
	case 3001:
		clickNPC(data[0], data[1], data[2]);
		break;
	case 3002:
		talk2(data[0], data[1], data[2], (char*)&data[4]);
		break;
	default:
		break;

	}
}