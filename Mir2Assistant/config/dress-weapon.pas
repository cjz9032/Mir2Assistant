procedure TFrmDlg.DHSWWeaponClick(Sender: TObject; X, Y: Integer);
var
  where, n, sel: Integer;
  flag: Boolean;
begin
  if (g_Myself = nil) or (g_Myself.m_HeroObject = nil) then  Exit;

  if not DHStateWin.Visible then  Exit;

  if not (m_nHeroStatePage in [0, 1]) then  Exit;
  
  if not g_Myself.m_HeroObject.n_boState then
  begin
    if g_boItemMoving then
    begin
      flag := False;
      if IsDetectItem(g_MovingItem.Index) then
        Exit;
      if IsStallItem(g_MovingItem.Index) then
        Exit;
      if (g_MovingItem.Index = -97) or (g_MovingItem.Index = -98) then
        Exit;
      if (g_MovingItem.item.s.Name = '') or (g_WaitingUseItem.item.s.Name <> '') then
        Exit;
      where := GetTakeOnPosition(g_MovingItem.item.s, g_HeroUseItems);
      if g_MovingItem.Index >= HERO_MIIDX_OFFSET then
      begin
        case where of
          U_DRUM:
            begin
              if (Sender = DHSWDrum) and (not g_boUI0508) then
                flag := True;
            end;
          U_HORSE:
            begin
              if (Sender = DHSWHorse) and (not g_boUI0508) then
                flag := True;
            end;
          U_FASHION: if (Sender = DHSWDress) and (m_nHeroStatePage = 1) and (not g_boUI0508) then
            begin
              if g_Myself.m_HeroObject.m_btSex = 0 then
                if g_MovingItem.item.s.StdMode <> 12 then
                  Exit;
              if g_Myself.m_HeroObject.m_btSex = 1 then
                if g_MovingItem.item.s.StdMode <> 13 then
                  Exit;
              flag := True;
            end;

          U_DRESS: if Sender = DHSWDress then
            begin
              if g_Myself.m_HeroObject.m_btSex = 0 then
                if g_MovingItem.item.s.StdMode <> 10 then
                  Exit;
              if g_Myself.m_HeroObject.m_btSex = 1 then
                if g_MovingItem.item.s.StdMode <> 11 then
                  Exit;
              flag := True;
            end;
          U_WEAPON: if Sender = DHSWWeapon then
              flag := True;
          U_NECKLACE: if Sender = DHSWNecklace then
              flag := True;
          U_RIGHTHAND: if Sender = DHSWLight then
              flag := True;
          U_HELMET, U_HELMETEX: if Sender = DHSWHelmet then
              flag := True;
          U_RINGR, U_RINGL:
            begin
              if Sender = DHSWRingL then
              begin
                where := U_RINGL;
                flag := True;
              end;
              if Sender = DHSWRingR then
              begin
                where := U_RINGR;
                flag := True;
              end;
            end;
          U_ARMRINGR:
            begin
              if Sender = DHSWArmRingL then
              begin
                where := U_ARMRINGL;
                flag := True;
              end;
              if Sender = DHSWArmRingR then
              begin
                where := U_ARMRINGR;
                flag := True;
              end;
            end;
          U_ARMRINGL:
            begin
              if Sender = DHSWArmRingL then
              begin
                where := U_ARMRINGL;
                flag := True;
              end;
            end;
          U_BUJUK:
            begin
              if Sender = DHSWBujuk then
              begin
                where := U_BUJUK;
                flag := True;
              end;
              if g_MovingItem.item.s.StdMode <> 42 then
              begin
                if Sender = DHSWArmRingL then
                begin
                  where := U_ARMRINGL;
                  flag := True;
                end;
              end;
            end;
          U_BELT:
            begin
              if Sender = DHSWBelt then
              begin
                where := U_BELT;
                flag := True;
              end;
            end;
          U_BOOTS:
            begin
              if Sender = DHSWBoots then
              begin
                where := U_BOOTS;
                flag := True;
              end;
            end;
          U_CHARM:
            begin
              if Sender = DHSWCharm then
              begin
                where := U_CHARM;
                flag := True;
              end;
            end;
        end;
      end
      else
      begin
        n := -(g_MovingItem.Index + 1 + HERO_MIIDX_OFFSET);
        if n in [0..U_FASHION] then
        begin
          g_SndMgr.ItemClickSound(g_MovingItem.item.s);
          g_HeroUseItems[n] := g_MovingItem.item;
          g_MovingItem.item.s.Name := '';
          g_boItemMoving := False;
        end;
      end;
      if flag then
      begin
        if g_MovingItem.item.s.StdMode = 42 then
        begin
          //食人树叶
          //DScreen.AddSysMsg(IntToStr(g_MovingItem.Index - HERO_MIIDX_OFFSET));
          //frmMain.MoveItemIdx := idx;
          frmMain.HeroEatItem(-1);
        end
        else
        begin
          g_SndMgr.ItemClickSound(g_MovingItem.item.s);
          g_WaitingUseItem := g_MovingItem;
          g_WaitingUseItem.Index := where;
          frmMain.HeroSendTakeOnItem(where, g_MovingItem.item.MakeIndex, g_MovingItem.item.s.Name);
          g_MovingItem.item.s.Name := '';
          g_boItemMoving := False;
        end;
      end;
    end
    else
    begin
      if (g_MovingItem.item.s.Name <> '') or (g_WaitingUseItem.item.s.Name <> '') then  Exit;
      sel := -1;
      if Sender = DHSWDress then
      begin
        if g_boUI0508 then
          sel := U_DRESS
        else
        begin
          if m_nHeroStatePage = 0 then
            sel := U_DRESS
          else
            sel := U_FASHION;
        end;
      end;
      if Sender = DHSWWeapon then
        sel := U_WEAPON;
      if Sender = DHSWHelmet then
      begin
        if g_HeroUseItems[U_HELMETEX].s.Name <> '' then
          sel := U_HELMETEX
        else
          sel := U_HELMET;
      end;
      if Sender = DHSWNecklace then
        sel := U_NECKLACE;
      if Sender = DHSWLight then
        sel := U_RIGHTHAND;
      if Sender = DHSWRingL then
        sel := U_RINGL;
      if Sender = DHSWRingR then
        sel := U_RINGR;
      if Sender = DHSWArmRingL then
        sel := U_ARMRINGL;
      if Sender = DHSWArmRingR then
        sel := U_ARMRINGR;

      if Sender = DHSWBujuk then
        sel := U_BUJUK;
      if Sender = DHSWBelt then
        sel := U_BELT;
      if Sender = DHSWBoots then
        sel := U_BOOTS;
      if Sender = DHSWCharm then
        sel := U_CHARM;
      if not g_boUI0508  then
      begin
        if Sender = DHSWDrum then
          sel := U_DRUM;
        if Sender = DHSWHorse then
          sel := U_HORSE;
      end;

      if sel >= 0 then
        if g_HeroUseItems[sel].s.Name <> '' then
        begin
          g_SndMgr.ItemClickSound(g_HeroUseItems[sel].s);
          g_MovingItem.Index := -(sel + 1 + HERO_MIIDX_OFFSET);
          g_MovingItem.item := g_HeroUseItems[sel];
          g_HeroUseItems[sel].s.Name := '';
          g_boItemMoving := True;
        end;
    end;
  end;
end;