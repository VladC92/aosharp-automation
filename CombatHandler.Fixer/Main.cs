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
                Chat.WriteLine("Fixer Combat Handler Loaded!");
                AOSharp.Core.Combat.CombatHandler.Set(new FixerCombathandler());
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
