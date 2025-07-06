进入前
fj.exe+286CED - 88 86 E7030000        - mov [esi+000003E7],al
fj.exe+286CF3 - A1 14A57200           - mov eax,[fj.exe+32A514] { (008FA634) }
fj.exe+286CF8 - 8B 15 6CA87200        - mov edx,[fj.exe+32A86C] { (008FA918) }
fj.exe+286CFE - 57                    - push edi
fj.exe+286CFF - 8B F0                 - mov esi,eax
fj.exe+286D01 - 8B FA                 - mov edi,edx
fj.exe+286D03 - B9 B8000000           - mov ecx,000000B8 { 184 }
fj.exe+286D08 - F3 A5                 - repe movsd 
fj.exe+286D0A - 66 A5                 - movsw 
fj.exe+286D0C - 5F                    - pop edi
fj.exe+286D0D - A1 6CA87200           - mov eax,[fj.exe+32A86C] { (008FA918) }
fj.exe+286D12 - 89 38                 - mov [eax],edi
fj.exe+286D14 - 8D 85 08FCFFFF        - lea eax,[ebp-000003F8]
fj.exe+286D1A - 8B 15 14A57200        - mov edx,[fj.exe+32A514] { (008FA634) }
fj.exe+286D20 - 83 C2 08              - add edx,08 { 8 }
fj.exe+286D23 - E8 D8E4D7FF           - call fj.exe+5200
fj.exe+286D28 - 8B 85 08FCFFFF        - mov eax,[ebp-000003F8]
fj.exe+286D2E - 50                    - push eax
fj.exe+286D2F - 8B D7                 - mov edx,edi
fj.exe+286D31 - 8B 0D 14A57200        - mov ecx,[fj.exe+32A514] { (008FA634) }
fj.exe+286D37 - 8B 89 7F010000        - mov ecx,[ecx+0000017F]
fj.exe+286D3D - A1 5C9F7200           - mov eax,[fj.exe+329F5C] { (00918C1C) }
fj.exe+286D42 - 8B 00                 - mov eax,[eax]
fj.exe+286D44 - E8 AB32F6FF           - call fj.exe+1E9FF4

; 调用前的参数准备
fj.exe+286D28 - 8B 85 08FCFFFF        - mov eax,[ebp-000003F8]  ; 第一个参数(堆栈参数)
fj.exe+286D2E - 50                    - push eax                ; 压入堆栈作为第一个参数
fj.exe+286D2F - 8B D7                 - mov edx,edi             ; 第二个参数放入edx寄存器
fj.exe+286D31 - 8B 0D 14A57200        - mov ecx,[fj.exe+32A514] ; 准备第三个参数
fj.exe+286D37 - 8B 89 7F010000        - mov ecx,[ecx+0000017F]  ; 第三个参数放入ecx寄存器
fj.exe+286D3D - A1 5C9F7200           - mov eax,[fj.exe+329F5C] ; 准备第四个参数
fj.exe+286D42 - 8B 00                 - mov eax,[eax]           ; 第四个参数放入eax寄存器
fj.exe+286D44 - E8 AB32F6FF           - call fj.exe+1E9FF4      ; 调用函数

; 函数入口处的参数接收
fj.exe+1E9FF4 - 55                    - push ebp                ; 保存调用者的栈基址
fj.exe+1E9FF5 - 8B EC                 - mov ebp,esp             ; 设置新的栈基址
fj.exe+1E9FF7 - 83 C4 EC              - add esp,-14             ; 分配局部变量空间
fj.exe+1E9FFA - 53                    - push ebx                ; 保存寄存器
fj.exe+1E9FFB - 56                    - push esi                ; 保存寄存器
fj.exe+1E9FFC - 57                    - push edi                ; 保存寄存器
fj.exe+1E9FFD - 33 DB                 - xor ebx,ebx             ; 初始化ebx为0
fj.exe+1E9FFF - 89 5D F0              - mov [ebp-10],ebx        ; 初始化局部变量
fj.exe+1EA002 - 89 5D EC              - mov [ebp-14],ebx        ; 初始化局部变量
fj.exe+1EA005 - 8B F9                 - mov edi,ecx             ; 保存第三个参数到edi
fj.exe+1EA007 - 8B DA                 - mov ebx,edx             ; 保存第二个参数到ebx
fj.exe+1EA009 - 8B F0                 - mov esi,eax             ; 保存第四个参数到esi
fj.exe+1EA00B - 8B 45 08              - mov eax,[ebp+08]        ; 获取第一个参数(堆栈参数)

