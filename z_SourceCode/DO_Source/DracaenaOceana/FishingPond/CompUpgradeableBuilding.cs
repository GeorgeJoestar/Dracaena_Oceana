using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorldProj.FishingPond
{
    public class CompProperties_UpgradeableBuilding : CompProperties
    {
        public ThingDef nextStageDef;
        public RecipeDef upgradeRecipeDef;
        public string commandLabelKey = "DO_FishingPond_UpgradeLabel";
        public string commandDescKey = "DO_FishingPond_UpgradeDesc";

        public CompProperties_UpgradeableBuilding()
        {
            compClass = typeof(CompUpgradeableBuilding);
        }
    }

    public class CompUpgradeableBuilding : ThingComp
    {
        private Building_WorkTable ParentTable => parent as Building_WorkTable;

        private CompProperties_UpgradeableBuilding Props => (CompProperties_UpgradeableBuilding)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            Building_WorkTable table = ParentTable;
            if (table == null || !parent.Spawned || Props.upgradeRecipeDef == null || Props.nextStageDef == null)
            {
                yield break;
            }

            bool isQueued = IsUpgradeQueued(table);
            bool recipeAvailable = Props.upgradeRecipeDef.AvailableNow && Props.upgradeRecipeDef.AvailableOnNow(parent);
            Material iconMaterial;
            Texture iconTexture = Widgets.GetIconFor(Props.nextStageDef, out iconMaterial);

            Command_UpgradeFishingPond command = new Command_UpgradeFishingPond
            {
                defaultLabel = Props.commandLabelKey.Translate(Props.nextStageDef.LabelCap),
                defaultDesc = Props.commandDescKey.Translate(parent.LabelCap, Props.nextStageDef.LabelCap, Props.upgradeRecipeDef.LabelCap),
                icon = iconTexture,
                overrideMaterial = iconMaterial,
                defaultIconColor = Props.nextStageDef.uiIconColor,
                action = QueueUpgrade
            };

            if (isQueued)
            {
                command.Disable("DO_FishingPond_UpgradeAlreadyQueued".Translate(parent.LabelCap));
            }
            else if (!recipeAvailable)
            {
                command.Disable("DO_FishingPond_UpgradeUnavailable".Translate(Props.upgradeRecipeDef.LabelCap));
            }

            yield return command;
        }

        public override string CompInspectStringExtra()
        {
            Building_WorkTable table = ParentTable;
            if (table == null || !IsUpgradeQueued(table))
            {
                return null;
            }

            return "DO_FishingPond_UpgradeInProgressInspect".Translate(Props.nextStageDef.LabelCap);
        }

        private void QueueUpgrade()
        {
            Building_WorkTable table = ParentTable;
            if (table == null || table.BillStack == null || Props.upgradeRecipeDef == null)
            {
                return;
            }

            if (IsUpgradeQueued(table))
            {
                Messages.Message(
                    "DO_FishingPond_UpgradeAlreadyQueued".Translate(parent.LabelCap),
                    parent,
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            if (table.BillStack.Count >= 15)
            {
                Messages.Message(
                    "DO_FishingPond_BillStackFull".Translate(parent.LabelCap),
                    parent,
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            Bill_Production bill = new Bill_Production(Props.upgradeRecipeDef)
            {
                repeatMode = BillRepeatModeDefOf.RepeatCount,
                repeatCount = 1,
                suspended = false
            };

            table.BillStack.AddBill(bill);
            SoundDefOf.Tick_High.PlayOneShotOnCamera();

            Messages.Message(
                "DO_FishingPond_UpgradeQueued".Translate(parent.LabelCap, Props.nextStageDef.LabelCap),
                parent,
                MessageTypeDefOf.TaskCompletion,
                historical: false);
        }

        private bool IsUpgradeQueued(Building_WorkTable table)
        {
            List<Bill> bills = table.BillStack?.Bills;
            if (bills == null)
            {
                return false;
            }

            for (int i = 0; i < bills.Count; i++)
            {
                Bill bill = bills[i];
                if (bill != null && !bill.deleted && bill.recipe == Props.upgradeRecipeDef)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
