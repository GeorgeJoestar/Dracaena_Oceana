using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimWorldProj.FishingPond
{
    public class RecipeWorker_UpgradeFishingPond : RecipeWorker
    {
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            AcceptanceReport report = base.AvailableReport(thing, part);
            if (!report.Accepted || thing == null)
            {
                return report;
            }

            if (RoofUtility.IsAnyCellUnderRoof(thing))
            {
                return "MustPlaceUnroofed".Translate();
            }

            return true;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);

            Building_WorkTable sourceTable = billDoer?.jobs?.curJob?.GetTarget(TargetIndex.A).Thing as Building_WorkTable;
            if (sourceTable == null || sourceTable.Destroyed || !sourceTable.Spawned)
            {
                return;
            }

            DefModExtension_UpgradeRecipe extension = recipe.GetModExtension<DefModExtension_UpgradeRecipe>();
            if (extension?.targetStageDef == null)
            {
                Log.Warning($"[Dracaena_Oceana] Recipe {recipe.defName} is missing {nameof(DefModExtension_UpgradeRecipe)} targetStageDef.");
                return;
            }

            if (!FishingPondUpgradeUtility.TryUpgradeBuilding(sourceTable, extension.targetStageDef, out Building newBuilding))
            {
                Messages.Message(
                    "DO_FishingPond_UpgradeFailed".Translate(sourceTable.LabelCap),
                    sourceTable,
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            Messages.Message(
                "DO_FishingPond_UpgradeCompleted".Translate(sourceTable.LabelCap, newBuilding.LabelCap),
                newBuilding,
                MessageTypeDefOf.TaskCompletion,
                historical: false);
        }
    }
}