; 函数返回处理
fj.exe+1EA08F - 5F                    - pop edi                 ; 恢复寄存器
fj.exe+1EA090 - 5E                    - pop esi                 ; 恢复寄存器
fj.exe+1EA091 - 5B                    - pop ebx                 ; 恢复寄存器
fj.exe+1EA092 - 8B E5                 - mov esp,ebp             ; 恢复栈指针
fj.exe+1EA094 - 5D                    - pop ebp                 ; 恢复调用者的栈基址
fj.exe+1EA095 - C2 0400               - ret 0004                ; 返回并清理4字节堆栈参数
fj.exe+1EA00E - E8 39B4E1FF           - call fj.exe+544C
fj.exe+1EA013 - 33 C0                 - xor eax,eax
fj.exe+1EA015 - 55                    - push ebp
fj.exe+1EA016 - 68 88A05E00           - push fj.exe+1EA088 { (-509076503) }
fj.exe+1EA01B - 64 FF 30              - push fs:[eax]
fj.exe+1EA01E - 64 89 20              - mov fs:[eax],esp
fj.exe+1EA021 - 6A 00                 - push 00 { 0 }
fj.exe+1EA023 - 6A 00                 - push 00 { 0 }
fj.exe+1EA025 - 8D 45 F4              - lea eax,[ebp-0C]
fj.exe+1EA028 - 50                    - push eax
fj.exe+1EA029 - 33 C9                 - xor ecx,ecx
fj.exe+1EA02B - 8A CB                 - mov cl,bl
fj.exe+1EA02D - 8B D7                 - mov edx,edi
fj.exe+1EA02F - 66 B8 EB03            - mov ax,03EB { 1003 }
fj.exe+1EA033 - E8 743EECFF           - call fj.exe+ADEAC
fj.exe+1EA038 - 8D 55 F0              - lea edx,[ebp-10]
fj.exe+1EA03B - 8D 45 F4              - lea eax,[ebp-0C]
fj.exe+1EA03E - E8 913EECFF           - call fj.exe+ADED4
fj.exe+1EA043 - 8D 45 F0              - lea eax,[ebp-10]
fj.exe+1EA046 - 50                    - push eax
fj.exe+1EA047 - 8D 55 EC              - lea edx,[ebp-14]
fj.exe+1EA04A - 8B 45 08              - mov eax,[ebp+08]
fj.exe+1EA04D - E8 FE3EECFF           - call fj.exe+ADF50
fj.exe+1EA052 - 8B 55 EC              - mov edx,[ebp-14]
fj.exe+1EA055 - 58                    - pop eax
fj.exe+1EA056 - E8 09B2E1FF           - call fj.exe+5264
fj.exe+1EA05B - 8B 55 F0              - mov edx,[ebp-10]
fj.exe+1EA05E - 8B C6                 - mov eax,esi
fj.exe+1EA060 - E8 7FD3FFFF           - call fj.exe+1E73E4
fj.exe+1EA065 - 33 C0                 - xor eax,eax
fj.exe+1EA067 - 5A                    - pop edx
fj.exe+1EA068 - 59                    - pop ecx
fj.exe+1EA069 - 59                    - pop ecx
fj.exe+1EA06A - 64 89 10              - mov fs:[eax],edx
fj.exe+1EA06D - 68 8FA05E00           - push fj.exe+1EA08F { (95) }
fj.exe+1EA072 - 8D 45 EC              - lea eax,[ebp-14]
fj.exe+1EA075 - BA 02000000           - mov edx,00000002 { 2 }
fj.exe+1EA07A - E8 31AFE1FF           - call fj.exe+4FB0
fj.exe+1EA07F - 8D 45 08              - lea eax,[ebp+08]
fj.exe+1EA082 - E8 05AFE1FF           - call fj.exe+4F8C
fj.exe+1EA087 - C3                    - ret 
fj.exe+1EA088 - E9 1BA8E1FF           - jmp fj.exe+48A8
fj.exe+1EA08D - EB E3                 - jmp fj.exe+1EA072
fj.exe+1EA08F - 5F                    - pop edi
fj.exe+1EA090 - 5E                    - pop esi
fj.exe+1EA091 - 5B                    - pop ebx
fj.exe+1EA092 - 8B E5                 - mov esp,ebp
fj.exe+1EA094 - 5D                    - pop ebp
fj.exe+1EA095 - C2 0400               - ret 0004 { 4 }
