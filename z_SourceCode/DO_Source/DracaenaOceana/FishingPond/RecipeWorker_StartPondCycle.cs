using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimWorldProj.FishingPond
{
    public class RecipeWorker_FishingPondOutdoorOnly : RecipeWorker
    {
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            AcceptanceReport baseReport = base.AvailableReport(thing, part);
            if (!baseReport.Accepted || thing == null)
            {
                return baseReport;
            }

            if (RoofUtility.IsAnyCellUnderRoof(thing))
            {
                return "MustPlaceUnroofed".Translate();
            }

            return true;
        }
    }

    public class RecipeWorker_StartPondCycle : RecipeWorker
    {
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            AcceptanceReport report = base.AvailableReport(thing, part);
            if (!report.Accepted)
            {
                return report;
            }

            if (thing == null)
            {
                return false;
            }

            if (RoofUtility.IsAnyCellUnderRoof(thing))
            {
                return "MustPlaceUnroofed".Translate();
            }

            CompFishingPondProduction productionComp = thing.TryGetComp<CompFishingPondProduction>();
            if (productionComp != null && productionComp.CycleActive)
            {
                return "DO_FishingPond_CycleAlreadyActive".Translate(thing.LabelCap);
            }

            return true;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);

            Building_WorkTable table = billDoer?.jobs?.curJob?.GetTarget(TargetIndex.A).Thing as Building_WorkTable;
            if (table == null || table.Destroyed || !table.Spawned)
            {
                return;
            }

            CompFishingPondProduction productionComp = table.GetComp<CompFishingPondProduction>();
            if (productionComp == null)
            {
                Log.Warning($"[Dracaena_Oceana] Recipe {recipe.defName} completed on {table.def.defName} but no {nameof(CompFishingPondProduction)} was found.");
                return;
            }

            if (productionComp.TryStartCycle())
            {
                Messages.Message(
                    "DO_FishingPond_CycleStarted".Translate(table.LabelCap),
                    table,
                    MessageTypeDefOf.TaskCompletion,
                    historical: false);
            }
            else
            {
                Messages.Message(
                    "DO_FishingPond_CycleAlreadyActive".Translate(table.LabelCap),
                    table,
                    MessageTypeDefOf.RejectInput,
                    historical: false);
            }
        }
    }
}
