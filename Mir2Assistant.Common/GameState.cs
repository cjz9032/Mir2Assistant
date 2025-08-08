using Mir2Assistant.Common.Models;
using System.Collections.Generic;

namespace Mir2Assistant.Common
{
    public static class GameState
    {
        public static string gamePath = "";

        public static Dictionary<string, nint> MirConfig = new Dictionary<string, nint>();

        public static List<MirGameInstanceModel> GameInstances { get; } = new List<MirGameInstanceModel>();
    }

} 