using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Functions
{
    static class CharacerStatusFunction
    {
        public static void GetInfo(AssiastantForm assiastantForm)
        {
            var status = assiastantForm.CharacerStatus;
            var memoryUtils = assiastantForm.memoryUtils!;
            var 角色基址 = assiastantForm.角色基址;
            status.Name = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0x4, 0));
            status.MapName = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0x10, 0));
            status.X = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0, 0x128));
            status.Y = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0, 0x12c));
            status.CurrentHP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0, 0xa8));
            status.MaxHP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0, 0x68));
            status.CurrentMP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0, 0x90));
            status.MaxMP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0, 0x80));
            status.GradeZS = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress("mir1", 角色基址, 0, 0x1dd8));
        }
    }
}
