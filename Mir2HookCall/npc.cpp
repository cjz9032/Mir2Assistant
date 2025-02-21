#include "pch.h"
#include "npc.h"



//µã»÷NPC
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

void npc::process(int code, int* data)
{
	switch (code)
	{
	case 3001:
		clickNPC(data[0], data[1], data[2]);
		break;

	default:
		break;

	}
}