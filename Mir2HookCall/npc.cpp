#include "pch.h"
#include "npc.h"
#include "utils.h" // ���ͷ�ļ�����



//���NPC
void clickNPC(int npcId)
{
	__asm {
		pushad
		pushfd

		push 00
		push 00
		push 00
		mov ecx, npcId
		mov edx, 0x000003F2 
		mov eax, [0x7524B4] // gvar_007524B4:TFrmMain
		mov eax, [eax] // gvar_007524B4:TFrmMain
		mov ebx, eax 
		mov esi, 0x642524 // sendclientmessage
		call esi 

		popfd
		popad
	}
}

//�����Ի� 
void talk2(DelphiString* cmd)
{

	auto cmdData = cmd->data;

	// ��talk2������
	__asm {

	    mov ecx, eax
		mov edx, dword ptr ds : [0x7563A4]
		mov eax, dword ptr ds : [0x00679EBC]
		mov eax, dword ptr[eax]
		mov ecx, cmdData
		mov esi, 0x006446D0
		call esi
	}
}


void Npc::process(int code, int* data)
{
	switch (code)
	{
	case 3001:
		clickNPC(data[0]);
		break;
	case 3002:
		ProcessWideString(data, [](const wchar_t* str, int length) {
			DelphiString* cmd = CreateDelphiString(str, length);
			talk2(cmd);
			delete cmd; // �ͷ��ڴ�
		});
		break;
	default:
		break;
	}
}