fj.exe+1D8EC2 - 8B 45 F4              - mov eax,[ebp-0C]
fj.exe+1D8EC5 - 50                    - push eax
fj.exe+1D8EC6 - 57                    - push edi
fj.exe+1D8EC7 - 6A 00                 - push 00 { 0 }
fj.exe+1D8EC9 - 6A 00                 - push 00 { 0 }
fj.exe+1D8ECB - 6A 00                 - push 00 { 0 }
fj.exe+1D8ECD - 6A 00                 - push 00 { 0 }
fj.exe+1D8ECF - A1 FC977200           - mov eax,[fj.exe+3297FC] { (008B3BF4) }
fj.exe+1D8ED4 - 8B 00                 - mov eax,[eax]
fj.exe+1D8ED6 - 8B 4D F8              - mov ecx,[ebp-08]
fj.exe+1D8ED9 - 66 BA C30B            - mov dx,0BC3 { 3011 }
fj.exe+1D8EDD - E8 0AC20C00           - call fj.exe+2A50EC

; 准备参数 - 从右到左压栈
fj.exe+1D8EC2 - 8B 45 F4              - mov eax,[ebp-0C]    ; 加载变量到eax (可能是dy或目标Y坐标)
fj.exe+1D8EC5 - 50                    - push eax            ; 压入第8个参数 nSound
fj.exe+1D8EC6 - 57                    - push edi            ; 压入第7个参数 sStr (空字符串)
fj.exe+1D8EC7 - 6A 00                 - push 00 { 0 }       ; 压入第6个参数 nState (0)
fj.exe+1D8EC9 - 6A 00                 - push 00 { 0 }       ; 压入第5个参数 nFeature (空字符串)
fj.exe+1D8ECB - 6A 00                 - push 00 { 0 }       ; 压入第4个参数 ndir (方向)
fj.exe+1D8ECD - 6A 00                 - push 00 { 0 }       ; 压入第3个参数 nY (目标Y坐标)

; 获取对象实例和准备前两个参数
fj.exe+1D8ECF - A1 FC977200           - mov eax,[fj.exe+3297FC] ; 加载g_MySelf指针
fj.exe+1D8ED4 - 8B 00                 - mov eax,[eax]           ; 获取g_MySelf对象
fj.exe+1D8ED6 - 8B 4D F8              - mov ecx,[ebp-08]        ; 加载第2个参数nX到ecx
fj.exe+1D8ED9 - 66 BA C30B            - mov dx,0BC3             ; 加载第1个参数wIdent(CM_WALK=3011)到dx

; 调用方法
fj.exe+1D8EDD - E8 0AC20C00           - call fj.exe+2A50EC      ; 调用TActor.UpdateMsg方法