_Unit72.sub_0056A810
0056A810        push        ebp
0056A811        mov         ebp,esp
0056A813        push        0
0056A815        push        0
0056A817        push        0
0056A819        push        0
0056A81B        push        0
0056A81D        push        0
0056A81F        push        0
0056A821        push        0
0056A823        push        ebx
0056A824        mov         dword ptr [ebp-8],ecx
0056A827        mov         dword ptr [ebp-0C],edx
0056A82A        mov         dword ptr [ebp-4],eax
0056A82D        xor         eax,eax
0056A82F        push        ebp
0056A830        push        56A991
0056A835        push        dword ptr fs:[eax]
0056A838        mov         dword ptr fs:[eax],esp
0056A83B        mov         eax,dword ptr [ebp-8]
0056A83E        cmp         word ptr [eax],7E
0056A842>       je          0056A84D
0056A844        mov         eax,dword ptr [ebp-8]
0056A847        cmp         word ptr [eax],27
0056A84B>       jne         0056A855
0056A84D        mov         eax,dword ptr [ebp-8]
0056A850        mov         word ptr [eax],5F
0056A855        mov         eax,dword ptr [ebp-8]
0056A858        mov         ax,word ptr [eax]
0056A85B        cmp         al,0D
0056A85D>       jne         0056A952
0056A863        mov         eax,dword ptr [ebp-8]
0056A866        mov         word ptr [eax],0
0056A86B        lea         edx,[ebp-10]
0056A86E        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
0056A873        mov         eax,dword ptr [eax]
0056A875        mov         eax,dword ptr [eax+8D4]
0056A87B        mov         eax,dword ptr [eax+27C]
0056A881        call        LowerCase
0056A886        mov         edx,dword ptr [ebp-10]
0056A889        mov         eax,dword ptr [ebp-4]
0056A88C        call        005694A8
0056A891        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
0056A896        mov         eax,dword ptr [eax]
0056A898        mov         eax,dword ptr [eax+8D8]
0056A89E        mov         edx,dword ptr [eax+27C]
0056A8A4        mov         eax,dword ptr [ebp-4]


030D2EAC  61 00 64 00 61 00 64 00                          a.d.a.d.
直接赋值
然后


0056A8A7        call        005695A0
0056A8AC        lea         edx,[ebp-14]
0056A8AF        mov         eax,dword ptr [ebp-4]
0056A8B2        call        00569434
0056A8B7        cmp         dword ptr [ebp-14],0
0056A8BB>       je          0056A976
0056A8C1        lea         edx,[ebp-18]
0056A8C4        mov         eax,dword ptr [ebp-4]
0056A8C7        call        0056952C
0056A8CC        cmp         dword ptr [ebp-18],0
0056A8D0>       je          0056A976
0056A8D6        lea         edx,[ebp-1C]
0056A8D9        mov         eax,dword ptr [ebp-4]
0056A8DC        call        0056952C
0056A8E1        mov         eax,dword ptr [ebp-1C]
0056A8E4        push        eax
0056A8E5        lea         edx,[ebp-20]
0056A8E8        mov         eax,dword ptr [ebp-4]
0056A8EB        call        00569434
0056A8F0        mov         edx,dword ptr [ebp-20]
0056A8F3        mov         eax,[00679EBC];^gvar_007524B4:TFrmMain
0056A8F8        mov         eax,dword ptr [eax]
0056A8FA        pop         ecx
0056A8FB        call        00642690
0056A900        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
0056A905        mov         eax,dword ptr [eax]
0056A907        mov         eax,dword ptr [eax+8D4]
0056A90D        xor         edx,edx
0056A90F        call        TDControl.SetCaption
0056A914        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
0056A919        mov         eax,dword ptr [eax]
0056A91B        mov         eax,dword ptr [eax+8D8]
0056A921        xor         edx,edx
0056A923        call        TDControl.SetCaption
0056A928        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
0056A92D        mov         eax,dword ptr [eax]
0056A92F        mov         eax,dword ptr [eax+8D4]
0056A935        mov         byte ptr [eax+300],0
0056A93C        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
0056A941        mov         eax,dword ptr [eax]
0056A943        mov         eax,dword ptr [eax+8D8]
0056A949        mov         byte ptr [eax+300],0
0056A950>       jmp         0056A976
0056A952        mov         eax,dword ptr [ebp-8]
0056A955        mov         ax,word ptr [eax]
0056A958        cmp         al,9
0056A95A>       jne         0056A976
0056A95C        mov         eax,dword ptr [ebp-8]
0056A95F        mov         word ptr [eax],0
0056A964        mov         eax,[0067A728];^gvar_0074350C:TFrmDlg
0056A969        mov         eax,dword ptr [eax]
0056A96B        mov         eax,dword ptr [eax+8D4]
0056A971        call        0053F430
0056A976        xor         eax,eax
0056A978        pop         edx
0056A979        pop         ecx
0056A97A        pop         ecx
0056A97B        mov         dword ptr fs:[eax],edx
0056A97E        push        56A998
0056A983        lea         eax,[ebp-20]
0056A986        mov         edx,5
0056A98B        call        @UStrArrayClr
0056A990        ret
0056A991>       jmp         @HandleFinally
0056A996>       jmp         0056A983
0056A998        pop         ebx
0056A999        mov         esp,ebp
0056A99B        pop         ebp
0056A99C        ret
