FState.TFrmDlg.DMenuBuyClick
005B112C        push        ebp
005B112D        mov         ebp,esp
005B112F        push        ecx
005B1130        mov         ecx,8
005B1135        push        0
005B1137        push        0
005B1139        dec         ecx
005B113A>       jne         005B1135
005B113C        push        ecx
005B113D        xchg        ecx,dword ptr [ebp-4]
005B1140        push        ebx
005B1141        push        esi
005B1142        push        edi
005B1143        mov         dword ptr [ebp-1C],ecx
005B1146        mov         dword ptr [ebp-18],edx
005B1149        mov         dword ptr [ebp-4],eax
005B114C        xor         eax,eax
005B114E        push        ebp
005B114F        push        5B1514
005B1154        push        dword ptr fs:[eax]
005B1157        mov         dword ptr fs:[eax],esp
005B115A        xor         eax,eax
005B115C        mov         dword ptr [ebp-10],eax
005B115F        call        kernel32.GetTickCount
005B1164        mov         edx,dword ptr [ebp-4]
005B1167        cmp         eax,dword ptr [edx+0C50];TFrmDlg.?fC50:dword
005B116D>       jb          005B14E4
005B1173        mov         eax,dword ptr [ebp-4]
005B1176        cmp         dword ptr [eax+0C6C],0;TFrmDlg.?fC6C:Integer
005B117D>       jl          005B14E4
005B1183        mov         eax,dword ptr [ebp-4]
005B1186        mov         eax,dword ptr [eax+0C6C];TFrmDlg.?fC6C:Integer
005B118C        mov         edx,dword ptr [ebp-4]
005B118F        mov         edx,dword ptr [edx+0C5C];TFrmDlg.?fC5C:TList
005B1195        cmp         eax,dword ptr [edx+8];TList.FCount:Integer
005B1198>       jge         005B14E4
005B119E        mov         eax,dword ptr [ebp-4]
005B11A1        mov         edx,dword ptr [eax+0C6C];TFrmDlg.?fC6C:Integer
005B11A7        mov         eax,dword ptr [ebp-4]
005B11AA        mov         eax,dword ptr [eax+0C5C];TFrmDlg.?fC5C:TList
005B11B0        call        TList.Get
005B11B5        mov         dword ptr [ebp-8],eax
005B11B8        call        kernel32.GetTickCount
005B11BD        add         eax,1388
005B11C2        mov         edx,dword ptr [ebp-4]
005B11C5        mov         dword ptr [edx+0C50],eax;TFrmDlg.?fC50:dword
005B11CB        mov         eax,dword ptr [ebp-8]
005B11CE        cmp         byte ptr [eax+0F],0
005B11D2>       jbe         005B1222
005B11D4        mov         eax,dword ptr [ebp-8]
005B11D7        cmp         byte ptr [eax+0F],2
005B11DB>       je          005B1222
005B11DD        lea         eax,[ebp-20]
005B11E0        mov         edx,dword ptr [ebp-8]
005B11E3        call        @UStrFromString
005B11E8        mov         eax,dword ptr [ebp-20]
005B11EB        push        eax
005B11EC        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005B11F2        mov         edx,dword ptr [edx]
005B11F4        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005B11F9        mov         eax,dword ptr [eax]
005B11FB        xor         ecx,ecx
005B11FD        call        006454AC
005B1202        mov         eax,dword ptr [ebp-4]
005B1205        xor         edx,edx
005B1207        mov         dword ptr [eax+0C74],edx;TFrmDlg.?fC74:dword
005B120D        mov         eax,dword ptr [ebp-4]
005B1210        add         eax,0C70;TFrmDlg.?fC70:string
005B1215        mov         edx,dword ptr [ebp-8]
005B1218        call        @UStrFromString
005B121D>       jmp         005B14E4
005B1222        mov         eax,dword ptr [ebp-4]
005B1225        cmp         byte ptr [eax+0C7A],0;TFrmDlg.?fC7A:byte
005B122C>       je          005B13C5
005B1232        xor         eax,eax
005B1234        push        ebp
005B1235        push        5B1270
005B123A        push        dword ptr fs:[eax]
005B123D        mov         dword ptr fs:[eax],esp
005B1240        mov         eax,dword ptr [ebp-4]
005B1243        mov         edx,dword ptr [eax+0C6C];TFrmDlg.?fC6C:Integer
005B1249        mov         eax,[0067A644];^gvar_00755AE8:TList
005B124E        mov         eax,dword ptr [eax]
005B1250        call        TList.Get
005B1255        mov         edx,dword ptr ds:[679FCC];^gvar_00755B04:PShortString
005B125B        mov         esi,eax
005B125D        mov         edi,edx
005B125F        mov         ecx,20
005B1264        rep movs    dword ptr [edi],dword ptr [esi]
005B1266        xor         eax,eax
005B1268        pop         edx
005B1269        pop         ecx
005B126A        pop         ecx
005B126B        mov         dword ptr fs:[eax],edx
005B126E>       jmp         005B127A
005B1270>       jmp         @HandleAnyException
005B1275        call        @DoneExcept
005B127A        mov         eax,[00679FCC];^gvar_00755B04:PShortString
005B127F        cmp         byte ptr [eax+4E],0
005B1283>       jbe         005B1392
005B1289        mov         eax,[00679FCC];^gvar_00755B04:PShortString
005B128E        movzx       eax,word ptr [eax+78]
005B1292        mov         edx,dword ptr [ebp-4]
005B1295        mov         dword ptr [edx+0C90],eax;TFrmDlg.?fC90:dword
005B129B        mov         eax,dword ptr [ebp-4]
005B129E        cmp         dword ptr [eax+0C90],1;TFrmDlg.?fC90:dword
005B12A5>       jne         005B12C2
005B12A7        mov         eax,dword ptr [ebp-4]
005B12AA        add         eax,0CB0;TFrmDlg.?fCB0:string
005B12AF        mov         edx,5B1530;'1'
005B12B4        call        @UStrAsg
005B12B9        mov         dword ptr [ebp-0C],1
005B12C0>       jmp         005B1302
005B12C2        push        5B1540;'当前数量 '
005B12C7        lea         edx,[ebp-28]
005B12CA        mov         eax,[00679FCC];^gvar_00755B04:PShortString
005B12CF        movzx       eax,word ptr [eax+78]
005B12D3        call        IntToStr
005B12D8        push        dword ptr [ebp-28]
005B12DB        push        5B1558;' 个.\请输入你想购买的商品数量?'
005B12E0        lea         eax,[ebp-24]
005B12E3        mov         edx,3
005B12E8        call        @UStrCatN
005B12ED        mov         edx,dword ptr [ebp-24]
005B12F0        mov         cx,word ptr ds:[5B157C];0x10 gvar_005B157C
005B12F7        mov         eax,dword ptr [ebp-4]
005B12FA        call        005C79E0
005B12FF        mov         dword ptr [ebp-0C],eax
005B1302        push        0
005B1304        lea         eax,[ebp-2C]
005B1307        push        eax
005B1308        mov         word ptr [ebp-30],20
005B130E        lea         ecx,[ebp-30]
005B1311        lea         edx,[ebp-14]
005B1314        mov         eax,dword ptr [ebp-4]
005B1317        mov         eax,dword ptr [eax+0CB0];TFrmDlg.?fCB0:string
005B131D        call        00535034
005B1322        xor         edx,edx
005B1324        mov         eax,dword ptr [ebp-14]
005B1327        call        00534DB0
005B132C        mov         dword ptr [ebp-10],eax
005B132F        mov         eax,[00679FCC];^gvar_00755B04:PShortString
005B1334        movzx       eax,word ptr [eax+78]
005B1338        cmp         eax,dword ptr [ebp-10]
005B133B>       jge         005B1349
005B133D        mov         eax,[00679FCC];^gvar_00755B04:PShortString
005B1342        movzx       eax,word ptr [eax+78]
005B1346        mov         dword ptr [ebp-10],eax
005B1349        cmp         dword ptr [ebp-0C],2
005B134D>       je          005B1355
005B134F        cmp         dword ptr [ebp-10],0
005B1353>       jg          005B135F
005B1355        xor         eax,eax
005B1357        mov         dword ptr [ebp-10],eax
005B135A>       jmp         005B14E4
005B135F        lea         eax,[ebp-34]
005B1362        mov         edx,dword ptr [ebp-8]
005B1365        call        @UStrFromString
005B136A        mov         eax,dword ptr [ebp-34]
005B136D        push        eax
005B136E        mov         ax,word ptr [ebp-10]
005B1372        push        eax
005B1373        mov         eax,dword ptr [ebp-8]
005B1376        mov         ecx,dword ptr [eax+10]
005B1379        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005B137F        mov         edx,dword ptr [edx]
005B1381        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005B1386        mov         eax,dword ptr [eax]
005B1388        call        00645B6C
005B138D>       jmp         005B14E4
005B1392        lea         eax,[ebp-38]
005B1395        mov         edx,dword ptr [ebp-8]
005B1398        call        @UStrFromString
005B139D        mov         eax,dword ptr [ebp-38]
005B13A0        push        eax
005B13A1        mov         ax,word ptr [ebp-10]
005B13A5        push        eax
005B13A6        mov         eax,dword ptr [ebp-8]
005B13A9        mov         ecx,dword ptr [eax+10]
005B13AC        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005B13B2        mov         edx,dword ptr [edx]
005B13B4        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005B13B9        mov         eax,dword ptr [eax]
005B13BB        call        00645B6C
005B13C0>       jmp         005B14E4
005B13C5        mov         eax,dword ptr [ebp-4]
005B13C8        cmp         byte ptr [eax+0CA3],0;TFrmDlg.?fCA3:byte
005B13CF>       je          005B1414
005B13D1        mov         eax,dword ptr [ebp-4]
005B13D4        add         eax,0C94;TFrmDlg.?fC94:byte
005B13D9        mov         edx,dword ptr [ebp-8]
005B13DC        mov         cl,0E
005B13DE        call        @PStrNCpy
005B13E3        lea         eax,[ebp-3C]
005B13E6        mov         edx,dword ptr [ebp-8]
005B13E9        call        @UStrFromString
005B13EE        mov         ecx,dword ptr [ebp-3C]
005B13F1        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005B13F7        mov         edx,dword ptr [edx]
005B13F9        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005B13FE        mov         eax,dword ptr [eax]
005B1400        call        00645D1C
005B1405        xor         edx,edx
005B1407        mov         eax,dword ptr [ebp-4]
005B140A        call        005C8534
005B140F>       jmp         005B14E4
005B1414        mov         eax,dword ptr [ebp-4]
005B1417        cmp         byte ptr [eax+0C7C],0;TFrmDlg.?fC7C:byte
005B141E>       je          005B1447
005B1420        lea         eax,[ebp-40]
005B1423        mov         edx,dword ptr [ebp-8]
005B1426        call        @UStrFromString
005B142B        mov         ecx,dword ptr [ebp-40]
005B142E        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005B1434        mov         edx,dword ptr [edx]
005B1436        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005B143B        mov         eax,dword ptr [eax]
005B143D        call        00645C4C
005B1442>       jmp         005B14E4
005B1447        mov         eax,dword ptr [ebp-8]
005B144A        cmp         byte ptr [eax+0F],2
005B144E>       jne         005B14B6
005B1450        mov         eax,dword ptr [ebp-4]
005B1453        mov         dword ptr [eax+0C90],64;TFrmDlg.?fC90:dword
005B145D        mov         cx,word ptr ds:[5B1580];0x1C gvar_005B1580
005B1464        mov         edx,5B1590;'你想买多少?'
005B1469        mov         eax,dword ptr [ebp-4]
005B146C        call        005C79E0
005B1471        mov         dword ptr [ebp-0C],eax
005B1474        push        0
005B1476        lea         eax,[ebp-44]
005B1479        push        eax
005B147A        mov         word ptr [ebp-30],20
005B1480        lea         ecx,[ebp-30]
005B1483        lea         edx,[ebp-14]
005B1486        mov         eax,dword ptr [ebp-4]
005B1489        mov         eax,dword ptr [eax+0CB0];TFrmDlg.?fCB0:string
005B148F        call        00535034
005B1494        xor         edx,edx
005B1496        mov         eax,dword ptr [ebp-14]
005B1499        call        00534DB0
005B149E        mov         dword ptr [ebp-10],eax
005B14A1        cmp         dword ptr [ebp-0C],2
005B14A5>       je          005B14E4
005B14A7        cmp         dword ptr [ebp-10],0
005B14AB>       jle         005B14E4
005B14AD        cmp         dword ptr [ebp-10],3E8
005B14B4>       jg          005B14E4
005B14B6        lea         eax,[ebp-48]
005B14B9        mov         edx,dword ptr [ebp-8]
005B14BC        call        @UStrFromString
005B14C1        mov         eax,dword ptr [ebp-48]
005B14C4        push        eax
005B14C5        mov         ax,word ptr [ebp-10]
005B14C9        push        eax
005B14CA        mov         eax,dword ptr [ebp-8]
005B14CD        mov         ecx,dword ptr [eax+14]
005B14D0        mov         edx,dword ptr ds:[6799E8];^gvar_007563A4
005B14D6        mov         edx,dword ptr [edx]
005B14D8        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
005B14DD        mov         eax,dword ptr [eax]
005B14DF        call        006459F4
005B14E4        xor         eax,eax
005B14E6        pop         edx
005B14E7        pop         ecx
005B14E8        pop         ecx
005B14E9        mov         dword ptr fs:[eax],edx
005B14EC        push        5B151B
005B14F1        lea         eax,[ebp-48]
005B14F4        mov         edx,6
005B14F9        call        @UStrArrayClr
005B14FE        lea         eax,[ebp-2C]
005B1501        mov         edx,4
005B1506        call        @UStrArrayClr
005B150B        lea         eax,[ebp-14]
005B150E        call        @UStrClr
005B1513        ret
005B1514>       jmp         @HandleFinally
005B1519>       jmp         005B14F1
005B151B        pop         edi
005B151C        pop         esi
005B151D        pop         ebx
005B151E        mov         esp,ebp
005B1520        pop         ebp
005B1521        ret         4
