#include "pch.h"
#include "npc.h"
#include "utils.h" // 添加头文件引用



//点击NPC
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

//二级对话 
void talk2(DelphiString* cmd)
{

	auto cmdData = cmd->data;

	// 在talk2函数中
	__asm {
		pushad
		pushfd

	    mov ecx, eax
		mov edx, dword ptr ds : [0x7563A4]
		mov eax, dword ptr ds : [0x00679EBC]
		mov eax, dword ptr[eax]
		mov ecx, cmdData
		mov esi, 0x006446D0
		call esi

		popfd
		popad
	}
}

void buy(DelphiString* name)
{

	auto nameData = name->data;
	__asm {
		pushad
		pushfd

		mov         eax,nameData
		push        eax
		push        1
		mov         ecx, 0x1
		mov edx,dword ptr ds:[0x6799E8];
		mov edx,dword ptr [edx]
		mov eax, [0x7524B4] // gvar_007524B4:TFrmMain
		mov eax, [eax]
		mov esi, 0x006459F4
		call esi

		popfd
		popad
	}
}

// 通用 NPC 功能执行函数
void executeNpcFunction(DelphiString* name, int id, uintptr_t functionAddress)
{
	auto nameData = name->data;
	__asm {
		pushad
		pushfd

		mov         eax, nameData
		push        eax
		push        1
		mov         ecx, id
		mov edx, dword ptr ds:[0x6799E8];
		mov edx, dword ptr [edx]
		mov eax, [0x7524B4] // gvar_007524B4:TFrmMain
		mov eax, [eax]
		mov esi, functionAddress
		call esi

		popfd
		popad
	}
}

void repairItem(DelphiString* name, int id)
{
	auto nameData = name->data;
	__asm {
		pushad
		pushfd

		mov         eax, nameData
		push        eax
		mov         ecx, id
		mov edx, dword ptr ds:[0x6799E8];
		mov edx, dword ptr [edx]
		mov eax, [0x7524B4] // gvar_007524B4:TFrmMain
		mov eax, [eax]
		mov esi, 0x006450F8
		call esi

		popfd
		popad
	}
}

void sell(DelphiString* name, int id)
{
	executeNpcFunction(name, id, 0x00645018);
}

void storeItem(DelphiString* name, int id)
{
	executeNpcFunction(name, id, 0x006452EC);
}

void backStoreItem(DelphiString* name, int id)
{
	executeNpcFunction(name, id, 0x006452EC);
}


inline void processNpcCommand(int* data, void (*action)(DelphiString*, int)) {
	ProcessWideString(data, [data, action](const wchar_t* str, int length) {
		DelphiString* name = CreateDelphiString(str, length);
		int id = data[length + 1];
		action(name, id);
		delete name;
	});
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
			delete cmd;
		});
		break;
	case 3010:
		ProcessWideString(data, [](const wchar_t* str, int length) {
			DelphiString* name = CreateDelphiString(str, length);
			buy(name);
			delete name; 
		});
		break;
	case 3011:
		processNpcCommand(data, sell);
		break;
	case 3012:
		processNpcCommand(data, repairItem);
		break;
	case 3015:
		processNpcCommand(data, storeItem);
		break;
	case 3016:
		processNpcCommand(data, backStoreItem);
		break;


	case 3020: // 脱
		clickNPC(data[0]);
		break;
	default:
		break;
	}
}