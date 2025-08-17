#include "pch.h"
#include "npc.h"
#include "utils.h" // 添加头文件引用
#include <map>



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
		mov eax, dword ptr ds : [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov ebx, eax 
		mov esi, MIR_SendClientMessage_CALL // sendclientmessage
		call esi 

		popfd
		popad
	}
}

// U_DRESS = 0;
// U_WEAPON = 1;
// U_RIGHTHAND = 2; -- DSWLight
// U_NECKLACE = 3;
// U_HELMET = 4;
// U_ARMRINGL = 5;
// U_ARMRINGR = 6;
// U_RINGL = 7;
// U_RINGR = 8;
// 0059F858        cmp         eax,dword ptr [edx+6C8];TFrmDlg.DSWDress:TDButton
// 0059F8AB        cmp         eax,dword ptr [edx+6C4];TFrmDlg.DSWWeapon:TDButton
// 0059F8C6        cmp         eax,dword ptr [edx+6AC];TFrmDlg.DSWNecklace:TDButton
// 0059F8E1        cmp         eax,dword ptr [edx+6B0];TFrmDlg.DSWLight:TDButton
// 0059F8FC        cmp         eax,dword ptr [edx+6CC];TFrmDlg.DSWHelmet:TDButton
// 0059F984        cmp         eax,dword ptr [edx+6C0];TFrmDlg.DSWRingL:TDButton
// 0059F99D        cmp         eax,dword ptr [edx+6BC];TFrmDlg.DSWRingR:TDButton
// 0059FA35        cmp         eax,dword ptr [edx+6B8];TFrmDlg.DSWArmRingL:TDButton
// 0059FA4E        cmp         eax,dword ptr [edx+6B4];TFrmDlg.DSWArmRingR:TDButton
// 0059FA7A        cmp         eax,dword ptr [edx+6EC];TFrmDlg.DSWBujuk:TDButton
// 0059FAAD        cmp         eax,dword ptr [edx+6EC];TFrmDlg.DSWBujuk:TDButton
// 0059FAE3        cmp         eax,dword ptr [edx+6B8];TFrmDlg.DSWArmRingL:TDButton
// 0059FB05        cmp         eax,dword ptr [edx+6F8];TFrmDlg.DSWCharm:TDButton
// 0059FB46        cmp         eax,dword ptr [edx+6F0];TFrmDlg.DSWBelt:TDButton
// 0059FB76        cmp         eax,dword ptr [edx+6F4];TFrmDlg.DSWBoots:TDButton
// 0059FB98        cmp         eax,dword ptr [edx+6F8];TFrmDlg.DSWCharm:TDButton
// 0059FBB3        cmp         eax,dword ptr [edx+0A08];TFrmDlg.ButTrans:TDButton

const std::map<int, int> offsetMap = {
    {0, 0x6C8},  // DRESS
    {1, 0x6C4},  // WEAPON
    {2, 0x6B0},  // DSWLight U_RIGHTHAND 
    {3, 0x6AC},  // NECKLACE
    {4, 0x6CC},  // HELMET
    {5, 0x6B8},  // ARMRINGL
    {6, 0x6B4},  // ARMRINGR
    {7, 0x6C0},  // RINGL
    {8, 0x6BC}   // RINGR
};





// DItemGridGridSelect_
void bagGridClick(int index){

	// 8x5的背包布局
	int row = index % 8;        // 每行8格
	int col = index / 8;        // 总共5行
	__asm {

		pushad
		pushfd

		push 100 
		push row
		push col
		push 0
		mov ebx, dword ptr ds : [MIR_GRID_FOO_ADDR]
		mov ecx,100
		mov edx, dword ptr ds : [MIR_GRID_FOO_ADDR]
		mov eax, dword ptr ds : [FRM_DLG_ADDR]
		mov esi, MIR_DItemGridGridSelect_CALL
		call esi 

		popfd
		popad
	}
}

// 
void charGridClick(int index){
	int myoffset = offsetMap.at(index);
	__asm {
		pushad
		pushfd

		push 100
		mov eax, dword ptr ds : [FRM_DLG_ADDR]
		mov ebx, eax
		add ebx, myoffset
		mov ebx, [ebx]
		mov ecx,100
		mov edx,ebx

		mov esi, MIR_DSWWeaponClick_CALL
		call esi

		popfd
		popad
	}
}


DWORD WINAPI TakeOffDelayThread(LPVOID lpParam) {
	Sleep(300);
	bagGridClick(0);
	return 0;
}

void takeOff(int idx) {
	charGridClick(idx);
	CreateThread(NULL, 0, TakeOffDelayThread, NULL, 0, NULL);
}
DWORD WINAPI TakeOnDelayThread(LPVOID lpParam) {
	Sleep(300);
	int itemIdx = reinterpret_cast<int>(lpParam);
	charGridClick(itemIdx);
	return 0;
}
void takeOn(int girdIdx, int itemIdx) {
	bagGridClick(girdIdx);
	CreateThread(NULL, 0, TakeOnDelayThread, reinterpret_cast<LPVOID>(itemIdx), 0, NULL);
}

void takeOff2(DelphiString* name, int idx, int id) {
	auto nameData = name->data;

	_asm{
		pushad
		pushfd

		mov eax, nameData
		push eax
		mov edx, idx
		mov ecx, id
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_TAKE_OFF_DIRECT_CALL
		call esi

		popfd
		popad
	}

}

void takeOn2(DelphiString* name, int idx, int id) {
	auto nameData = name->data;
	_asm{
		pushad
		pushfd

		mov eax, nameData
		push eax
		mov edx, idx
		mov ecx, id
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_TAKE_ON_DIRECT_CALL
		call esi

		popfd
		popad
	}

}






