using Mir2Assistant.Common.Models;
using Mir2Assistant.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Functions;
/// <summary>
/// 走路、跑路、寻路
/// </summary>
public static class GoRunFunction
{
    /// <summary>
    /// 走路跑路
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="type">1走路，2跑路，3骑黑马跑</param>
    /// <param name="x">当前x</param>
    /// <param name="x">当前y</param>
    /// <param name="direct">方向，小键盘数字</param>
    /// <param name="走路参数">搜索 8B00 8B4C24 20获取</param>
    /// <param name="走路CALL地址"></param>
    public static void GoRun(MirGameInstanceModel gameInstance, int x, int y, byte direct, byte type)
    {
        int dir = 0;
        switch (direct)
        {
            case 1:
                dir = 5;
                x -= type;
                y += type;
                break;
            case 2:
                dir = 4;
                y += type;
                break;
            case 3:
                dir = 3;
                x += type;
                y += type;
                break;
            case 4:
                dir = 6;
                x -= type;
                break;
            case 6:
                dir = 2;
                x += type;
                break;
            case 7:
                dir = 7;
                x -= type;
                y -= type;
                break;
            case 8:
                dir = 0;
                y -= type;
                break;
            case 9:
                dir = 1;
                x += type;
                y -= type;
                break;
        }
        int typePara = 0;
        switch (type)
        {
            case 2:
                typePara = 0xbc5;
                break;
            case 3:
                typePara = 0x19100c;
                break;
            default:
                typePara = 0xbc3;
                break;
        }
        SendMirCall.Send(gameInstance, 1001, new nint[] { x, y, dir, typePara, gameInstance!.MirConfig["角色基址"], gameInstance!.MirConfig["走路CALL地址"] });
        //AsmUtils.Init(gameInstance.MirPid)
        //   .Push68(y)
        //   .Push6A(dir)
        //   .Push6A(0)
        //   .Push6A(0)
        //   .Push6A(0)
        //   .Push6A(0)
        //   .Push6A(0)
        //   .Mov_EAX((int)gameInstance!.MirConfig["角色基址"])
        //   .Mov_EAX_DWORD_Ptr_EAX()
        //   .Mov_EAX_DWORD_Ptr_EAX()
        //   .Mov_ECX(x)
        //   .Mov_EDX(typePara)
        //   .Mov_EBX((int)gameInstance!.MirConfig["走路CALL地址"])
        //   .Call_EBX()
        //   .Run();
        
    }

    /// <summary>
    /// 寻路
    /// </summary>
    /// <param name="x">要去的坐标x</param>
    /// <param name="y">要去的坐标y</param>
    /// <param name="寻路参数"></param>
    /// <param name="寻路CALL地址"></param>
    public static void FindPath(MirGameInstanceModel gameInstance, int x, int y)
    {
        SendMirCall.Send(gameInstance, 1002, new nint[] { x, y, gameInstance!.MirConfig["寻路参数"], gameInstance!.MirConfig["寻路CALL地址"] });
    }


    public static void FlyCY(MirGameInstanceModel gameInstance)
    {
        SendMirCall.Send(gameInstance, 1010, new nint[] { gameInstance!.MirConfig["通用参数"], gameInstance!.MirConfig["对话CALL地址"] });
    }
}
