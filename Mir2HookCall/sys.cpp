#include "pch.h"
#include "sys.h"
#include <string>
#include "account_info.h" // 添加新的头文件引用
#include "login.h"        // 新增：引入loginFirst声明
#include "utils.h"        // 添加工具函数头文件

// Define a function type (adjust as needed)

// 切组
void setGroupState(int state) {
    __asm {
		pushad
		pushfd
		mov edx , state
		mov eax,[FRMMAIN_ADDR]
		mov eax,[eax]
		mov esi, MIR_ALLOW_GROUP_CALL
		call esi
		popfd
		popad
    }
}

//组队
void groupOne(DelphiString* name)
{
	auto nameData = name->data;

    __asm {
        pushad
        pushfd

        mov eax, [MIR_GROUP_MEMBER_ADDR]
        mov eax, [eax]
        add eax, 0x30
        mov ecx, [eax]           // ecx = *(int*)(MIR_GROUP_MEMBER_ADDR + 30)
        test ecx, ecx
        jz  group_zero           // 如果为0，跳转到group_zero

        // > 0 分支
        mov eax, nameData
        mov edx, eax
        mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
        mov eax, dword ptr [eax]
        mov esi, MIR_GROUP_ONE_CALL
        call esi                 
        jmp group_end

    group_zero:
        mov eax, nameData
        mov edx, eax
        mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
        mov eax, dword ptr [eax]
        mov esi, MIR_GROUP_TWO_CALL
        call esi                

    group_end:
        popfd
        popad
    }
}

void DSServer1Click(){
	__asm {
		pushad
		pushfd
		push 1
		mov eax, dword ptr ds : [FRM_DLG_ADDR]
		add eax, 678 
		mov edx, eax
		mov ecx,1
		mov eax, dword ptr ds : [FRM_DLG_ADDR]
		mov esi, MIR_DSServer1Click_CALL
		call esi
		popfd
		popad
	}
}

void addChat(DelphiString* chat)
{
	auto chatData = chat->data;

	__asm {
		pushad
		pushfd


		mov edx, chatData
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_CHAT_CALL
		call esi

		popfd
		popad
	}
}

// void clearChat(){
// 	__asm {
// 	pushad
// 	pushfd
// 		mov eax, DRAW_SCREEN_ADDR
// 		mov eax, [eax]
// 		mov eax, [eax]
// 		add eax, 0x1C
// 		mov eax, [eax]
// 		mov esi, DELPHI_STRINGLIST_CLEAR_CALL
// 		call esi
// 	popfd
// 	popad
// 	}
// }


void cancelItemMoving(){
	__asm {

		pushad
		pushfd

		mov eax, dword ptr ds : [FRM_DLG_ADDR]
		mov esi, MIR_CANCEL_ITEM_MOVING_CALL
		call esi

		popfd
		popad
	}
}

void refPkg()
{
	_asm {
 		pushad
		pushfd
		call cancelItemMoving;

		push 00
		push 00
		push 00
		xor ecx,ecx
		mov edx, 0x51 
		mov ebx, 0 // unused
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_SendClientMessage_CALL // sendclientmessage
		call esi 

		popfd
		popad

	}
}
void openDoor(int x, int y)
{
	_asm {
		pushad
		pushfd

		push x
		push y
		push 0
		mov ecx, 0
		mov edx, 0x3ea
		mov ebx, 0
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_SendClientMessage_CALL // sendclientmessage
		call esi 

		popfd
		popad

	}
}
void exitToSelectScene(){
	_asm {
		pushad
		pushfd
		push 100
		mov ebx, MIR_GRID_FOO_ADDR
		mov ebx, [ebx]
		mov edx, ebx
		mov eax, [FRMMAIN_ADDR]
		mov eax,[eax]
		mov ecx, 100
		mov esi, MIR_DBotLogoutClick_CALL
		call esi
		popfd
		popad
	}
}
void startButton(){
	_asm {
		pushad
		pushfd

		mov eax, [MIR_SELECT_CHR_SCENE_ADDR]
        mov eax, [eax]
        mov esi, MIR_START_BTN_CALL
        call esi

		popfd
		popad
	}
}

void okButton(){
	_asm { 
		pushad
		pushfd
		push 100
		
		mov ebx, FRM_DLG_ADDR
		mov ebx, [ebx]
		add ebx, 0x8FC
		mov ebx,[ebx]
		mov ecx, 100
		mov edx, ebx
		
		mov eax, dword ptr ds : [FRM_DLG_ADDR]
		mov esi, MIR_Dlg_Ok_Click
		call esi	
		
		popfd
		popad
	}
}


void any_call(int* data) {
	void (*func)() = reinterpret_cast<void(*)()>(data);
	func();
}

void Sys::process(int code, int* data)
{
	switch (code)
	{
	case 9001: //hook 写屏call
		break;
	case 9002: //恢复写屏call
		break;
	case 9003: // 接收账号和密码数据
		ProcessWideStringsWithLengths(data, 2, [](wchar_t** strings, int* length) {
             SetAccountInfo(strings[0], strings[1]);
             loginFirst();
         });
        break;
	case 9104: // 选服务器1
			DSServer1Click();
		break;
	case 9004: // 组人
		ProcessWideString(data, [](const wchar_t* str, int length) {
			DelphiString* groupName = CreateDelphiString(str, length);
			groupOne(groupName);
			delete groupName;
		});
		break;
	case 9005: // 切组
		setGroupState(data[0]);
		break;
	case 9010: //刷新背包
		refPkg();
	case 9011: // cancel
		cancelItemMoving();
		break;
	case 9020:
		openDoor(data[0], data[1]);
		break;
	case 9098:
		exitToSelectScene();
		break;
	case 9099:
		startButton();
		break;
	case 9100:
		okButton();
		break;
	case 9200:
		ProcessWideString(data, [](const wchar_t* str, int length) {
			DelphiString* chat = CreateDelphiString(str, length);
			addChat(chat);
			delete chat;
		});
		break;
	// case 9201:
	// 		clearChat();
	// 	break;
	case 9999: //执行任务ASM代码
		any_call(data);
		break;

	default:
		break;

	}
}