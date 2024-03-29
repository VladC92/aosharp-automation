﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Combat;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;

namespace Desu
{
    public class ShadeCombatHandler : GenericCombatHandler
    {
        private const int ShadesCaress = 266300;
        private const int Tattoo = 269511;
        private const int MissingHealthCombatAbortPercentage = 30;
        private const int CompositeAttribute = 223372;
        private const int CompositeNano = 223380;
        private const int CompositeMelee = 223360;
        private const int CompositeMeleeSpec = 215264;
        private readonly int[] ShadeDmgProc = { 224167, 224165, 224163, 210371, 210369, 210367, 210365, 210363, 210361, 210359, 210357, 210355, 210353 };
        private Menu _menu;

        private List<PerkHash> TotemicRites = new List<PerkHash>
        {
            PerkHash.RitualOfDevotion,
            PerkHash.DevourVigor,
            PerkHash.RitualOfZeal,
            PerkHash.DevourEssence,
            PerkHash.RitualOfSpirit,
            PerkHash.DevourVitality,
            PerkHash.RitualOfBlood
        };

        private List<PerkHash> PiercingMastery = new List<PerkHash>
        {
            PerkHash.Stab,
            PerkHash.DoubleStab,
            PerkHash.Perforate,
            PerkHash.Lacerate,
            PerkHash.Impale,
            PerkHash.Gore,
            PerkHash.Hecatomb
        };

        private List<PerkHash> SpiritPhylactery = new List<PerkHash>
        {
            PerkHash.CaptureVigor,
            PerkHash.UnsealedBlight,
            PerkHash.CaptureEssence,
            PerkHash.UnsealedPestilence,
            PerkHash.CaptureSpirit,
            PerkHash.UnsealedContagion,
            PerkHash.CaptureVitality
        };

