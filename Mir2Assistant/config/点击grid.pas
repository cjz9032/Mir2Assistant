ZC.H+1ABD11 - 8D 79 04              - lea edi,[ecx+04]
ZC.H+1ABD14 - B9 20000000           - mov ecx,00000020 { 32 }
ZC.H+1ABD19 - F3 A5                 - repe movsd 
ZC.H+1ABD1B - 8B 45 F8              - mov eax,[ebp-08]
ZC.H+1ABD1E - C1 E0 04              - shl eax,04 { 4 }
ZC.H+1ABD21 - 8B 15 48A56700        - mov edx,[ZC.H+27A548] { (00752EE8) }


procedure TFrmDlg.DItemGridGridSelect(Sender: TObject; ACol, ARow: Integer; Shift: TShiftState);
var
  idx, mi: Integer;
  temp: TClientItem;
  MsgResult, Count: Integer;
  valstr: string;
  keyvalue: TKeyBoardState;
  StallItem: TClientStall;
  DefMsg: TDefaultMessage;
begin
  //if (g_WaitingUseItem.Item.S.Name <> '') then Exit;
  if DItemGrid.tButton = mbLeft then
  begin
    FillChar(keyvalue, SizeOf(TKeyBoardState), #0);
    GetKeyboardState(keyvalue);
    idx := ACol + ARow * DItemGrid.ColCount + 6;

    if idx in [6..MAXBAGITEM - 1] then
    begin
      if not g_boItemMoving then
      begin
        if g_ItemArr[idx].s.Name <> '' then
        begin

          if g_ItemArr[idx].s.NeedIdentify = 4 then
          begin
            g_ItemArr[idx].s.NeedIdentify := 0;
            DelStallItem(g_ItemArr[idx]);
          end
          else if g_ItemArr[idx].s.NeedIdentify = 5 then
            Exit;

          if (g_ItemArr[idx].s.Overlap > 0) and (g_ItemArr[idx].Dura > 1) and (Shift = [{ssCtrl,} ssShift]) then
          begin

            MsgResult := DMessageDlg(Format('你想拆分多少 %s ？', [g_ItemArr[idx].s.Name]), [mbOk, mbCancel, mbAbort], IntToStr(g_ItemArr[idx].Dura - 1));

            GetValidStrVal(DlgEditText, valstr, [' ']);
            Count := Str_ToInt(valstr, 0);

            if Count >= g_ItemArr[idx].Dura then  Count := g_ItemArr[idx].Dura - 1;

            if (MsgResult = mrCancel) or (Count <= 0) then  Exit;

            frmMain.SendDismantleItem(g_ItemArr[idx].s.Name, g_ItemArr[idx].MakeIndex, Count, 0);

          end
          else
          begin
            g_boItemMoving := True;
            g_MovingItem.Index := idx;
            g_MovingItem.item := g_ItemArr[idx];
            g_ItemArr[idx].s.Name := '';
            g_SndMgr.ItemClickSound(g_ItemArr[idx].s);
          end;
        end;
      end
      else
      begin
        mi := g_MovingItem.Index;
        if IsDetectItem(mi) then
        begin
          g_WaitingDetectItem := g_MovingItem;
          g_boItemMoving := False;
          g_MovingItem.Index := 0;
          g_MovingItem.item.s.Name := '';
          DefMsg := EDcode.MakeDefaultMsg(CM_MoveDetectItem, g_WaitingDetectItem.item.MakeIndex, 0, 0, 0);
          frmMain.SendSocket(EncodeMessage(DefMsg));
          Exit;
        end;

        if IsStallItem(mi) then
        begin
          if g_Myself.m_StallMgr.OnSale then
          begin
            //if g_WaitingStallItem.Item.s.Name <> '' then exit;
            g_WaitingStallItem := g_MovingItem;
            g_boItemMoving := False;
            g_MovingItem.Index := 0;
            g_MovingItem.item.s.Name := '';

            StallItem.MakeIndex := g_WaitingStallItem.item.MakeIndex;
            DefMsg := EDcode.MakeDefaultMsg(CM_UPDATESTALLITEM, 0, 0, 0, 0);
            frmMain.SendSocket(EncodeMessage(DefMsg) + EncodeBuffer(@StallItem, SizeOf(StallItem)));
          end
          else
          begin
            UpdateBagStallItem(g_MovingItem.item, 0);
            DelStallItem(g_MovingItem.item);
            g_boItemMoving := False;
            g_MovingItem.Index := 0;
            g_MovingItem.item.s.Name := '';
          end;
          Exit;
        end;
        if (DDealDlg.Visible) then
        begin
          if (mi >= 0) and (mi < 6) then
          begin
            CancelItemMoving;
            DMessageDlg('[注意] 交易时不能将物品放到包裹中', [mbOk]);
            Exit;
          end;
        end;
        if (mi = -97) or (mi = -98) then
          Exit;
        if (mi < (0 - HERO_MIIDX_OFFSET)) and (mi >= -((U_FASHION + 1) + HERO_MIIDX_OFFSET)) then
          Exit;

        if (mi < 0) and (mi >= -(U_FASHION + 1)) then
        begin
          g_SndMgr.ItemClickSound(g_MovingItem.item.s);
          g_WaitingUseItem := g_MovingItem;
          frmMain.SendTakeOffItem(-(g_MovingItem.Index + 1), g_MovingItem.item.MakeIndex, g_MovingItem.item.s.Name);
          g_MovingItem.item.s.Name := '';
          g_boItemMoving := False;
        end
        else
        begin
          if (mi <= -20) and (mi > -30) then
          begin
            DealItemReturnBag(g_MovingItem.item);
            if g_MovingItem.item.s.Overlap > 0 then
            begin
              g_MovingItem.item.s.Name := '';
              g_boItemMoving := False;
              Exit;
            end;
          end
          else if (mi <= -30) and (mi > -39) then
            ; //AddYbDealItem(g_MovingItem.Item);

          if mi >= HERO_MIIDX_OFFSET then
          begin //英雄物品->主人包裹
            if (g_Myself = nil) or (g_Myself.m_HeroObject = nil) or (g_Myself.m_HeroObject.m_boDeath) then
              Exit;
            g_WaitingUseItem := g_MovingItem;
            frmMain.SendHeroItemToMasterBag(g_MovingItem.item.MakeIndex, g_MovingItem.item.s.Name);
            g_MovingItem.item.s.Name := '';
            g_boItemMoving := False;
            g_SndMgr.ItemClickSound(g_MovingItem.item.s);
          end
          else
          begin
            if g_ItemArr[idx].s.Name <> '' then
            begin

              if g_ItemArr[idx].s.NeedIdentify = 4 then
              begin
                g_ItemArr[idx].s.NeedIdentify := 0;
                DelStallItem(g_ItemArr[idx]);
              end
              else if g_ItemArr[idx].s.NeedIdentify = 5 then
                Exit;

              if (g_ItemArr[idx].s.Overlap > 0) and (g_ItemArr[idx].s.Name = g_MovingItem.item.s.Name) then
              begin

                frmMain.SendItemSumCount(g_ItemArr[idx].MakeIndex, g_MovingItem.item.MakeIndex, 0, g_ItemArr[idx].s.Name, g_MovingItem.item.s.Name);

                //if (mi > 0) and (mi < 100) then
                g_WaitingUseItem := g_MovingItem;
                //CancelItemMoving;
                //else begin
                g_MovingItem.item.s.Name := '';
                g_boItemMoving := False;
                //end;
              end
              else
              begin
                temp := g_ItemArr[idx];
                g_ItemArr[idx] := g_MovingItem.item;
                g_MovingItem.Index := idx;
                g_MovingItem.item := temp;
                g_SndMgr.ItemClickSound(g_MovingItem.item.s);
              end;
            end
            else
            begin
              g_ItemArr[idx] := g_MovingItem.item;
              g_MovingItem.item.s.Name := '';
              g_boItemMoving := False;
              g_SndMgr.ItemClickSound(g_MovingItem.item.s);
            end;
          end;
        end;
      end;
    end;
    ArrangeItembag;
    Exit;
  end;

  if DStorageViewDlg.Visible then  //增加右键点击移入仓库 2019-12-21
  begin
    idx := ACol + ARow * DItemGrid.ColCount + 6;
    if (g_ItemArr[idx].s.Name <> '') and (idx in [6..MAXBAGITEM - 1]) then
    begin
      g_boItemMoving := False;
      g_MovingItem.Index := idx;
      g_MovingItem.item := g_ItemArr[idx];
      g_ItemArr[idx].s.Name := '';
      frmMain.SendStorageItemView(g_nCurMerchant, g_MovingItem.item.MakeIndex, g_MovingItem.item.s.Name, g_MovingItem.item.Dura);
    end;
  end
  else if DHeroItemBag.Visible  then //必须英雄背包打开时，将主人物品放入英雄背包中 2019-12-26
  begin
    if (g_Myself = nil) or (g_Myself.m_HeroObject = nil) then
      Exit;
    if g_Myself.m_StallMgr.OnSale then
      Exit;
    if g_WaitingUseItem.item.s.Name <> '' then
      Exit;
    idx := ACol + ARow * DItemGrid.ColCount + 6;
    if idx in [6..MAXBAGITEM - 1] then
    begin
      if not g_boItemMoving then
      begin
        if g_ItemArr[idx].s.Name <> '' then
        begin
          if (idx = -97) or (idx = -98) then
            Exit;
          if (idx < 0) and (idx >= -(U_FASHION + 1)) then
            Exit;
          if (idx <= -20) and (idx > -39) then
            Exit;
          if idx < HERO_MIIDX_OFFSET then
          begin
            g_WaitingUseItem.Index := idx;
            g_WaitingUseItem.item := g_ItemArr[idx];
            g_WaitingUseItem.item.s.NeedIdentify := 0;
            frmMain.SendMasterItemToHeroBag(g_WaitingUseItem.item.MakeIndex, g_WaitingUseItem.item.s.Name);
            g_ItemArr[idx].s.Name := '';
            g_ItemArr[idx].s.NeedIdentify := 0;
            g_SndMgr.ItemClickSound(g_WaitingUseItem.item.s);
            ArrangeHeroItembag;
          end;
        end;
      end;
    end;
  end;
end;



























FState.TFrmDlg.DItemGridGridSelect
005ABB7C        push        ebp
005ABB7D        mov         ebp,esp
005ABB7F        push        ecx
005ABB80        mov         ecx,28
005ABB85        push        0
005ABB87        push        0
005ABB89        dec         ecx
005ABB8A>       jne         005ABB85
005ABB8C        xchg        ecx,dword ptr [ebp-4]
005ABB8F        push        ebx
005ABB90        push        esi
005ABB91        push        edi
005ABB92        mov         dword ptr [ebp-24],ecx
005ABB95        mov         dword ptr [ebp-20],edx
005ABB98        mov         dword ptr [ebp-4],eax
005ABB9B        xor         eax,eax
005ABB9D        push        ebp
005ABB9E        push        5AC444
005ABBA3        push        dword ptr fs:[eax]
005ABBA6        mov         dword ptr fs:[eax],esp
005ABBA9        mov         byte ptr [ebp-0D],0
005ABBAD        mov         eax,dword ptr [ebp-4]
005ABBB0        mov         eax,dword ptr [eax+508];TFrmDlg.DItemGrid:TDGrid
005ABBB6        mov         eax,dword ptr [eax+340];TDGrid.ColCount:Integer
005ABBBC        imul        dword ptr [ebp+0C]
005ABBBF        add         eax,dword ptr [ebp+10]
005ABBC2        add         eax,6
005ABBC5        mov         dword ptr [ebp-8],eax
005ABBC8        mov         eax,[00679B8C];^gvar_00752764
005ABBCD        cmp         dword ptr [eax],0
005ABBD0>       je          005AC3FE
005ABBD6        mov         eax,[00679B8C];^gvar_00752764
005ABBDB        mov         eax,dword ptr [eax]
005ABBDD        mov         eax,dword ptr [eax+36C]
005ABBE3        cmp         byte ptr [eax+6],0
005ABBE7>       jne         005AC3FE
005ABBED        mov         eax,[0067A238];^gvar_00755F5C
005ABBF2        cmp         byte ptr [eax],0
005ABBF5>       jne         005ABC7F
005ABBFB        mov         eax,dword ptr [ebp-8]
005ABBFE        shl         eax,4
005ABC01        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABC07        cmp         byte ptr [edx+eax*8],0
005ABC0B>       je          005ABC7F
005ABC0D        test        byte ptr [ebp+8],10
005ABC11>       je          005ABC7F
005ABC13        call        kernel32.GetTickCount
005ABC18        mov         edx,dword ptr ds:[67A6E8];^gvar_007562F8:DWORD
005ABC1E        mov         edx,dword ptr [edx]
005ABC20        add         edx,12C
005ABC26        cmp         eax,edx
005ABC28>       jbe         005ABC7F
005ABC2A        mov         eax,dword ptr [ebp-8]
005ABC2D        shl         eax,4
005ABC30        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABC36        cmp         byte ptr [edx+eax*8+0F],4
005ABC3B>       jae         005ABC7F
005ABC3D        mov         eax,dword ptr [ebp-8]
005ABC40        shl         eax,4
005ABC43        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABC49        cmp         byte ptr [edx+eax*8+0F],3
005ABC4E>       jne         005ABC6B
005ABC50        mov         eax,dword ptr [ebp-8]
005ABC53        shl         eax,4
005ABC56        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABC5C        mov         al,byte ptr [edx+eax*8+10]
005ABC60        dec         eax
005ABC61        sub         al,6
005ABC63>       jb          005ABC7F
005ABC65        add         al,0FE
005ABC67        sub         al,3
005ABC69>       jb          005ABC7F
005ABC6B        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005ABC70        mov         eax,dword ptr [eax]
005ABC72        mov         edx,dword ptr [ebp-8]
005ABC75        call        0063D914
005ABC7A>       jmp         005AC3FE
005ABC7F        mov         eax,dword ptr [ebp-8]
005ABC82        add         eax,0FFFFFFFA
005ABC85        sub         eax,28
005ABC88>       jae         005AC3F9
005ABC8E        mov         eax,[0067A238];^gvar_00755F5C
005ABC93        cmp         byte ptr [eax],0
005ABC96>       jne         005ABD44
005ABC9C        mov         eax,dword ptr [ebp-8]
005ABC9F        shl         eax,4
005ABCA2        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABCA8        cmp         dword ptr [edx+eax*8+2C],2C
005ABCAD>       je          005AC3FE
005ABCB3        mov         eax,dword ptr [ebp-8]
005ABCB6        shl         eax,4
005ABCB9        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABCBF        cmp         dword ptr [edx+eax*8+2C],2D
005ABCC4>       je          005AC3FE
005ABCCA        mov         eax,dword ptr [ebp-8]
005ABCCD        shl         eax,4
005ABCD0        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABCD6        cmp         byte ptr [edx+eax*8],0
005ABCDA>       je          005AC3F9
005ABCE0        mov         dword ptr ds:[677198],2;gvar_00677198
005ABCEA        mov         eax,[0067A238];^gvar_00755F5C
005ABCEF        mov         byte ptr [eax],1
005ABCF2        mov         eax,[0067A240];^gvar_00755F60:dword
005ABCF7        mov         edx,dword ptr [ebp-8]
005ABCFA        mov         dword ptr [eax],edx
005ABCFC        mov         eax,dword ptr [ebp-8]
005ABCFF        shl         eax,4
005ABD02        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABD08        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
005ABD0E        lea         esi,[edx+eax*8]
005ABD11        lea         edi,[ecx+4]
005ABD14        mov         ecx,20
005ABD19        rep movs    dword ptr [edi],dword ptr [esi]
005ABD1B        mov         eax,dword ptr [ebp-8]
005ABD1E        shl         eax,4
005ABD21        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABD27        mov         byte ptr [edx+eax*8],0
005ABD2B        mov         eax,dword ptr [ebp-8]
005ABD2E        shl         eax,4
005ABD31        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABD37        lea         eax,[edx+eax*8]
005ABD3A        call        00552B9C
005ABD3F>       jmp         005AC3F9
005ABD44        mov         eax,dword ptr [ebp-4]
005ABD47        cmp         byte ptr [eax+0C78],0;TFrmDlg.?fC78:byte
005ABD4E>       je          005ABD5D
005ABD50        cmp         dword ptr ds:[677198],1;gvar_00677198
005ABD57>       je          005AC24D
005ABD5D        mov         eax,[0067A240];^gvar_00755F60:dword
005ABD62        mov         eax,dword ptr [eax]
005ABD64        mov         dword ptr [ebp-0C],eax
005ABD67        mov         eax,dword ptr [ebp-0C]
005ABD6A        call        0058FE50
005ABD6F        test        al,al
005ABD71>       je          005ABD93
005ABD73        mov         eax,[0067A240];^gvar_00755F60:dword
005ABD78        xor         edx,edx
005ABD7A        mov         dword ptr [eax+30],edx
005ABD7D        mov         eax,[0067A240];^gvar_00755F60:dword
005ABD82        add         eax,4
005ABD85        call        005D6DA4
005ABD8A        mov         eax,[0067A240];^gvar_00755F60:dword
005ABD8F        mov         byte ptr [eax+4],0
005ABD93        mov         eax,dword ptr [ebp-4]
005ABD96        mov         eax,dword ptr [eax+5D0];TFrmDlg.DMakeItemDlg:TDWindow
005ABD9C        cmp         byte ptr [eax+300],0;TDWindow.Visible:Boolean
005ABDA3>       jne         005ABDB7
005ABDA5        mov         eax,dword ptr [ebp-4]
005ABDA8        mov         eax,dword ptr [eax+41C];TFrmDlg.DDealDlg:TDWindow
005ABDAE        cmp         byte ptr [eax+300],0;TDWindow.Visible:Boolean
005ABDB5>       je          005ABE25
005ABDB7        cmp         dword ptr [ebp-0C],0
005ABDBB>       jl          005ABE25
005ABDBD        cmp         dword ptr [ebp-0C],6
005ABDC1>       jge         005ABE25
005ABDC3        mov         eax,dword ptr [ebp-4]
005ABDC6        call        00599734
005ABDCB        mov         eax,dword ptr [ebp-4]
005ABDCE        mov         eax,dword ptr [eax+5D0];TFrmDlg.DMakeItemDlg:TDWindow
005ABDD4        cmp         byte ptr [eax+300],0;TDWindow.Visible:Boolean
005ABDDB>       je          005ABDF6
005ABDDD        mov         cx,word ptr ds:[5AC454];0x4 gvar_005AC454
005ABDE4        mov         edx,5AC464;'当事项的制作中,\展品不可以从一地转移到带窗前包窗口.'
005ABDE9        mov         eax,dword ptr [ebp-4]
005ABDEC        call        0059A75C
005ABDF1>       jmp         005AC3FE
005ABDF6        mov         eax,dword ptr [ebp-4]
005ABDF9        mov         eax,dword ptr [eax+41C];TFrmDlg.DDealDlg:TDWindow
005ABDFF        cmp         byte ptr [eax+300],0;TDWindow.Visible:Boolean
005ABE06>       je          005AC3FE
005ABE0C        mov         cx,word ptr ds:[5AC454];0x4 gvar_005AC454
005ABE13        mov         edx,5AC4A8;'在交换项目,\展品不可以从一地转移到带窗前包窗口.'
005ABE18        mov         eax,dword ptr [ebp-4]
005ABE1B        call        0059A75C
005ABE20>       jmp         005AC3FE
005ABE25        cmp         dword ptr [ebp-0C],0FFFFFF9F
005ABE29>       je          005AC3FE
005ABE2F        cmp         dword ptr [ebp-0C],0FFFFFF9E
005ABE33>       je          005AC3FE
005ABE39        cmp         dword ptr [ebp-0C],0
005ABE3D>       jge         005ABEAC
005ABE3F        cmp         dword ptr [ebp-0C],0FFFFFFF1
005ABE43>       jl          005ABEAC
005ABE45        mov         eax,[0067A240];^gvar_00755F60:dword
005ABE4A        mov         edx,dword ptr ds:[67A3E8];^gvar_007561F4
005ABE50        mov         esi,eax
005ABE52        mov         edi,edx
005ABE54        mov         ecx,21
005ABE59        rep movs    dword ptr [edi],dword ptr [esi]
005ABE5B        lea         eax,[ebp-0AC]
005ABE61        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005ABE67        add         edx,4
005ABE6A        call        @UStrFromString
005ABE6F        mov         eax,dword ptr [ebp-0AC]
005ABE75        push        eax
005ABE76        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005ABE7C        mov         dl,byte ptr [edx]
005ABE7E        inc         edx
005ABE7F        neg         dl
005ABE81        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
005ABE87        mov         ecx,dword ptr [ecx+78]
005ABE8A        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005ABE8F        mov         eax,dword ptr [eax]
005ABE91        call        006440F4
005ABE96        mov         eax,[0067A240];^gvar_00755F60:dword
005ABE9B        mov         byte ptr [eax+4],0
005ABE9F        mov         eax,[0067A238];^gvar_00755F5C
005ABEA4        mov         byte ptr [eax],0
005ABEA7>       jmp         005AC3F9
005ABEAC        cmp         dword ptr [ebp-0C],0FFFFFFEC
005ABEB0>       jg          005ABEEA
005ABEB2        cmp         dword ptr [ebp-0C],0FFFFFFE2
005ABEB6>       jle         005ABEEA
005ABEB8        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005ABEBE        add         edx,4
005ABEC1        mov         eax,dword ptr [ebp-4]
005ABEC4        call        005B7D0C
005ABEC9        mov         eax,[0067A240];^gvar_00755F60:dword
005ABECE        cmp         byte ptr [eax+52],0
005ABED2>       jbe         005ABEEA
005ABED4        mov         eax,[0067A240];^gvar_00755F60:dword
005ABED9        mov         byte ptr [eax+4],0
005ABEDD        mov         eax,[0067A238];^gvar_00755F5C
005ABEE2        mov         byte ptr [eax],0
005ABEE5>       jmp         005AC3FE
005ABEEA        mov         eax,dword ptr [ebp-8]
005ABEED        shl         eax,4
005ABEF0        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABEF6        cmp         byte ptr [edx+eax*8],0
005ABEFA>       je          005AC218
005ABF00        test        byte ptr [ebp+8],4
005ABF04>       je          005ABFEF
005ABF0A        mov         eax,[0067A240];^gvar_00755F60:dword
005ABF0F        mov         al,byte ptr [eax+13]
005ABF12        add         al,0C4
005ABF14        sub         al,2
005ABF16>       jae         005ABFEB
005ABF1C        mov         eax,[0067A240];^gvar_00755F60:dword
005ABF21        cmp         byte ptr [eax+13],3D
005ABF25>       jne         005ABF39
005ABF27        mov         eax,[0067A240];^gvar_00755F60:dword
005ABF2C        mov         al,byte ptr [eax+14]
005ABF2F        add         al,0EC
005ABF31        sub         al,2
005ABF33>       jb          005ABFEB
005ABF39        mov         eax,dword ptr [ebp-8]
005ABF3C        shl         eax,4
005ABF3F        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005ABF45        lea         edx,[edx+eax*8]
005ABF48        lea         eax,[ebp-0C8]
005ABF4E        call        @PStrCpy
005ABF53        mov         edx,5AC4DC;' 使用 '
005ABF58        lea         eax,[ebp-0C8]
005ABF5E        mov         cl,14
005ABF60        call        @PStrNCat
005ABF65        lea         edx,[ebp-0C8]
005ABF6B        lea         eax,[ebp-0EC]
005ABF71        call        @PStrCpy
005ABF76        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005ABF7C        add         edx,4
005ABF7F        lea         eax,[ebp-0EC]
005ABF85        mov         cl,22
005ABF87        call        @PStrNCat
005ABF8C        lea         edx,[ebp-0EC]
005ABF92        lea         eax,[ebp-11C]
005ABF98        call        @PStrCpy
005ABF9D        mov         edx,5AC4E4;' 进行升级?'
005ABFA2        lea         eax,[ebp-11C]
005ABFA8        mov         cl,2C
005ABFAA        call        @PStrNCat
005ABFAF        lea         edx,[ebp-11C]
005ABFB5        lea         eax,[ebp-0B0]
005ABFBB        call        @UStrFromString
005ABFC0        mov         edx,dword ptr [ebp-0B0]
005ABFC6        mov         cx,word ptr ds:[5AC4F0];0xC gvar_005AC4F0
005ABFCD        mov         eax,dword ptr [ebp-4]
005ABFD0        call        0059A75C
005ABFD5        dec         eax
005ABFD6>       jne         005ABFDE
005ABFD8        mov         byte ptr [ebp-0D],1
005ABFDC>       jmp         005ABFEF
005ABFDE        mov         eax,dword ptr [ebp-4]
005ABFE1        call        00599734
005ABFE6>       jmp         005AC3FE
005ABFEB        mov         byte ptr [ebp-0D],1
005ABFEF        cmp         byte ptr [ebp-0D],0
005ABFF3>       je          005AC0A6
005ABFF9        mov         eax,dword ptr [ebp-8]
005ABFFC        shl         eax,4
005ABFFF        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC005        mov         ecx,dword ptr ds:[67A698];^gvar_00756174:ShortString
005AC00B        lea         esi,[edx+eax*8]
005AC00E        mov         edi,ecx
005AC010        mov         ecx,20
005AC015        rep movs    dword ptr [edi],dword ptr [esi]
005AC017        lea         eax,[ebp-120]
005AC01D        mov         edx,dword ptr [ebp-8]
005AC020        shl         edx,4
005AC023        mov         ecx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC029        lea         edx,[ecx+edx*8]
005AC02C        call        @UStrFromString
005AC031        mov         eax,dword ptr [ebp-120]
005AC037        push        eax
005AC038        lea         eax,[ebp-124]
005AC03E        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC044        add         edx,4
005AC047        call        @UStrFromString
005AC04C        mov         eax,dword ptr [ebp-124]
005AC052        push        eax
005AC053        mov         eax,dword ptr [ebp-8]
005AC056        shl         eax,4
005AC059        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC05F        mov         edx,dword ptr [edx+eax*8+74]
005AC063        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC069        mov         ecx,dword ptr [ecx+78]
005AC06C        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005AC071        mov         eax,dword ptr [eax]
005AC073        call        006442AC
005AC078        mov         eax,[0067A240];^gvar_00755F60:dword
005AC07D        add         eax,4
005AC080        or          edx,0FFFFFFFF
005AC083        call        005D678C
005AC088        test        al,al
005AC08A>       je          005AC3F9
005AC090        mov         eax,[0067A240];^gvar_00755F60:dword
005AC095        mov         byte ptr [eax+4],0
005AC099        mov         eax,[0067A238];^gvar_00755F5C
005AC09E        mov         byte ptr [eax],0
005AC0A1>       jmp         005AC3F9
005AC0A6        mov         eax,dword ptr [ebp-8]
005AC0A9        shl         eax,4
005AC0AC        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC0B2        cmp         byte ptr [edx+eax*8+4E],0
005AC0B7>       jbe         005AC18B
005AC0BD        mov         eax,dword ptr [ebp-8]
005AC0C0        shl         eax,4
005AC0C3        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC0C9        lea         eax,[edx+eax*8]
005AC0CC        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC0D2        add         edx,4
005AC0D5        xor         ecx,ecx
005AC0D7        mov         cl,byte ptr [eax]
005AC0D9        inc         ecx
005AC0DA        call        @AStrCmp
005AC0DF>       jne         005AC18B
005AC0E5        mov         eax,dword ptr [ebp-4]
005AC0E8        mov         eax,dword ptr [eax+5D0];TFrmDlg.DMakeItemDlg:TDWindow
005AC0EE        cmp         byte ptr [eax+300],0;TDWindow.Visible:Boolean
005AC0F5>       jne         005AC18B
005AC0FB        lea         eax,[ebp-128]
005AC101        mov         edx,dword ptr [ebp-8]
005AC104        shl         edx,4
005AC107        mov         ecx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC10D        lea         edx,[ecx+edx*8]
005AC110        call        @UStrFromString
005AC115        mov         eax,dword ptr [ebp-128]
005AC11B        push        eax
005AC11C        lea         eax,[ebp-12C]
005AC122        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC128        add         edx,4
005AC12B        call        @UStrFromString
005AC130        mov         eax,dword ptr [ebp-12C]
005AC136        push        eax
005AC137        mov         eax,dword ptr [ebp-8]
005AC13A        shl         eax,4
005AC13D        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC143        mov         edx,dword ptr [edx+eax*8+74]
005AC147        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC14D        mov         ecx,dword ptr [ecx+78]
005AC150        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005AC155        mov         eax,dword ptr [eax]
005AC157        call        006443C8
005AC15C        cmp         dword ptr [ebp-0C],0
005AC160>       jle         005AC175
005AC162        cmp         dword ptr [ebp-0C],64
005AC166>       jge         005AC175
005AC168        mov         eax,dword ptr [ebp-4]
005AC16B        call        00599734
005AC170>       jmp         005AC3F9
005AC175        mov         eax,[0067A240];^gvar_00755F60:dword
005AC17A        mov         byte ptr [eax+4],0
005AC17E        mov         eax,[0067A238];^gvar_00755F5C
005AC183        mov         byte ptr [eax],0
005AC186>       jmp         005AC3F9
005AC18B        mov         eax,dword ptr [ebp-8]
005AC18E        shl         eax,4
005AC191        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC197        cmp         dword ptr [edx+eax*8+2C],2C
005AC19C>       je          005AC3FE
005AC1A2        mov         eax,dword ptr [ebp-8]
005AC1A5        shl         eax,4
005AC1A8        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC1AE        cmp         dword ptr [edx+eax*8+2C],2D
005AC1B3>       je          005AC3FE
005AC1B9        mov         eax,dword ptr [ebp-8]
005AC1BC        shl         eax,4
005AC1BF        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC1C5        lea         esi,[edx+eax*8]
005AC1C8        lea         edi,[ebp-0A8]
005AC1CE        mov         ecx,20
005AC1D3        rep movs    dword ptr [edi],dword ptr [esi]
005AC1D5        mov         eax,dword ptr [ebp-8]
005AC1D8        shl         eax,4
005AC1DB        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC1E1        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC1E7        lea         edi,[edx+eax*8]
005AC1EA        lea         esi,[ecx+4]
005AC1ED        mov         ecx,20
005AC1F2        rep movs    dword ptr [edi],dword ptr [esi]
005AC1F4        mov         eax,[0067A240];^gvar_00755F60:dword
005AC1F9        mov         edx,dword ptr [ebp-8]
005AC1FC        mov         dword ptr [eax],edx
005AC1FE        mov         eax,[0067A240];^gvar_00755F60:dword
005AC203        lea         edi,[eax+4]
005AC206        lea         esi,[ebp-0A8]
005AC20C        mov         ecx,20
005AC211        rep movs    dword ptr [edi],dword ptr [esi]
005AC213>       jmp         005AC3F9
005AC218        mov         eax,dword ptr [ebp-8]
005AC21B        shl         eax,4
005AC21E        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
005AC224        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC22A        lea         edi,[edx+eax*8]
005AC22D        lea         esi,[ecx+4]
005AC230        mov         ecx,20
005AC235        rep movs    dword ptr [edi],dword ptr [esi]
005AC237        mov         eax,[0067A240];^gvar_00755F60:dword
005AC23C        mov         byte ptr [eax+4],0
005AC240        mov         eax,[0067A238];^gvar_00755F5C
005AC245        mov         byte ptr [eax],0
005AC248>       jmp         005AC3F9
005AC24D        mov         eax,dword ptr [ebp-4]
005AC250        cmp         byte ptr [eax+0C78],0;TFrmDlg.?fC78:byte
005AC257>       jne         005AC266
005AC259        cmp         dword ptr ds:[677198],1;gvar_00677198
005AC260>       jne         005AC3FE
005AC266        mov         eax,[0067A240];^gvar_00755F60:dword
005AC26B        cmp         byte ptr [eax+52],0
005AC26F>       jbe         005AC39F
005AC275        mov         eax,[0067A240];^gvar_00755F60:dword
005AC27A        movzx       eax,word ptr [eax+7C]
005AC27E        mov         edx,dword ptr [ebp-4]
005AC281        mov         dword ptr [edx+0C90],eax;TFrmDlg.?fC90:dword
005AC287        mov         eax,dword ptr [ebp-4]
005AC28A        cmp         dword ptr [eax+0C90],1;TFrmDlg.?fC90:dword
005AC291>       jne         005AC2AE
005AC293        mov         eax,dword ptr [ebp-4]
005AC296        add         eax,0CB0;TFrmDlg.?fCB0:string
005AC29B        mov         edx,5AC500;'1'
005AC2A0        call        @UStrAsg
005AC2A5        mov         dword ptr [ebp-14],1
005AC2AC>       jmp         005AC2FA
005AC2AE        push        5AC510;'当前数量 '
005AC2B3        lea         edx,[ebp-134]
005AC2B9        mov         eax,[0067A240];^gvar_00755F60:dword
005AC2BE        movzx       eax,word ptr [eax+7C]
005AC2C2        call        IntToStr
005AC2C7        push        dword ptr [ebp-134]
005AC2CD        push        5AC528;' 个.\请输入你想购买的商品数量?'
005AC2D2        lea         eax,[ebp-130]
005AC2D8        mov         edx,3
005AC2DD        call        @UStrCatN
005AC2E2        mov         edx,dword ptr [ebp-130]
005AC2E8        mov         cx,word ptr ds:[5AC54C];0x10 gvar_005AC54C
005AC2EF        mov         eax,dword ptr [ebp-4]
005AC2F2        call        005C79E0
005AC2F7        mov         dword ptr [ebp-14],eax
005AC2FA        push        0
005AC2FC        lea         eax,[ebp-138]
005AC302        push        eax
005AC303        mov         word ptr [ebp-13C],20
005AC30C        lea         ecx,[ebp-13C]
005AC312        lea         edx,[ebp-1C]
005AC315        mov         eax,dword ptr [ebp-4]
005AC318        mov         eax,dword ptr [eax+0CB0];TFrmDlg.?fCB0:string
005AC31E        call        00535034
005AC323        xor         edx,edx
005AC325        mov         eax,dword ptr [ebp-1C]
005AC328        call        00534DB0
005AC32D        mov         dword ptr [ebp-18],eax
005AC330        mov         eax,[0067A240];^gvar_00755F60:dword
005AC335        movzx       eax,word ptr [eax+7C]
005AC339        cmp         eax,dword ptr [ebp-18]
005AC33C>       jge         005AC34A
005AC33E        mov         eax,[0067A240];^gvar_00755F60:dword
005AC343        movzx       eax,word ptr [eax+7C]
005AC347        mov         dword ptr [ebp-18],eax
005AC34A        cmp         dword ptr [ebp-14],2
005AC34E>       je          005AC356
005AC350        cmp         dword ptr [ebp-18],0
005AC354>       jg          005AC360
005AC356        xor         eax,eax
005AC358        mov         dword ptr [ebp-18],eax
005AC35B>       jmp         005AC3FE
005AC360        lea         eax,[ebp-140]
005AC366        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC36C        add         edx,4
005AC36F        call        @UStrFromString
005AC374        mov         eax,dword ptr [ebp-140]
005AC37A        push        eax
005AC37B        mov         ax,word ptr [ebp-18]
005AC37F        push        eax
005AC380        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC386        mov         ecx,dword ptr [ecx+78]
005AC389        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005AC38F        mov         edx,dword ptr [edx]
005AC391        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005AC396        mov         eax,dword ptr [eax]
005AC398        call        00645B6C
005AC39D>       jmp         005AC3DC
005AC39F        lea         eax,[ebp-144]
005AC3A5        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC3AB        add         edx,4
005AC3AE        call        @UStrFromString
005AC3B3        mov         eax,dword ptr [ebp-144]
005AC3B9        push        eax
005AC3BA        mov         ax,word ptr [ebp-18]
005AC3BE        push        eax
005AC3BF        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
005AC3C5        mov         ecx,dword ptr [ecx+78]
005AC3C8        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005AC3CE        mov         edx,dword ptr [edx]
005AC3D0        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005AC3D5        mov         eax,dword ptr [eax]
005AC3D7        call        00645B6C
005AC3DC        mov         eax,[0067A240];^gvar_00755F60:dword
005AC3E1        mov         byte ptr [eax+4],0
005AC3E5        mov         eax,[0067A238];^gvar_00755F5C
005AC3EA        mov         byte ptr [eax],0
005AC3ED        mov         dword ptr ds:[677198],0FFFFFFFF;gvar_00677198
005AC3F7>       jmp         005AC3FE
005AC3F9        call        005D7034
005AC3FE        xor         eax,eax
005AC400        pop         edx
005AC401        pop         ecx
005AC402        pop         ecx
005AC403        mov         dword ptr fs:[eax],edx
005AC406        push        5AC44B
005AC40B        lea         eax,[ebp-144]
005AC411        mov         edx,2
005AC416        call        @UStrArrayClr
005AC41B        lea         eax,[ebp-138]
005AC421        mov         edx,7
005AC426        call        @UStrArrayClr
005AC42B        lea         eax,[ebp-0B0]
005AC431        mov         edx,2
005AC436        call        @UStrArrayClr
005AC43B        lea         eax,[ebp-1C]
005AC43E        call        @UStrClr
005AC443        ret
005AC444>       jmp         @HandleFinally
005AC449>       jmp         005AC40B
005AC44B        pop         edi
005AC44C        pop         esi
005AC44D        pop         ebx
005AC44E        mov         esp,ebp
005AC450        pop         ebp
005AC451        ret         10
