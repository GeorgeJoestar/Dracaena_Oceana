using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldProj.FishingPond
{
    public class FishingPondCatchOption
    {
        public ThingDef thingDef;
        public int count = 1;
        public float weight = 1f;
    }

    public class CompProperties_FishingPondProduction : CompProperties
    {
        public int cycleDurationTicks = 60000;
        public int rollsPerCycle = 1;
        public List<FishingPondCatchOption> catches = new List<FishingPondCatchOption>();

        public CompProperties_FishingPondProduction()
        {
            compClass = typeof(CompFishingPondProduction);
        }
    }

    public class CompFishingPondProduction : ThingComp
    {
        private bool cycleActive;
        private int ticksRemaining;

        private CompProperties_FishingPondProduction Props => (CompProperties_FishingPondProduction)props;

        public bool CycleActive => cycleActive;

        public bool BlockedByRoof => parent.Spawned && parent.Map != null && RoofUtility.IsAnyCellUnderRoof(parent);

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cycleActive, "cycleActive");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!cycleActive || parent.Map == null || !parent.Spawned)
            {
                return;
            }

            if (BlockedByRoof)
            {
                return;
            }

            ticksRemaining--;
            if (ticksRemaining > 0)
            {
                return;
            }

            CompleteCycle();
        }

        public override string CompInspectStringExtra()
        {
            if (!cycleActive)
            {
                return "DO_FishingPond_CycleIdleInspect".Translate();
            }

            if (BlockedByRoof)
            {
                return "DO_FishingPond_CycleBlockedRoofedInspect".Translate();
            }

            return "DO_FishingPond_CycleInProgressInspect".Translate(Math.Max(0, ticksRemaining).ToStringTicksToPeriod());
        }

        public bool TryStartCycle()
        {
            if (cycleActive || !HasValidCatchConfig())
            {
                return false;
            }

            cycleActive = true;
            ticksRemaining = Math.Max(1, Props.cycleDurationTicks);
            parent.BroadcastCompSignal("DO_FishingPond_CycleStarted");
            return true;
        }

        public void CopyStateFrom(CompFishingPondProduction source)
        {
            if (source == null)
            {
                return;
            }

            cycleActive = source.cycleActive;
            ticksRemaining = source.ticksRemaining;
            if (cycleActive && ticksRemaining <= 0)
            {
                ticksRemaining = Math.Max(1, Props.cycleDurationTicks);
            }
        }

        private void CompleteCycle()
        {
            cycleActive = false;
            ticksRemaining = 0;
            TryProduceCatch();
        }

        private bool HasValidCatchConfig()
        {
            if (Props.catches == null || Props.catches.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < Props.catches.Count; i++)
            {
                FishingPondCatchOption option = Props.catches[i];
                if (option?.thingDef != null && option.count > 0 && option.weight > 0f)
                {
                    return true;
                }
            }

            return false;
        }

        private void TryProduceCatch()
        {
            if (parent.Map == null || !parent.Spawned || !HasValidCatchConfig())
            {
                return;
            }

            int rolls = Math.Max(1, Props.rollsPerCycle);
            bool producedAny = false;

            for (int i = 0; i < rolls; i++)
            {
                FishingPondCatchOption option = PickCatchOption();
                if (option?.thingDef == null)
                {
                    continue;
                }

                SpawnStacks(option.thingDef, Math.Max(1, option.count));
                producedAny = true;
            }

            if (producedAny)
            {
                Messages.Message(
                    "DO_FishingPond_CycleFinished".Translate(parent.LabelCap),
                    parent,
                    MessageTypeDefOf.TaskCompletion,
                    historical: false);
            }
        }

        private FishingPondCatchOption PickCatchOption()
        {
            float totalWeight = 0f;
            for (int i = 0; i < Props.catches.Count; i++)
            {
                FishingPondCatchOption option = Props.catches[i];
                if (option?.thingDef == null || option.count <= 0 || option.weight <= 0f)
                {
                    continue;
                }

                totalWeight += option.weight;
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float randomWeight = Rand.Range(0f, totalWeight);
            float running = 0f;
            for (int i = 0; i < Props.catches.Count; i++)
            {
                FishingPondCatchOption option = Props.catches[i];
                if (option?.thingDef == null || option.count <= 0 || option.weight <= 0f)
                {
                    continue;
                }

                running += option.weight;
                if (randomWeight <= running)
                {
                    return option;
                }
            }

            return Props.catches[Props.catches.Count - 1];
        }

        private void SpawnStacks(ThingDef thingDef, int totalCount)
        {
            if (totalCount <= 0 || thingDef == null || parent.Map == null)
            {
                return;
            }

            IntVec3 dropCell = parent.InteractionCell;
            if (!dropCell.IsValid || !dropCell.InBounds(parent.Map))
            {
                dropCell = parent.Position;
            }

            int remaining = totalCount;
            while (remaining > 0)
            {
                Thing stack = ThingMaker.MakeThing(thingDef);
                int stackCount = Math.Min(remaining, stack.def.stackLimit);
                stack.stackCount = stackCount;
                remaining -= stackCount;

                GenPlace.TryPlaceThing(stack, dropCell, parent.Map, ThingPlaceMode.Near, out _);
            }
        }
    }
}
