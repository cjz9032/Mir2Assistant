同样是 SendClientMessage 
很容易找到

// 点npc test call
pushad
pushfd

push 00
push 00
push 00
mov ecx , 11fc8da0
mov edx,000003F2
mov eax,[7524B4]
mov ebx,[7524B4]
call 642524

popfd
popad



ZC.H+23FF4F - 6A 00                 - push 00 { 0 }
ZC.H+23FF51 - 6A 00                 - push 00 { 0 }
ZC.H+23FF53 - 6A 00                 - push 00 { 0 }
ZC.H+23FF55 - 8B 45 E0              - mov eax,[ebp-20]
ZC.H+23FF58 - 8B 48 04              - mov ecx,[eax+04]  -- NPC ID和mons一样
ZC.H+23FF5B - BA F2030000           - mov edx,000003F2 { 1010 }
ZC.H+23FF60 - 8B 45 FC              - mov eax,[ebp-04]
ZC.H+23FF63 - E8 BC250000           - call ZC.H+242524


--2级对话
g_nCurMerchant 很容易根据ID找到 7563A4
    就能找到对应的函数, 和string这些
关闭 FrmDlg.CloseMDlg; 
    // 需要重置 否则 重复点NPC不识别
    mov  eax,[0067A728];^gvar_0074350C:TFrmDlg
    mov  eax,dword ptr [eax]
    call        005AE87C
MDlgStr: string; 
当前npcname [74350C] TFrmDlg +C38
    偏移 这个直接搜字符串 多搜几次就好 [74350C] TFrmDlg +C40 // 或者暴力OMG看也行
    UNICODE 欢迎，我能为你做点什么？
            <买/@buy>　药品
            <卖/@sell>　药品
            <打听/@XUNWENTSYS> 关于特殊药水的消息
            <退出/@exit>

 
2级对话命令 直接找g_nCurMerchant 或是 DMerchantDlgClick_005B18B8 再继续找select的call
    frmMain.SendMerchantDlgSelect(g_nCurMerchant, p.rstr); -- 006446D0

直接调用会不对, 最好不, 但是一般都可以
005B19F3        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005B19F9        mov         edx,dword ptr [edx]
005B19FB        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005B1A00        mov         eax,dword ptr [eax]
005B1A02        mov         ecx,dword ptr [ebp-18] -- cmd @sell 等的str
005B1A05        mov         ecx,dword ptr [ecx+10]
005B1A08        call        006446D0

// test 2级 call
pushad
pushfd

mov edx, 0x7563A4
mov edx, [edx]
mov eax, 0x00679EBC ;^gvar_007524B4:TFrmMain
mov eax,[0x00679EBC]
mov ecx, TODO
call 006446D0

popfd
popad


procedure TfrmMain.ClientGetMerchantSay(merchant, face: Integer; saying: string);
var
  npcname: string;
begin
  g_nMDlgX := g_MySelf.m_nCurrX;
  g_nMDlgY := g_MySelf.m_nCurrY;
  if g_nCurMerchant <> merchant then
  begin
    g_nCurMerchant := merchant;
    // FrmDlg.ResetMenuDlg;
    FrmDlg.CloseMDlg;
  end;
  saying := GetValidStr3(saying, npcname, ['/']);
  FrmDlg.ShowMDlg(face, npcname, saying);
end;

_Unit101.sub_00657FBC
...
00658001        mov         [007563AC],eax;gvar_007563AC
00658006        mov         eax,[007563A4];gvar_007563A4
0065800B        cmp         eax,dword ptr [ebp-8]
0065800E>       je          00658030
clear
00658010        mov         eax,dword ptr [ebp-8]
00658013        mov         [007563A4],eax;gvar_007563A4
00658018        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
0065801D        mov         eax,dword ptr [eax]
0065801F        call        005ADDF4
00658024        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
00658029        mov         eax,dword ptr [eax]
0065802B        call        005AE87C
say string
00658030        push        0
00658032        lea         eax,[ebp-14]
00658035        push        eax
...
// menuList的长度, 暂时没找属性 没意义
 [[[0x74350C]+0x00000C5C]+08]

