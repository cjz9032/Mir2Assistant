using Mir2Assistant.Common.Models;
using System.Collections.Generic;

namespace Mir2Assistant.Common
{
    public static class GameState
    {
        public static Dictionary<int, MirGameInstanceModel> GameInstances { get; } = new Dictionary<int, MirGameInstanceModel>();
    }
} 