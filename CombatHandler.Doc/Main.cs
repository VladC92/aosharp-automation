using AOSharp.Core;
using System;
using AOSharp.Core.UI;

namespace Desu
{
    public class Main : AOPluginEntry
    {
        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Doctor Combat Handler Loaded!", AOSharp.Common.GameData.ChatColor.DarkPink);
                AOSharp.Core.Combat.CombatHandler.Set(new DocCombatHandler());
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
