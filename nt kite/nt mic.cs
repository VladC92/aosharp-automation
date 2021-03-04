﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System;

namespace Desu
{
    public class NTCombatHandler : GenericCombatHandler
    {
        private Menu _menu;

        public NTCombatHandler()
        {

            RegisterSpellProcessor(RelevantNanos.VolcanicEruption, AOE);
          
     
            RegisterPerkProcessor(PerkHash.FlimFocus, DamagePerk);
 






            _menu = new Menu("CombatHandler.NT", "CombatHandler.NT");

            OptionPanel.AddMenu(_menu);
        }


        private bool AOE(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)

                return false;
            return true;

        
        }
        private bool NanoPerk(Perk perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.NanoPercent < 30)
                return true;

            return false;
        }
        private bool RegainNano(Perk perk, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (fightingtarget == null)
                return false;

            if (DynelManager.LocalPlayer.MaxNano < 800)
                return DynelManager.LocalPlayer.NanoPercent < 50;

            return DynelManager.LocalPlayer.MissingNano > 800;
        }




        private static class RelevantNanos
        {
            public const int VolcanicEruption = 28638;


            //Buffs
            public static readonly int[] NanobotShelter = { 273388, 263265 };
            public static readonly int CompositeAttribute = 223372;
            public static readonly int CompositeNano = 223380;
            public static readonly int[] NanoBuffs = { 95417, 273386, 150631, 270802, 90406, 95443 };

        }
     
    }
}
