using Mir2Assistant.Common.Models;
using System.Collections.Generic;

namespace Mir2Assistant.Common
{
    public static class GameState
    {
        public static List<MirGameInstanceModel> GameInstances { get; } = new List<MirGameInstanceModel>();
    }
} 