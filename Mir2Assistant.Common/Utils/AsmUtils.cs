using Mir2Assistant.Common.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace Mir2Assistant.Utils;
public class AsmUtils
{
    private string Asmcode = "";
    public int? pid { get; set; }

    private string hex(int address)
    {
        string str = address.ToString("X");
        return str;
    }

    public string intTohex(int value, int num)
    {
        string str1;
        string str2 = "";
        str1 = "0000000" + this.hex(value);
        str1 = str1.Substring(str1.Length - num, num);
        for (int i = 0; i < str1.Length / 2; i++)
        {
            str2 = str2 + str1.Substring(str1.Length - 2 - 2 * i, 2);
        }
        return str2;
    }

    public AsmUtils SUB_ESP(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
        {
            this.Asmcode = this.Asmcode + "83EC" + intTohex(addre, 2);
        }
        else
        {
            this.Asmcode = this.Asmcode + "81EC" + intTohex(addre, 8);
        }
        return this;
    }

    public AsmUtils Nop()
    {
        this.Asmcode = this.Asmcode + "90";
        return this;
    }

    public AsmUtils RetA(int addre)
    {
        this.Asmcode = this.Asmcode + intTohex(addre, 4);
        return this;
    }

    public AsmUtils IN_AL_DX()
    {
        this.Asmcode = this.Asmcode + "EC";
        return this;
    }

    public AsmUtils TEST_EAX_EAX()
    {
        this.Asmcode = this.Asmcode + "85C0";
        return this;
    }

    public AsmUtils Leave()
    {
        this.Asmcode = this.Asmcode + "C9";
        return this;
    }

    public AsmUtils Pushad()
    {
        this.Asmcode = this.Asmcode + "60";
        return this;
    }

    public AsmUtils Pushfd()
    {
        this.Asmcode = this.Asmcode + "9C";
        return this;
    }

    public AsmUtils Popad()
    {
        this.Asmcode = this.Asmcode + "61";
        return this;
    }
    public AsmUtils Popfd()
    {
        this.Asmcode = this.Asmcode + "9D";
        return this;
    }

    public AsmUtils Ret()
    {
        this.Asmcode = this.Asmcode + "C3";
        return this;
    }

    #region ADD
    public AsmUtils Add_EAX_EDX()
    {
        this.Asmcode = this.Asmcode + "03C2";
        return this;
    }

    public AsmUtils Add_EBX_EAX()
    {
        this.Asmcode = this.Asmcode + "03D8";
        return this;
    }

