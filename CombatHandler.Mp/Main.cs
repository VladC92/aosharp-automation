using System;
using AOSharp.Core;
using AOSharp.Core.UI;

namespace Desu
{
    public class Main : AOPluginEntry
    {
        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Mp Combat Handler Loaded!");
                AOSharp.Core.Combat.CombatHandler.Set(new MPCombatHandler());
              
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