一般购买 
// 第一个是获取列表的函数 
frmMain.SendGetDetailItem(g_nCurMerchant, 0, pg.Name);
ZC.H+1B11E8 - 8B 45 E0              - mov eax,[ebp-20]
ZC.H+1B11EB - 50                    - push eax
ZC.H+1B11EC - 8B 15 E8996700        - mov edx,[ZC.H+2799E8] { (007563A4) }
ZC.H+1B11F2 - 8B 12                 - mov edx,[edx]
ZC.H+1B11F4 - A1 BC9E6700           - mov eax,[ZC.H+279EBC] { (007524B4) }
ZC.H+1B11F9 - 8B 00                 - mov eax,[eax]
ZC.H+1B11FB - 33 C9                 - xor ecx,ecx
ZC.H+1B11FD - E8 AA420900           - call ZC.H+2454AC

// test call
push eax // name
mov edx,ptr 0x6799E8
mov edx,[edx]
mov eax,ptr 0x679EBC
mov eax,[eax]
xor ecx,ecx
mov esi, 0x6454AC
call esi

// 使用选index 购买 ZC.H+34350C C6C

// 调用 DMenuBuyClick
2. 直接call内部
// DMenuBuyClick, 一般就在最后, 
frmMain.SendBuyItem(g_nCurMerchant, pg.Stock, pg.Name, Word(Count)) 006459F4
  g_nCurMerchant 可以拿到
  pg.Name 就随便找个地方赋值delphistr
  stock有点骑怪, 但是不影响, 貌似可以瞎写 
  count为0自动会重置1
  注意name count因为push所以相反, name再c, 这样pop是c再n

 从 FState.TFrmDlg.DMenuBuyClick_005B112C 找到
  005B14DD        mov         eax,dword ptr [eax]
  005B14DF        call        006459F4 // SendBuyItem 

// 买/卖/修/存 都是 ... 用完刷新
// 修特殊加了

// 仓库UI很多, 所以都直接找call 
// 存
SendStorageItem 006452EC -- 从通用里找 DSellDlgOkClick 多断几个就找到了
procedure TfrmMain.SendStorageItem(merchant, itemindex: Integer; itemname: string; count: Word);
  同上, count随便写1就好了

// 取
SendTakeBackStorageItem 645B6C -- 从通用里找 DMenuBuyClick 多断几个就找到了
procedure TfrmMain.SendTakeBackStorageItem(merchant, itemserverindex: Integer; itemname: string; count: Word);


// 卖
dmSell: frmMain.SendSellItem(g_nCurMerchant, g_SellDlgItem.MakeIndex, g_SellDlgItem.s.Name, g_SellDlgItem.Dura);








// -------------------------------------------------BACKUP 以下 2个函数都是通用的 卖修存 as BACKUP

---- 建议用对应的 UI状态和服务器更统一, 不要直接调发包, 但是暂时不想搞这么麻烦了, 直接发包 刷新就可以了




// 声明 DSellDlgOkClick
push x
mov ebx  [7432F4] // 随便炒另一个 因为没用 所以写的不报错就行
mov ecx,y
mov edx,ebx
mov  eax,[0067A728];^gvar_0074350C:TFrmDlg
mov  eax,dword ptr [eax]
call TFrmDlg.DSellDlgOkClick 005B2380

// test call DSellDlgOkClick
pushad
pushfd

push 100
mov ebx, [7432F4]
mov ecx, 100
mov edx,ebx
mov  eax,[0067A728]
mov  eax,dword ptr [eax]
call 005B2380

popfd
popad


// test call DSellDlgSpotClick 005B1D78 其实都是一样的随便写
pushad
pushfd

push 100
mov ebx, [7432F4]
mov ecx, 100
mov edx,ebx
mov  eax,[0067A728]
mov  eax,dword ptr [eax]
call 005B1D78

popfd
popad

---- 建议用对应的 UI状态和服务器更统一, 不要直接调发包, 但是暂时不想搞这么麻烦了, 直接发包 刷新就可以了 end