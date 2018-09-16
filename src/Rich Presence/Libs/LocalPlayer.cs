// Decompiled with JetBrains decompiler
// Type: CharacterData.Utils.LocalPlayer
// Assembly: CharacterData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 74E598EA-D86C-4665-83EF-E2CAA5899D71
// Assembly location: F:\Tools\Path of Exile Tools\Macros\plugins\Character Data\CharacterData.dll

using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;

namespace Rich_Presence.Libs
{
    public class LocalPlayer
    {
        public static EntityWrapper Entity => BasePlugin.API.GameController.Player;

        public static long Experience => Entity.GetComponent<Player>().XP;

        public static string Name => Entity.GetComponent<Player>().PlayerName;

        public static int Level => Entity.GetComponent<Player>().Level;

        public static Stats Stat => Entity.GetComponent<Stats>();

        public static Life Health => Entity.GetComponent<Life>();

        public static AreaInstance Area => BasePlugin.API.GameController.Area.CurrentArea;

        public static uint AreaHash => BasePlugin.API.GameController.Game.IngameState.Data.CurrentAreaHash;

        public static bool HasBuff(string buffName)
        {
            return Entity.GetComponent<Life>().HasBuff(buffName);
        }
    }
}