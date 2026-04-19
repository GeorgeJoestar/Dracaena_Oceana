using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimWorldProj.Patches
{
    internal static class DracaenaBodyTypeUtility
    {
        private const string DracaenaDefName = "Dracaena";

        internal static void Normalize(Pawn pawn)
        {
            if (!IsDracaena(pawn) || pawn.story == null || pawn.story.bodyType == BodyTypeDefOf.Female)
            {
                return;
            }

            pawn.story.bodyType = BodyTypeDefOf.Female;
        }

        internal static bool ShouldForceFemaleBodyType(RimWorld.Apparel apparel)
        {
            if (apparel?.Wearer != null)
            {
                Normalize(apparel.Wearer);
                return IsDracaena(apparel.Wearer);
            }

            return apparel?.def?.defName?.StartsWith("Apparel_DO_", StringComparison.Ordinal) == true;
        }

        private static bool IsDracaena(Pawn pawn)
        {
            return pawn?.def?.defName == DracaenaDefName;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
    internal static class DracaenaGenerateBodyTypePatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Postfix(ref Pawn pawn)
        {
            DracaenaBodyTypeUtility.Normalize(pawn);
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel))]
    internal static class DracaenaApparelGraphicRecordPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(RimWorld.Apparel apparel, ref BodyTypeDef bodyType)
        {
            if (bodyType != BodyTypeDefOf.Female && DracaenaBodyTypeUtility.ShouldForceFemaleBodyType(apparel))
            {
                bodyType = BodyTypeDefOf.Female;
            }
        }
    }
}
