#include "pch.h"
#include "goRun.h"
#include <string>

extern "C" __declspec(dllexport) void run(int x, int y, int dir, int typePara, int para, int addr);

//走路跑路
void run(int x, int y, int dir, int typePara, int para, int addr)
{
	__asm {
		mov eax, y
		push eax
		mov eax, dir
		push eax
		push 0
		push 0
		push 0
		push 0
		push 0
		mov eax, para
		mov eax, [eax]
		mov eax, [eax]
		mov ecx, x
		mov edx, typePara
		mov ebx, addr
		call ebx
	}
}

//寻路
void findPath(int x, int y, int para, int addr)
{
	__asm {
		push 1
		push 0
		mov edx, x
		mov ecx, y
		mov eax, para
		mov eax, [eax]
		mov eax, [eax]
		mov ebx, addr
		call ebx
	}
}


void goRun::process(int code, int* data)
{
	switch (code)
	{
	case 1001:
		run(data[0], data[1], data[2], data[3], data[4], data[5]);
		break;
	case 1002:
		findPath(data[0], data[1], data[2], data[3]);
		break;

	default:
		break;

	}
}
