FState.sub_00599734
00599734        push        ebp
00599735        mov         ebp,esp
00599737        add         esp,0FFFFFFF0
0059973A        push        esi
0059973B        push        edi
0059973C        mov         dword ptr [ebp-4],eax
0059973F        mov         eax,[0067A238];^gvar_00755F5C
00599744        cmp         byte ptr [eax],0
00599747>       je          005998BE
0059974D        mov         eax,[0067A238];^gvar_00755F5C
00599752        mov         byte ptr [eax],0
00599755        mov         eax,[0067A240];^gvar_00755F60:dword
0059975A        mov         eax,dword ptr [eax]
0059975C        mov         dword ptr [ebp-8],eax
0059975F        mov         eax,dword ptr [ebp-4]
00599762        cmp         byte ptr [eax+0C78],0
00599769>       je          00599778
0059976B        cmp         dword ptr ds:[677198],1;gvar_00677198
00599772>       je          00599879
00599778        mov         eax,dword ptr [ebp-8]
0059977B        call        0058FE50
00599780        test        al,al
00599782>       je          005997B5
00599784        mov         eax,[0067A240];^gvar_00755F60:dword
00599789        cmp         dword ptr [eax+30],2C
0059978D>       je          0059979A
0059978F        mov         eax,[0067A240];^gvar_00755F60:dword
00599794        cmp         dword ptr [eax+30],2D
00599798>       jne         005997B5
0059979A        mov         eax,[0067A240];^gvar_00755F60:dword
0059979F        add         eax,4
005997A2        call        0058FEC8
005997A7        mov         eax,[0067A240];^gvar_00755F60:dword
005997AC        mov         byte ptr [eax+4],0
005997B0>       jmp         005998C3
005997B5        cmp         dword ptr [ebp-8],0
005997B9>       jge         00599823
005997BB        cmp         dword ptr [ebp-8],0FFFFFF9D
005997BF>       jne         005997D6
005997C1        mov         eax,[0067A240];^gvar_00755F60:dword
005997C6        add         eax,4
005997C9        or          edx,0FFFFFFFF
005997CC        call        005D678C
005997D1>       jmp         005998C3
005997D6        cmp         dword ptr [ebp-8],0FFFFFFEC
005997DA>       jg          005997F1
005997DC        cmp         dword ptr [ebp-8],0FFFFFFE2
005997E0>       jle         005997F1
005997E2        mov         eax,[0067A240];^gvar_00755F60:dword
005997E7        add         eax,4
005997EA        call        005D75A8
005997EF>       jmp         0059986E
005997F1        mov         eax,dword ptr [ebp-8]
005997F4        inc         eax
005997F5        neg         eax
005997F7        mov         dword ptr [ebp-0C],eax
005997FA        mov         eax,dword ptr [ebp-0C]
005997FD        sub         eax,0F
00599800>       jae         0059986E
00599802        mov         eax,dword ptr [ebp-0C]
00599805        shl         eax,4
00599808        mov         edx,dword ptr ds:[679940];^gvar_00752768:Pointer
0059980E        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
00599814        lea         edi,[edx+eax*8]
00599817        lea         esi,[ecx+4]
0059981A        mov         ecx,20
0059981F        rep movs    dword ptr [edi],dword ptr [esi]
00599821>       jmp         0059986E
00599823        mov         eax,dword ptr [ebp-8]
00599826        sub         eax,2E
00599829>       jae         0059986E
0059982B        mov         eax,dword ptr [ebp-8]
0059982E        shl         eax,4
00599831        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
00599837        cmp         byte ptr [edx+eax*8],0
0059983B>       jne         0059985E
0059983D        mov         eax,dword ptr [ebp-8]
00599840        shl         eax,4
00599843        mov         edx,dword ptr ds:[67A548];^gvar_00752EE8:Pointer
00599849        mov         ecx,dword ptr ds:[67A240];^gvar_00755F60:dword
0059984F        lea         edi,[edx+eax*8]
00599852        lea         esi,[ecx+4]
00599855        mov         ecx,20
0059985A        rep movs    dword ptr [edi],dword ptr [esi]
0059985C>       jmp         0059986E
0059985E        mov         eax,[0067A240];^gvar_00755F60:dword
00599863        add         eax,4
00599866        or          edx,0FFFFFFFF
00599869        call        005D678C
0059986E        mov         eax,[0067A240];^gvar_00755F60:dword
00599873        mov         byte ptr [eax+4],0
00599877>       jmp         005998BE
00599879        mov         eax,[0067A644];^gvar_00755AE8:TList
0059987E        mov         eax,dword ptr [eax]
00599880        mov         eax,dword ptr [eax+8]
00599883        cmp         eax,dword ptr [ebp-8]
00599886>       jl          005998B3
00599888        mov         eax,[0067A644];^gvar_00755AE8:TList
0059988D        mov         eax,dword ptr [eax]
0059988F        mov         edx,dword ptr [ebp-8]
00599892        call        TList.Get
00599897        mov         dword ptr [ebp-10],eax
0059989A        cmp         dword ptr [ebp-10],0
0059989E>       je          005998B3
005998A0        mov         eax,dword ptr [ebp-10]
005998A3        mov         edx,dword ptr ds:[67A240];^gvar_00755F60:dword
005998A9        add         edx,4
005998AC        mov         cl,0E
005998AE        call        @PStrNCpy
005998B3        mov         eax,[0067A240];^gvar_00755F60:dword
005998B8        mov         byte ptr [eax+4],0
005998BC>       jmp         005998C3
005998BE        call        005D7034
005998C3        pop         edi
005998C4        pop         esi
005998C5        mov         esp,ebp
005998C7        pop         ebp
005998C8        ret