void eatIndexItem(int idx){
	__asm {
		pushad
		pushfd
		mov edx, idx
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_EAT_ITEM_CALL
		call esi
		popfd
		popad
	}
}

// DWORD WINAPI EatIndexDelayThread(LPVOID lpParam) {
// 	Sleep(300);
// 	int itemIdx = reinterpret_cast<int>(lpParam);
// 	eatIndexItem(-1);
// 	return 0;
// }
// void eatWithMovingIndexItem(int girdIdx) {
// 	bagGridClick(girdIdx);
// 	CreateThread(NULL, 0, EatIndexDelayThread, reinterpret_cast<LPVOID>(girdIdx), 0, NULL);
// }
void eatItemDirectly(int id) {
	__asm {
		// 0063DD40        mov         eax,dword ptr [ebp-94] 
		// 0063DD46        push        eax
		// 0063DD47        mov         ecx,dword ptr ds:[7562EC];gvar_007562EC
		// 0063DD4D        mov         edx,dword ptr [ebp-8] // 0xFFFFFFFF 书记?
		// 0063DD50        mov         eax,dword ptr [ebp-4]
		// 0063DD53        call        006441CC

		pushad
		pushfd

		push 0 // NAME
		mov ecx, id
		mov edx ,0xFFFFFFFF
		mov eax, [FRMMAIN_ADDR]
		mov eax, [eax]
		mov esi, MIR_EAT_DIRECT_CALL
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

		mov edx, dword ptr ds:[MIC_G_NPC_ID]
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov ecx, cmdData
		mov esi, MIR_NPC_2ND_TALK_CALL
		call esi

		popfd
		popad
	}
}

void getGoodsList(DelphiString* name)
{

	auto nameData = name->data;
	__asm {
		pushad
		pushfd
		mov eax,nameData
		push eax
		mov edx,dword ptr ds:[MIC_G_NPC_ID];
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		xor ecx,ecx
		mov esi, MIR_GET_GoodsList_CALL
		call esi
		popfd
		popad
	}
}

void buyGoodsFixedIndex()
{
	__asm {
		pushad
		pushfd
		push 0
		mov ebx, MIR_GRID_FOO_ADDR
		mov ebx, [ebx]
		mov ecx, 0
		mov edx, ebx
		mov eax, [FRM_DLG_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_DMenuBuyClick_CALL
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
		push        0 // count 1或0 不懂
		mov         ecx, 0 // stock 药水不用
		mov edx,dword ptr ds:[MIC_G_NPC_ID];
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_SendBuyItem_CALL
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
		mov edx, dword ptr ds:[MIC_G_NPC_ID];
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
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
		mov edx, dword ptr ds:[MIC_G_NPC_ID];
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_REPAIR_ITEM_CALL
		call esi

		popfd
		popad
	}
}



void butch(int x, int y,int dir, int monsterId)
{
	__asm {
		pushad
		pushfd

		push dir
		push monsterId
		mov ecx, y 
		mov edx, x 
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_BUTCH_DIRECT_CALL
		call esi

		popfd
		popad
	}
}

void pickUp(){
	__asm {
		pushad
		pushfd
		mov eax, [FRMMAIN_ADDR] // gvar_:TFrmMain
		mov eax, [eax]
		mov esi, MIR_PICKUP_DIRECT_CALL
		call esi
		popfd
		popad
	}
	
}

void sendSpell(int spellId, int x, int y, int targetId){
	__asm {
		pushad
		pushfd
		push y
		push spellId
		push targetId
		mov ecx,x
		mov edx, 0x00000BC9
		mov eax, FRMMAIN_ADDR
		mov eax, [eax]
		mov esi, MIR_SPELL_DIRECT_CALL
		call esi 
		popfd
		popad
	}
	
}




void sell(DelphiString* name, int id)
{
	executeNpcFunction(name, id, MIR_SELL_ITEM_CALL);
}

void storeItem(DelphiString* name, int id)
{
	executeNpcFunction(name, id, MIR_STORE_ITEM_CALL);
}

void backStoreItem(DelphiString* name, int id)
{
	executeNpcFunction(name, id, MIR_BACK_STORE_ITEM_CALL);
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
	case 3005:
		ProcessWideString(data, [](const wchar_t* str, int length) {
			DelphiString* name = CreateDelphiString(str, length);
			getGoodsList(name);
			delete name; 
		});
		break;
	case 3006:
		// 按index随便买
		buyGoodsFixedIndex();
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
	case 3019: // 吃
		eatIndexItem(data[0]);
		break;
	case 3029: // 吃书
		eatItemDirectly(data[0]);
		break;
	case 3020: // 脱
		takeOff(data[0]);
		break;
	case 3021: // 穿
		takeOn(data[0], data[1]);
		break;

	case 3022: // 脱2
		ProcessWideString(data, [data](const wchar_t* str, int length) {
			DelphiString* name = CreateDelphiString(str, length);
			int idx = data[length + 1];
			int id = data[length + 2];
			takeOff2(name, idx, id);
			delete name;
		});
		break;
	case 3023: // 穿2
		if(MIR_TAKE_ON_DIRECT_CALL == 0) break;
		ProcessWideString(data, [data](const wchar_t* str, int length) {
			DelphiString* name = CreateDelphiString(str, length);
			int idx = data[length + 1];
			int id = data[length + 2];
			takeOn2(name, idx, id);
			delete name;
		});
		break;
	case 3030: // 屠宰
		butch(data[0], data[1], data[2], data[3]);
		break;
	case 3031: // 捡取
		pickUp();
		break;
	case 3100: // 魔法
		sendSpell(data[0], data[1], data[2], data[3]);
		break;
	default:
		break;
	}
}