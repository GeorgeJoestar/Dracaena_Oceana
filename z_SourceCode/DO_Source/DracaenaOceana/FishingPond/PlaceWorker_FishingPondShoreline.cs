using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldProj.FishingPond
{
    public class PlaceWorker_FishingPondShoreline : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(
            BuildableDef checkingDef,
            IntVec3 loc,
            Rot4 rot,
            Map map,
            Thing thingToIgnore = null,
            Thing thing = null)
        {
            if (map == null || checkingDef is not ThingDef thingDef)
            {
                return true;
            }

            if (!AnyCellMatches(GetTopEdgeCells(loc, rot, thingDef.Size), map, IsWaterCell))
            {
                return "DO_FishingPond_PlaceNeedsTopWater".Translate();
            }

            return true;
        }

        public override void DrawGhost(
            ThingDef def,
            IntVec3 loc,
            Rot4 rot,
            Color ghostCol,
            Thing thing = null)
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            List<IntVec3> topEdge = GetTopEdgeCells(loc, rot, def.Size).ToList();
            bool topValid = AnyCellMatches(topEdge, map, IsWaterCell);

            GenDraw.DrawFieldEdges(topEdge, topValid ? Designator_Place.CanPlaceColor.ToOpaque() : Designator_Place.CannotPlaceColor.ToOpaque());
        }

        private static IEnumerable<IntVec3> GetTopEdgeCells(IntVec3 loc, Rot4 rot, IntVec2 size)
        {
            CellRect rect = GenAdj.OccupiedRect(loc, rot, size);
            Rot4 topDir = Rot4.FromIntVec3(IntVec3.North.RotatedBy(rot));

            switch (topDir.AsInt)
            {
                case Rot4.NorthInt:
                    for (int x = rect.minX; x <= rect.maxX; x++)
                    {
                        yield return new IntVec3(x, 0, rect.maxZ + 1);
                    }

                    break;
                case Rot4.EastInt:
                    for (int z = rect.minZ; z <= rect.maxZ; z++)
                    {
                        yield return new IntVec3(rect.maxX + 1, 0, z);
                    }

                    break;
                case Rot4.SouthInt:
                    for (int x = rect.minX; x <= rect.maxX; x++)
                    {
                        yield return new IntVec3(x, 0, rect.minZ - 1);
                    }

                    break;
                case Rot4.WestInt:
                    for (int z = rect.minZ; z <= rect.maxZ; z++)
                    {
                        yield return new IntVec3(rect.minX - 1, 0, z);
                    }

                    break;
            }
        }

        private static bool AnyCellMatches(IEnumerable<IntVec3> cells, Map map, System.Func<IntVec3, Map, bool> predicate)
        {
            foreach (IntVec3 cell in cells)
            {
                if (predicate(cell, map))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsWaterCell(IntVec3 cell, Map map)
        {
            TerrainDef terrain = cell.InBounds(map) ? cell.GetTerrain(map) : null;
            return terrain != null && terrain.IsWater;
        }
    }
}
