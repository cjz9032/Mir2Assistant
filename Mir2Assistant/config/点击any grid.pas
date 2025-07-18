//通过IDX和bool是否选中 可以找到点击事件, 然后直接call, 参数也不复杂
// 可以点击所有的


// call DItemGridGridSelect test

pushad
pushfd

push 100 

push 0

push 0

push 0

mov ebx,[7432F4]

mov ecx,100

mov edx, [7432F4]

mov eax,[0074350C]

call 005ABB7C 

popfd
popad

--声明

push eax // Y轴

--base0
push col 1

push row 1

push ax 0 

mov ebx [7432F4]

mov ecx // X轴

mov edx  [7432F4] -- same 

mov eax [0074350C] :TFrmDlg


// cancel call
procedure TFrmDlg.CancelItemMoving;

pushad
pushfd
mov eax , [0074350C]
call 0065EA88
popfd
popad
 
// dress call for DSWWeaponClick -- sender map -> 6C4等 查询汇编得到 XY没用, 直接写100
pushad
pushfd

push 100
mov eax,[0074350C]
mov ebx, [eax+6C4] 
mov ecx,100
mov edx,ebx
call 0059F718

popfd
popad

procedure TFrmDlg.DItemGridGridSelect(Sender: TObject; ACol, ARow: Integer; Shift: TShiftState);

关注index
可以找到 g_UseItems

U_DRESS = 0;
U_WEAPON = 1;
U_RIGHTHAND = 2;
U_NECKLACE = 3;
U_HELMET = 4;
U_ARMRINGL = 5;
U_ARMRINGR = 6;
U_RINGL = 7;
U_RINGR = 8;

    begin
      g_SndMgr.ItemClickSound(g_UseItems[sel].s);
      g_MovingItem.Index := -(sel + 1);
      g_MovingItem.item := g_UseItems[sel];
      g_UseItems[sel].s.Name := '';
      g_pweapon := @g_UseItems[sel];
      g_boItemMoving := True;
    end;
DSWWeaponClick_0059F718 就看sender
FState.TFrmDlg.DSWWeaponClick 

0059F858        cmp         eax,dword ptr [edx+6C8];TFrmDlg.DSWDress:TDButton
0059F8AB        cmp         eax,dword ptr [edx+6C4];TFrmDlg.DSWWeapon:TDButton
0059F8C6        cmp         eax,dword ptr [edx+6AC];TFrmDlg.DSWNecklace:TDButton
0059F8E1        cmp         eax,dword ptr [edx+6B0];TFrmDlg.DSWLight:TDButton
0059F8FC        cmp         eax,dword ptr [edx+6CC];TFrmDlg.DSWHelmet:TDButton
0059F984        cmp         eax,dword ptr [edx+6C0];TFrmDlg.DSWRingL:TDButton
0059F99D        cmp         eax,dword ptr [edx+6BC];TFrmDlg.DSWRingR:TDButton
0059FA35        cmp         eax,dword ptr [edx+6B8];TFrmDlg.DSWArmRingL:TDButton
0059FA4E        cmp         eax,dword ptr [edx+6B4];TFrmDlg.DSWArmRingR:TDButton
0059FA7A        cmp         eax,dword ptr [edx+6EC];TFrmDlg.DSWBujuk:TDButton
0059FAAD        cmp         eax,dword ptr [edx+6EC];TFrmDlg.DSWBujuk:TDButton
0059FAE3        cmp         eax,dword ptr [edx+6B8];TFrmDlg.DSWArmRingL:TDButton
0059FB05        cmp         eax,dword ptr [edx+6F8];TFrmDlg.DSWCharm:TDButton
0059FB46        cmp         eax,dword ptr [edx+6F0];TFrmDlg.DSWBelt:TDButton
0059FB76        cmp         eax,dword ptr [edx+6F4];TFrmDlg.DSWBoots:TDButton
0059FB98        cmp         eax,dword ptr [edx+6F8];TFrmDlg.DSWCharm:TDButton
0059FBB3        cmp         eax,dword ptr [edx+0A08];TFrmDlg.ButTrans:TDButton

ZC.H+19FE55 - 8B 15 40996700        - mov edx,[ZC.H+279940] { (00752768) }
ZC.H+19FE5B - C6 04 C2  00          - mov byte ptr [edx+eax*8],00 { 0 }
ZC.H+19FE5F - A1 38A26700           - mov eax,[ZC.H+27A238] { (00755F5C) }
ZC.H+19FE64 - C6 00 01              - mov byte ptr [eax],01 { 1 }

procedure TFrmDlg.DSWWeaponClick(Sender: TObject; X, Y: Integer);


关注index找到 g_ItemArr和这个函数(其实不用,已有)

g_boItemMoving := True;
g_MovingItem.Index := idx;
g_MovingItem.item := g_ItemArr[idx];
g_ItemArr[idx].s.Name := '';
g_SndMgr.ItemClickSound(g_ItemArr[idx].s);


FState.TFrmDlg.DItemGridGridSelect