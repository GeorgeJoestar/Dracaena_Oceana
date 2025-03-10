using RimWorld;
using Verse;
using UnityEngine;

namespace RimWorldProj.TestLogic
{
    public class Building_Workbench : Building_WorkTable
    {
        private bool usePartialGraphic = false;
        private Graphic fullGraphic;
        private Graphic partialGraphic;

        public override Graphic Graphic => usePartialGraphic ? partialGraphic : fullGraphic;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            fullGraphic = def.graphicData.GraphicColoredFor(this);
            partialGraphic = GraphicDatabase.Get<Graphic_Single>(
                "TestWindow/CustomButton", 
                ShaderDatabase.Cutout, 
                def.graphicData.drawSize, 
                Color.white
            );
            UpdateGraphic();
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 60 == 0) UpdateGraphic();
        }

        private void UpdateGraphic()
        {
            if (this.Rotation == Rot4.South)
            {
                bool wallFound = false;
                IntVec3 southCell = this.Position + IntVec3.South;
                foreach (Thing t in southCell.GetThingList(this.Map))
                {
                    if (t.def.category != ThingCategory.Building ||
                        t.def.building.buildingTags == null ||
                        !t.def.building.buildingTags.Contains("Wall")) continue;
                    wallFound = true;
                    break;
                }
                usePartialGraphic = wallFound;
            }
            else
            {
                // For non-southern facing benches, always show the full graphic.
                usePartialGraphic = false;
            }
        }

    }
}
