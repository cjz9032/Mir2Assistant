#include "pch.h"
#include "skill.h"

//¼¼ÄÜCALL
void skillCall(int skillAddr, int skillCallAddr)
{
	__asm {
		push skillAddr
		push 0
		push 0
		mov ecx,0x12c
		mov edx,0x1c0
		mov ebx, skillCallAddr
		call ebx
	}
}

void skill::process(int code, int* data)
{
	switch (code)
	{
	case 2001:
		skillCall(data[0], data[1]);
		break;
	
	default:
		break;

	}
}
