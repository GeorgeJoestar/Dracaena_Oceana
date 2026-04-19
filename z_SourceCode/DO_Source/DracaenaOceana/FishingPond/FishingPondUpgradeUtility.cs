using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldProj.FishingPond
{
    public static class FishingPondUpgradeUtility
    {
        public static bool TryUpgradeBuilding(Building_WorkTable sourceTable, ThingDef targetStageDef, out Building newBuilding)
        {
            newBuilding = null;
            if (sourceTable == null || targetStageDef == null || sourceTable.Destroyed || !sourceTable.Spawned || sourceTable.Map == null)
            {
                return false;
            }

            Map map = sourceTable.Map;
            IntVec3 position = sourceTable.Position;
            Rot4 rotation = sourceTable.Rotation;
            Faction faction = sourceTable.Faction;

            float hpPercent = sourceTable.MaxHitPoints > 0
                ? (float)sourceTable.HitPoints / sourceTable.MaxHitPoints
                : 1f;

            bool wasForbidden = sourceTable.Faction == Faction.OfPlayer && sourceTable.IsForbidden(Faction.OfPlayer);

            List<Bill> billsToTransfer = CollectTransferableBills(sourceTable, targetStageDef);
            CompFishingPondProduction oldProductionComp = sourceTable.GetComp<CompFishingPondProduction>();

            sourceTable.DeSpawn(DestroyMode.WillReplace);

            Thing newThing = targetStageDef.MadeFromStuff
                ? ThingMaker.MakeThing(targetStageDef, sourceTable.Stuff)
                : ThingMaker.MakeThing(targetStageDef);

            if (faction != null)
            {
                newThing.SetFaction(faction);
            }

            newBuilding = GenSpawn.Spawn(newThing, position, map, rotation, WipeMode.Vanish) as Building;
            if (newBuilding == null)
            {
                return false;
            }

            if (faction != null && newBuilding.Faction != faction)
            {
                newBuilding.SetFaction(faction);
            }

            if (newBuilding.def.useHitPoints && newBuilding.MaxHitPoints > 0)
            {
                int newHitPoints = Mathf.Clamp(Mathf.RoundToInt(newBuilding.MaxHitPoints * hpPercent), 1, newBuilding.MaxHitPoints);
                newBuilding.HitPoints = newHitPoints;
            }

            if (wasForbidden && newBuilding.Faction == Faction.OfPlayer)
            {
                newBuilding.SetForbidden(value: true, warnOnFail: false);
            }

            if (newBuilding is Building_WorkTable newTable)
            {
                TransferBills(newTable, billsToTransfer);
            }

            if (oldProductionComp != null)
            {
                CompFishingPondProduction newProductionComp = newBuilding.GetComp<CompFishingPondProduction>();
                newProductionComp?.CopyStateFrom(oldProductionComp);
            }

            return true;
        }

        private static List<Bill> CollectTransferableBills(Building_WorkTable sourceTable, ThingDef targetStageDef)
        {
            List<Bill> results = new List<Bill>();
            if (sourceTable.BillStack == null || sourceTable.BillStack.Bills == null)
            {
                return results;
            }

            List<RecipeDef> targetRecipes = targetStageDef.AllRecipes;
            if (targetRecipes == null || targetRecipes.Count == 0)
            {
                return results;
            }

            List<Bill> sourceBills = sourceTable.BillStack.Bills;
            for (int i = 0; i < sourceBills.Count; i++)
            {
                Bill sourceBill = sourceBills[i];
                if (sourceBill?.recipe == null || !targetRecipes.Contains(sourceBill.recipe))
                {
                    continue;
                }

                Bill clone = sourceBill.Clone();
                clone.InitializeAfterClone();
                results.Add(clone);
            }

            return results;
        }

        private static void TransferBills(Building_WorkTable targetTable, List<Bill> bills)
        {
            if (targetTable.BillStack == null || bills == null || bills.Count == 0)
            {
                return;
            }

            for (int i = 0; i < bills.Count && targetTable.BillStack.Count < 15; i++)
            {
                targetTable.BillStack.AddBill(bills[i]);
            }
        }
    }
}

