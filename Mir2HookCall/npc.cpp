#include "pch.h"
#include "npc.h"



//���NPC
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

//�����Ի� para1�������, para2С�˲���
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