    public AsmUtils Add_EAX_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "0305" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Add_EBX_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "031D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Add_EBP_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "032D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Add_EAX(int addre)
    {
        this.Asmcode = this.Asmcode + "05" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Add_EBX(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "83C3" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "81C3" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Add_ECX(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "83C1" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "81C1" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Add_EDX(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "83C2" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "81C2" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Add_ESI(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "83C6" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "81C6" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Add_ESP(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "83C4" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "81C4" + intTohex(addre, 8);
        return this;
    }
    #endregion

    #region mov
    public AsmUtils Mov_DWORD_Ptr_EAX_ADD(int addre, int addre1)
    {
        if ((addre <= 127) && (addre >= -128))
        {
            this.Asmcode = this.Asmcode + "C740" + intTohex(addre, 2) + intTohex(addre1, 8);
        }
        else
        {
            this.Asmcode = this.Asmcode + "C780" + intTohex(addre, 8) + intTohex(addre1, 8);
        }
        return this;
    }

    public AsmUtils Mov_DWORD_Ptr_ESP_ADD(int addre, int addre1)
    {
        if ((addre <= 127) && (addre >= -128))
        {
            this.Asmcode = this.Asmcode + "C74424" + intTohex(addre, 2) + intTohex(addre1, 8);
        }
        else
        {
            this.Asmcode = this.Asmcode + "C78424" + intTohex(addre, 8) + intTohex(addre1, 8);
        }
        return this;
    }

    public AsmUtils Mov_DWORD_Ptr_ESP_ADD_EAX(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
        {
            this.Asmcode = this.Asmcode + "894424" + intTohex(addre, 2);
        }
        else
        {
            this.Asmcode = this.Asmcode + "898424" + intTohex(addre, 8);
        }
        return this;
    }

    public AsmUtils Mov_DWORD_Ptr_ESP(int addre)
    {
        this.Asmcode = this.Asmcode + "C70424" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_DWORD_Ptr_EAX(int addre)
    {
        this.Asmcode = this.Asmcode + "A3" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "8B1D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "8B0D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "A1" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "8B15" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ESI_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "8B35" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ESP_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "8B25" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBP_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "8B2D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EAX(int addre)
    {
        this.Asmcode = this.Asmcode + "8B00";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EAX()
    {
        this.Asmcode = this.Asmcode + "8B00";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EBP()
    {
        this.Asmcode = this.Asmcode + "8B4500";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EBX()
    {
        this.Asmcode = this.Asmcode + "8B03";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_ECX()
    {
        this.Asmcode = this.Asmcode + "8B01";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EDX()
    {
        this.Asmcode = this.Asmcode + "8B02";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EDI()
    {
        this.Asmcode = this.Asmcode + "8B07";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_ESP()
    {
        this.Asmcode = this.Asmcode + "8B0424";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_ESI()
    {
        this.Asmcode = this.Asmcode + "8B06";
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EAX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
        {
            this.Asmcode = this.Asmcode + "8B40" + intTohex(addre, 2);
        }
        else
        {
            this.Asmcode = this.Asmcode + "8B80" + intTohex(addre, 8);
        }
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_ESP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B4424" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B8424" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EBX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B43" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B83" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_ECX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B41" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B81" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EDX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B42" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B82" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EDI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B47" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B87" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_EBP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B45" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B85" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EAX_DWORD_Ptr_ESI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B46" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B86" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EAX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B58" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B98" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_ESP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B5C24" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B9C24" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EBX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B5B" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B9B" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_ECX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B59" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B99" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EDX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B5A" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B9A" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EDI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B5F" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B9F" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EBP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B5D" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B9D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_ESI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B5E" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B9E" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EAX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B48" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B88" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_ESP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B4C24" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B8C24" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EBX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B4B" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B8B" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_ECX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B49" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B89" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EDX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B4A" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B8A" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EDI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B4F" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B8F" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EBP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B4D" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B8D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_ESI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B4E" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B8E" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EAX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B50" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B90" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_ESP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B5424" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B9424" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EBX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B53" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B93" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_ECX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B51" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B91" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EDX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B52" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B92" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EDI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B57" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B97" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EBP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B55" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B95" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_ESI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8B56" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8B96" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX_EAX()
    {
        this.Asmcode = this.Asmcode + "8BC8";
        return this;
    }

    public AsmUtils Mov_EAX(int addre)
    {
        this.Asmcode = this.Asmcode + "B8" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBX(int addre)
    {
        this.Asmcode = this.Asmcode + "BB" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ECX(int addre)
    {
        this.Asmcode = this.Asmcode + "B9" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDX(int addre)
    {
        this.Asmcode = this.Asmcode + "BA" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ESI(int addre)
    {
        this.Asmcode = this.Asmcode + "BE" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ESP(int addre)
    {
        this.Asmcode = this.Asmcode + "BC" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EBP(int addre)
    {
        this.Asmcode = this.Asmcode + "BD" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_EDI(int addre)
    {
        this.Asmcode = this.Asmcode + "BF" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Mov_ESI_DWORD_Ptr_EAX()
    {
        this.Asmcode = this.Asmcode + "8B7020";
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EAX()
    {
        this.Asmcode = this.Asmcode + "8B18";
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EBP()
    {
        this.Asmcode = this.Asmcode + "8B5D00";
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EBX()
    {
        this.Asmcode = this.Asmcode + "8B1B";
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_ECX()
    {
        this.Asmcode = this.Asmcode + "8B19";
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EDX()
    {
        this.Asmcode = this.Asmcode + "8B1A";
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_EDI()
    {
        this.Asmcode = this.Asmcode + "8B1F";
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_ESP()
    {
        this.Asmcode = this.Asmcode + "8B1C24";
        return this;
    }

    public AsmUtils Mov_EBX_DWORD_Ptr_ESI()
    {
        this.Asmcode = this.Asmcode + "8B1E";
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EAX()
    {
        this.Asmcode = this.Asmcode + "8B08";
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EBP()
    {
        this.Asmcode = this.Asmcode + "8B4D00";
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EBX()
    {
        this.Asmcode = this.Asmcode + "8B0B";
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_ECX()
    {
        this.Asmcode = this.Asmcode + "8B09";
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EDX()
    {
        this.Asmcode = this.Asmcode + "8B0A";
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_EDI()
    {
        this.Asmcode = this.Asmcode + "8B0F";
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_ESP()
    {
        this.Asmcode = this.Asmcode + "8B0C24";
        return this;
    }

    public AsmUtils Mov_ECX_DWORD_Ptr_ESI()
    {
        this.Asmcode = this.Asmcode + "8B0E";
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EAX()
    {
        this.Asmcode = this.Asmcode + "8B10";
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EBP()
    {
        this.Asmcode = this.Asmcode + "8B5500";
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EBX()
    {
        this.Asmcode = this.Asmcode + "8B13";
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_ECX()
    {
        this.Asmcode = this.Asmcode + "8B11";
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EDX()
    {
        this.Asmcode = this.Asmcode + "8B12";
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_EDI()
    {
        this.Asmcode = this.Asmcode + "8B17";
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_ESI()
    {
        this.Asmcode = this.Asmcode + "8B16";
        return this;
    }

    public AsmUtils Mov_EDX_DWORD_Ptr_ESP()
    {
        this.Asmcode = this.Asmcode + "8B1424";
        return this;
    }

    public AsmUtils Mov_EAX_EBP()
    {
        this.Asmcode = this.Asmcode + "8BC5";
        return this;
    }

    public AsmUtils Mov_EAX_EBX()
    {
        this.Asmcode = this.Asmcode + "8BC3";
        return this;
    }

    public AsmUtils Mov_EAX_ECX()
    {
        this.Asmcode = this.Asmcode + "8BC1";
        return this;
    }

    public AsmUtils Mov_EAX_EDI()
    {
        this.Asmcode = this.Asmcode + "8BC7";
        return this;
    }

    public AsmUtils Mov_EAX_EDX()
    {
        this.Asmcode = this.Asmcode + "8BC2";
        return this;
    }

    public AsmUtils Mov_EAX_ESI()
    {
        this.Asmcode = this.Asmcode + "8BC6";
        return this;
    }

    public AsmUtils Mov_EAX_ESP()
    {
        this.Asmcode = this.Asmcode + "8BC4";
        return this;
    }

    public AsmUtils Mov_EBX_EBP()
    {
        this.Asmcode = this.Asmcode + "8BDD";
        return this;
    }

    public AsmUtils Mov_EBX_EAX()
    {
        this.Asmcode = this.Asmcode + "8BD8";
        return this;
    }

    public AsmUtils Mov_EBX_ECX()
    {
        this.Asmcode = this.Asmcode + "8BD9";
        return this;
    }

    public AsmUtils Mov_EBX_EDI()
    {
        this.Asmcode = this.Asmcode + "8BDF";
        return this;
    }

    public AsmUtils Mov_EBX_EDX()
    {
        this.Asmcode = this.Asmcode + "8BDA";
        return this;
    }

    public AsmUtils Mov_EBX_ESI()
    {
        this.Asmcode = this.Asmcode + "8BDE";
        return this;
    }

    public AsmUtils Mov_EBX_ESP()
    {
        this.Asmcode = this.Asmcode + "8BDC";
        return this;
    }

    public AsmUtils Mov_ECX_EBP()
    {
        this.Asmcode = this.Asmcode + "8BCD";
        return this;
    }

    public AsmUtils Mov_ECX_EBX()
    {
        this.Asmcode = this.Asmcode + "8BCB";
        return this;
    }

    public AsmUtils Mov_ECX_EDI()
    {
        this.Asmcode = this.Asmcode + "8BCF";
        return this;
    }

    public AsmUtils Mov_ECX_EDX()
    {
        this.Asmcode = this.Asmcode + "8BCA";
        return this;
    }

    public AsmUtils Mov_ECX_ESI()
    {
        this.Asmcode = this.Asmcode + "8BCE";
        return this;
    }

    public AsmUtils Mov_ECX_ESP()
    {
        this.Asmcode = this.Asmcode + "8BCC";
        return this;
    }

    public AsmUtils Mov_EDX_EBP()
    {
        this.Asmcode = this.Asmcode + "8BD5";
        return this;
    }

    public AsmUtils Mov_EDX_EBX()
    {
        this.Asmcode = this.Asmcode + "8BD3";
        return this;
    }

    public AsmUtils Mov_EDX_ECX()
    {
        this.Asmcode = this.Asmcode + "8BD1";
        return this;
    }

    public AsmUtils Mov_EDX_EDI()
    {
        this.Asmcode = this.Asmcode + "8BD7";
        return this;
    }

    public AsmUtils Mov_EDX_EAX()
    {
        this.Asmcode = this.Asmcode + "8BD0";
        return this;
    }

    public AsmUtils Mov_EDX_ESI()
    {
        this.Asmcode = this.Asmcode + "8BD6";
        return this;
    }

    public AsmUtils Mov_EDX_ESP()
    {
        this.Asmcode = this.Asmcode + "8BD4";
        return this;
    }

    public AsmUtils Mov_ESI_EBP()
    {
        this.Asmcode = this.Asmcode + "8BF5";
        return this;
    }

    public AsmUtils Mov_ESI_EBX()
    {
        this.Asmcode = this.Asmcode + "8BF3";
        return this;
    }

    public AsmUtils Mov_ESI_ECX()
    {
        this.Asmcode = this.Asmcode + "8BF1";
        return this;
    }

    public AsmUtils Mov_ESI_EDI()
    {
        this.Asmcode = this.Asmcode + "8BF7";
        return this;
    }

    public AsmUtils Mov_ESI_EAX()
    {
        this.Asmcode = this.Asmcode + "8BF0";
        return this;
    }

    public AsmUtils Mov_ESI_EDX()
    {
        this.Asmcode = this.Asmcode + "8BF2";
        return this;
    }

    public AsmUtils Mov_ESI_ESP()
    {
        this.Asmcode = this.Asmcode + "8BF4";
        return this;
    }

    public AsmUtils Mov_ESP_EBP()
    {
        this.Asmcode = this.Asmcode + "8BE5";
        return this;
    }

    public AsmUtils Mov_ESP_EBX()
    {
        this.Asmcode = this.Asmcode + "8BE3";
        return this;
    }

    public AsmUtils Mov_ESP_ECX()
    {
        this.Asmcode = this.Asmcode + "8BE1";
        return this;
    }

    public AsmUtils Mov_ESP_EDI()
    {
        this.Asmcode = this.Asmcode + "8BE7";
        return this;
    }

    public AsmUtils Mov_ESP_EAX()
    {
        this.Asmcode = this.Asmcode + "8BE0";
        return this;
    }

    public AsmUtils Mov_ESP_EDX()
    {
        this.Asmcode = this.Asmcode + "8BE2";
        return this;
    }

    public AsmUtils Mov_ESP_ESI()
    {
        this.Asmcode = this.Asmcode + "8BE6";
        return this;
    }

    public AsmUtils Mov_EDI_EBP()
    {
        this.Asmcode = this.Asmcode + "8BFD";
        return this;
    }

    public AsmUtils Mov_EDI_EAX()
    {
        this.Asmcode = this.Asmcode + "8BF8";
        return this;
    }

    public AsmUtils Mov_EDI_EBX()
    {
        this.Asmcode = this.Asmcode + "8BFB";
        return this;
    }

    public AsmUtils Mov_EDI_ECX()
    {
        this.Asmcode = this.Asmcode + "8BF9";
        return this;
    }

    public AsmUtils Mov_EDI_EDX()
    {
        this.Asmcode = this.Asmcode + "8BFA";
        return this;
    }

    public AsmUtils Mov_EDI_ESI()
    {
        this.Asmcode = this.Asmcode + "8BFE";
        return this;
    }

    public AsmUtils Mov_EDI_ESP()
    {
        this.Asmcode = this.Asmcode + "8BFC";
        return this;
    }

    public AsmUtils Mov_EBP_EDI()
    {
        this.Asmcode = this.Asmcode + "8BDF";
        return this;
    }

    public AsmUtils Mov_EBP_EAX()
    {
        this.Asmcode = this.Asmcode + "8BE8";
        return this;
    }

    public AsmUtils Mov_EBP_EBX()
    {
        this.Asmcode = this.Asmcode + "8BEB";
        return this;
    }

    public AsmUtils Mov_EBP_ECX()
    {
        this.Asmcode = this.Asmcode + "8BE9";
        return this;
    }

    public AsmUtils Mov_EBP_EDX()
    {
        this.Asmcode = this.Asmcode + "8BEA";
        return this;
    }

    public AsmUtils Mov_EBP_ESI()
    {
        this.Asmcode = this.Asmcode + "8BEE";
        return this;
    }

    public AsmUtils Mov_EBP_ESP()
    {
        this.Asmcode = this.Asmcode + "8BEC";
        return this;
    }
    #endregion

    #region Push
    public AsmUtils Push68(int addre)
    {
        this.Asmcode = this.Asmcode + "68" + intTohex(addre, 8);

        return this;
    }

    public AsmUtils Push6A(int addre)
    {
        this.Asmcode = this.Asmcode + "6A" + intTohex(addre, 2);
        return this;
    }

    public AsmUtils Push_EAX()
    {
        this.Asmcode = this.Asmcode + "50";
        return this;
    }

    public AsmUtils Push_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "FF35" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Push_ECX()
    {
        this.Asmcode = this.Asmcode + "51";
        return this;
    }

    public AsmUtils Push_EDX()
    {
        this.Asmcode = this.Asmcode + "52";
        return this;
    }

    public AsmUtils Push_EBX()
    {
        this.Asmcode = this.Asmcode + "53";
        return this;
    }

    public AsmUtils Push_ESP()
    {
        this.Asmcode = this.Asmcode + "54";
        return this;
    }

    public AsmUtils Push_EBP()
    {
        this.Asmcode = this.Asmcode + "55";
        return this;
    }

    public AsmUtils Push_ESI()
    {
        this.Asmcode = this.Asmcode + "56";
        return this;
    }

    public AsmUtils Push_EDI()
    {
        this.Asmcode = this.Asmcode + "57";
        return this;
    }
    #endregion

    #region Call
    public AsmUtils Call_EAX()
    {
        this.Asmcode = this.Asmcode + "FFD0";
        return this;
    }

    public AsmUtils Call_EBX()
    {
        this.Asmcode = this.Asmcode + "FFD3";
        return this;
    }

    public AsmUtils Call_ECX()
    {
        this.Asmcode = this.Asmcode + "FFD1";
        return this;
    }

    public AsmUtils Call_EDX()
    {
        this.Asmcode = this.Asmcode + "FFD2";
        return this;
    }

    public AsmUtils Call_ESI()
    {
        this.Asmcode = this.Asmcode + "FFD2";
        return this;
    }

    public AsmUtils Call_ESP()
    {
        this.Asmcode = this.Asmcode + "FFD4";
        return this;
    }

    public AsmUtils Call_EBP()
    {
        this.Asmcode = this.Asmcode + "FFD5";
        return this;
    }

    public AsmUtils Call_EDI()
    {
        this.Asmcode = this.Asmcode + "FFD7";
        return this;
    }

    public AsmUtils Call_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "FF15" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Call_DWORD_Ptr_EAX()
    {
        this.Asmcode = this.Asmcode + "FF10";
        return this;
    }

    public AsmUtils Call_DWORD_Ptr_EBX()
    {
        this.Asmcode = this.Asmcode + "FF13";
        return this;
    }
    #endregion

    #region Lea
    public AsmUtils Lea_EAX_DWORD_Ptr_EAX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D40" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D80" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EAX_DWORD_Ptr_EBX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D43" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D83" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EAX_DWORD_Ptr_ECX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D41" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D81" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EAX_DWORD_Ptr_EDX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D42" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D82" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EAX_DWORD_Ptr_ESI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D46" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D86" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EAX_DWORD_Ptr_ESP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D40" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D80" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EAX_DWORD_Ptr_EBP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D4424" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D8424" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EAX_DWORD_Ptr_EDI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D47" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D87" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EBX_DWORD_Ptr_EAX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D58" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D98" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EBX_DWORD_Ptr_ESP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D5C24" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D9C24" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EBX_DWORD_Ptr_EBX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D5B" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D9B" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EBX_DWORD_Ptr_ECX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D59" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D99" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EBX_DWORD_Ptr_EDX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D5A" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D9A" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EBX_DWORD_Ptr_EDI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D5F" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D9F" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EBX_DWORD_Ptr_EBP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D5D" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D9D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EBX_DWORD_Ptr_ESI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D5E" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D9E" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_ECX_DWORD_Ptr_EAX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D48" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D88" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_ECX_DWORD_Ptr_ESP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D4C24" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D8C24" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_ECX_DWORD_Ptr_EBX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D4B" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D8B" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_ECX_DWORD_Ptr_ECX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D49" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D89" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_ECX_DWORD_Ptr_EDX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D4A" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D8A" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_ECX_DWORD_Ptr_EDI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D4F" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D8F" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_ECX_DWORD_Ptr_EBP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D4D" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D8D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_ECX_DWORD_Ptr_ESI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D4E" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D8E" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EDX_DWORD_Ptr_EAX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D50" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D90" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EDX_DWORD_Ptr_ESP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D5424" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D9424" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EDX_DWORD_Ptr_EBX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D53" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D93" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EDX_DWORD_Ptr_ECX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D51" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D91" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EDX_DWORD_Ptr_EDX_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D52" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D92" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EDX_DWORD_Ptr_EDI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D57" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D97" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EDX_DWORD_Ptr_EBP_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D55" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D95" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Lea_EDX_DWORD_Ptr_ESI_Add(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "8D56" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "8D96" + intTohex(addre, 8);
        return this;
    }
    #endregion

    #region POP
    public AsmUtils Pop_EAX()
    {
        this.Asmcode = this.Asmcode + "58";
        return this;
    }

    public AsmUtils Pop_EBX()
    {
        this.Asmcode = this.Asmcode + "5B";
        return this;
    }

    public AsmUtils Pop_ECX()
    {
        this.Asmcode = this.Asmcode + "59";
        return this;
    }

    public AsmUtils Pop_EDX()
    {
        this.Asmcode = this.Asmcode + "5A";
        return this;
    }

    public AsmUtils Pop_ESI()
    {
        this.Asmcode = this.Asmcode + "5E";
        return this;
    }

    public AsmUtils Pop_ESP()
    {
        this.Asmcode = this.Asmcode + "5C";
        return this;
    }

    public AsmUtils Pop_EDI()
    {
        this.Asmcode = this.Asmcode + "5F";
        return this;
    }

    public AsmUtils Pop_EBP()
    {
        this.Asmcode = this.Asmcode + "5D";
        return this;
    }
    #endregion

    #region CMP
    public AsmUtils Cmp_EAX(int addre)
    {
        if ((addre <= 127) && (addre >= -128))
            this.Asmcode = this.Asmcode + "83F8" + intTohex(addre, 2);
        else
            this.Asmcode = this.Asmcode + "3D" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Cmp_EAX_EDX()
    {
        this.Asmcode = this.Asmcode + "3BC2";
        return this;
    }

    public AsmUtils Cmp_EAX_DWORD_Ptr(int addre)
    {
        this.Asmcode = this.Asmcode + "3B05" + intTohex(addre, 8);
        return this;
    }

    public AsmUtils Cmp_DWORD_Ptr_EAX(int addre)
    {
        this.Asmcode = this.Asmcode + "3905" + intTohex(addre, 8);
        return this;
    }
    #endregion

    #region DEC
    public AsmUtils Dec_EAX()
    {
        this.Asmcode = this.Asmcode + "48";
        return this;
    }

    public AsmUtils Dec_EBX()
    {
        this.Asmcode = this.Asmcode + "4B";
        return this;
    }

    public AsmUtils Dec_ECX()
    {
        this.Asmcode = this.Asmcode + "49";
        return this;
    }

    public AsmUtils Dec_EDX()
    {
        this.Asmcode = this.Asmcode + "4A";
        return this;
    }
    #endregion

    #region idiv
    public AsmUtils Idiv_EAX()
    {
        this.Asmcode = this.Asmcode + "F7F8";
        return this;
    }

    public AsmUtils Idiv_EBX()
    {
        this.Asmcode = this.Asmcode + "F7FB";
        return this;
    }

    public AsmUtils Idiv_ECX()
    {
        this.Asmcode = this.Asmcode + "F7F9";
        return this;
    }

    public AsmUtils Idiv_EDX()
    {
        this.Asmcode = this.Asmcode + "F7FA";
        return this;
    }
    #endregion

    #region Imul
    public AsmUtils Imul_EAX_EDX()
    {
        this.Asmcode = this.Asmcode + "0FAFC2";
        return this;
    }

    public AsmUtils Imul_EAX(int addre)
    {
        this.Asmcode = this.Asmcode + "6BC0" + intTohex(addre, 2);
        return this;
    }

    public AsmUtils ImulB_EAX(int addre)
    {
        this.Asmcode = this.Asmcode + "69C0" + intTohex(addre, 8);
        return this;
    }
    #endregion

    #region Inc
    public AsmUtils Inc_EAX()
    {
        this.Asmcode = this.Asmcode + "40";
        return this;
    }

    public AsmUtils Inc_EBX()
    {
        this.Asmcode = this.Asmcode + "43";
        return this;
    }

    public AsmUtils Inc_ECX()
    {
        this.Asmcode = this.Asmcode + "41";
        return this;
    }

    public AsmUtils Inc_EDX()
    {
        this.Asmcode = this.Asmcode + "42";
        return this;
    }

    public AsmUtils Inc_EDI()
    {
        this.Asmcode = this.Asmcode + "47";
        return this;
    }

    public AsmUtils Inc_ESI()
    {
        this.Asmcode = this.Asmcode + "46";
        return this;
    }

    public AsmUtils Inc_DWORD_Ptr_EAX()
    {
        this.Asmcode = this.Asmcode + "FF00";
        return this;
    }

    public AsmUtils Inc_DWORD_Ptr_EBX()
    {
        this.Asmcode = this.Asmcode + "FF03";
        return this;
    }

    public AsmUtils Inc_DWORD_Ptr_ECX()
    {
        this.Asmcode = this.Asmcode + "FF01";
        return this;
    }

    public AsmUtils Inc_DWORD_Ptr_EDX()
    {
        this.Asmcode = this.Asmcode + "FF02";
        return this;
    }
    #endregion

    #region jmp
    public AsmUtils JMP_EAX()
    {
        this.Asmcode = this.Asmcode + "FFE0";
        return this;
    }
    #endregion



    public byte[] AsmChangebytes(string asmPram)
    {
        byte[] reAsmCode = new byte[asmPram.Length / 2];
        for (int i = 0; i < reAsmCode.Length; i++)
        {
            reAsmCode[i] = Convert.ToByte(Int32.Parse(asmPram.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier));
        }
        return reAsmCode;
    }
    public static AsmUtils Init(int _pid)
    {
        var asm = new AsmUtils();
        asm.pid = _pid;
        asm.Pushfd().Pushad().Push_EBP();
        return asm;
    }

    public byte[] GetAsmCode()
    {
        this.Pop_EBP().Popad().Popfd().Ret();
        return AsmChangebytes(Asmcode); ;
    }



    private void RunAsm(byte[] DShell)
    {
        IntPtr outSize;
        IntPtr hProcess = WindowUtils.OpenProcess(0x001F0FFF, false, pid!.Value);
        IntPtr addr = WindowUtils.VirtualAllocEx(hProcess, IntPtr.Zero, DShell.Length, 0x3000, 0x40);
        WindowUtils.WriteProcessMemory(hProcess, addr, DShell, DShell.Length, out outSize);
        IntPtr hThread = WindowUtils.CreateRemoteThread(hProcess, IntPtr.Zero, 0, addr, IntPtr.Zero, 0, IntPtr.Zero);
        WindowUtils.WaitForSingleObject(hThread, 1000);
        WindowUtils.VirtualFreeEx(hProcess, addr, DShell.Length, 0x4000);
        WindowUtils.CloseHandle(hThread);
        WindowUtils.CloseHandle(hProcess);
    }

    public void Run()
    {
        this.RunAsm(GetAsmCode());
    }



}