        public ShadeCombatHandler() : base()
        {
            //Perks
            RegisterPerkProcessor(PerkHash.Blur, TargetedDamagePerk);
            SpiritPhylactery.ForEach(p => RegisterPerkProcessor(p, SpiritPhylacteryPerk));
            TotemicRites.ForEach(p => RegisterPerkProcessor(p, TotemicRitesPerk));
            PiercingMastery.ForEach(p => RegisterPerkProcessor(p, PiercingMasteryPerk));

            RegisterPerkProcessor(PerkHash.ChaosRitual, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Diffuse, TargetedDamagePerk);

            //Spells
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.EmergencySneak).OrderByStackingOrder(), SmokeBombNano, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NemesisNanoPrograms).OrderByStackingOrder(), ShadesCaressNano, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.HealthDrain).OrderByStackingOrder(), HealthDrainNano);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SpiritDrain).OrderByStackingOrder(), SpiritSiphonNano);

            //Items
            RegisterItemProcessor(Tattoo, Tattoo, TattooItem, CombatActionPriority.High);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AgilityBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ConcealmentBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.FastAttackBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MultiwieldBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.RunspeedBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ShadePiercingBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SneakAttackBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.WeaponEffectAdd_On2).OrderByStackingOrder(), GenericBuff);

            RegisterSpellProcessor(ShadeDmgProc, GenericBuff);

            _menu = new Menu("CombatHandler.Shade", "CombatHandler.Shade");
            _menu.AddItem(new MenuBool("UseDrainNanoForDps", "Use drain nano for dps", true));
            _menu.AddItem(new MenuBool("UseSpiritSiphon", "Use spirit siphon", true));
            OptionPanel.AddMenu(_menu);
        }

        private bool ShadesCaressNano(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!DynelManager.LocalPlayer.IsAttacking || fightingtarget == null)
                return false;

            if (DynelManager.LocalPlayer.HealthPercent <= 50 && fightingtarget.HealthPercent > 5)
                return true;

            return false;
        }

        private bool TattooItem(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            // don't use if BM is locked (we will add this dynamically later)
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BiologicalMetamorphosis))
                return false;

            // don't use if we're above 40%
            if (DynelManager.LocalPlayer.HealthPercent > 40)
                return false;

            // don't use if nothing is fighting us
            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
                return false;

            // don't use if we have another major absorb (example: nanomage booster) running
            // we could check remaining absorb stat to be slightly more effective
            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon))
                return false;

            // don't use if our fighting target has caress running
            if (fightingtarget.Buffs.Contains(275242))
                return false;

            return true;
        }

        private bool SmokeBombNano(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;

            if (DynelManager.LocalPlayer.HealthPercent <= MissingHealthCombatAbortPercentage)
                return true;

            return false;
        }

        
        private bool SpiritSiphonNano(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!_menu.GetBool("UseSpiritSiphon"))
                return false;

            if (DynelManager.LocalPlayer.Nano < spell.Cost)
                return false;

            if (!DynelManager.LocalPlayer.IsAttacking)
                return false;

            return true;
        }

        private bool HealthDrainNano(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Nano < spell.Cost)
                return false;

            if (!DynelManager.LocalPlayer.IsAttacking)
                return false;

            // if we have caress, save enough nano to use it
            if (Spell.Find(ShadesCaress, out Spell caress))
            {
                if (DynelManager.LocalPlayer.Nano - spell.Cost < caress.Cost)
                    return false;
            }

            // only use it for dps if we have plenty of nano
            if (_menu.GetBool("UseDrainNanoForDps") && DynelManager.LocalPlayer.NanoPercent > 80)
                return true;

            // otherwise save it for if our health starts to drop
            if (DynelManager.LocalPlayer.HealthPercent >= 85)
                return false;

            return true;
        }

        private bool PiercingMasteryPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
                return false;

            //Don't PM if there are TR/SP chains in progress
            if (_actionQueue.Any(x => x.CombatAction is PerkAction action && (TotemicRites.Contains(action.Hash) || SpiritPhylactery.Contains(action.Hash))))
                return false;

            if (!(PerkAction.Find(PerkHash.Stab, out PerkAction stab) && PerkAction.Find(PerkHash.DoubleStab, out PerkAction doubleStab)))
                return true;

            if (perkAction.Hash == PerkHash.Perforate)
            {
                if (_actionQueue.Any(x => x.CombatAction is Perk action && (perkAction == stab || perkAction == doubleStab)))
                    return false;
            }

            if (!(PerkAction.Find(PerkHash.Stab, out PerkAction perforate) && PerkAction.Find(PerkHash.DoubleStab, out PerkAction lacerate)))
                return true;

            if (perkAction.Hash == PerkHash.Impale)
            {
                if (_actionQueue.Any(x => x.CombatAction is Perk action && (perkAction == stab || perkAction == doubleStab || perkAction == perforate || perkAction == lacerate)))
                    return false;
            }

            return true;
        }

        private bool SpiritPhylacteryPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
                return false;

            //Don't SP if there are TR/PM chains in progress
            if (_actionQueue.Any(x => x.CombatAction is Perk action && (TotemicRites.Contains(perkAction.Hash) || PiercingMastery.Contains(perkAction.Hash))))
                return false;
            if (fightingTarget.HealthPercent < 5)
                return false;
            return true;
        }

        private bool TotemicRitesPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
                return false;

            //Don't TR if there are SP/PM chains in progress
            if (_actionQueue.Any(x => x.CombatAction is Perk action && (SpiritPhylactery.Contains(perkAction.Hash) || PiercingMastery.Contains(perkAction.Hash))))
                return false;
            if (fightingTarget.HealthPercent < 5)
                return false;

            return true;
        }

        protected override bool TargetedDamagePerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            //Don't use if there are SP/PM/TR chains in progress
            if (_actionQueue.Any(x => x.CombatAction is Perk action && (SpiritPhylactery.Contains(perkAction.Hash) || PiercingMastery.Contains(perkAction.Hash) || TotemicRites.Contains(perkAction.Hash))))
                return false;

            actionTarget.ShouldSetTarget = true;
            return DamagePerk(perkAction, fightingTarget, ref actionTarget);
        }

        protected override bool DamagePerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
                return false;

            if (fightingTarget.Health > 50000)
                return true;

            if (fightingTarget.HealthPercent < 5)
                return false;

            //Don't use if there are SP/PM/TR chains in progress
            if (_actionQueue.Any(x => x.CombatAction is Perk action && (SpiritPhylactery.Contains(perkAction.Hash) || PiercingMastery.Contains(perkAction.Hash) || TotemicRites.Contains(perkAction.Hash))))
                return false;

            return true;
        }
    }
}
