// using RimWorld;
// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Linq;
// using System.Security.Cryptography;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;
// using Verse;
// using Verse.Noise;
//
// namespace MilianModification
// {
//     public class Recipe_MilianComponentResearch : RecipeWorker
//     {
//         private static readonly SimpleCurve IntelligenceToThresholdCurve = new SimpleCurve
//         {
//             new CurvePoint(0f, 0.7f),
//             new CurvePoint(5f, 0.65f),
//             new CurvePoint(8f, 0.6f),
//             new CurvePoint(10f, 0.54f),
//             new CurvePoint(16f, 0.5f)
//         };
//
//         public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
//         {
//             var resources = ConvertIngredientsToDictionary(recipe.ingredients);
//             var resourcePoint = MilianComponentResearchUtility.GetResourcePoint(resources);
//             MilianComponentResearchUtility.Notify_AddedToResourceTotal(resources);
//             Dictionary<MilianComponentDef, float> possibleComponents = MilianComponentResearchUtility.FindApproximateComponent(resourcePoint)
//                 .Where(kvp => !kvp.Key.recipeMaker.factionPrerequisiteTags.NullOrEmpty() && !MilianComponentResearchUtility.miliraGC.milianComponentAcquire.Contains(kvp.Key) && kvp.Value > IntelligenceToThresholdCurve.Evaluate(billDoer.skills.GetSkill(SkillDefOf.Intellectual).levelInt))
//                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
//
//             if (possibleComponents.Any())
//             {
//                 var kvp = possibleComponents.RandomElementByWeight(c => (float)Math.Pow((c.Value - 0.3f), 2));
//                 MilianComponentDef componentDef = kvp.Key;
//                 float num = kvp.Value;
//                 MilianComponentResearchUtility.miliraGC.Notify_PlayerAcquireRecipeOf(componentDef);
//                 
//                 string letterDesc = "Milian.ComponentRecipeFoundDesc_Base".Translate(billDoer.LabelShort, componentDef.label);
//                 if (num >= 0.8)
//                 {
//                     MilianComponentResearchUtility.TryPlaceComponentOnResearchRecipeDone(componentDef, billDoer.Position, billDoer.Map);
//                     letterDesc += "\n" + "Milian.ComponentRecipeFoundDesc_GreatSuccess".Translate(billDoer.LabelShort, componentDef.label);
//                 }
//                 if (possibleComponents.Count() > 1)
//                 {
//                     letterDesc += "\n\n" + "Milian.ComponentRecipeFoundDesc_Inspiration".Translate(billDoer.LabelShort, componentDef.label);
//                 }
//                 
//                 Find.LetterStack.ReceiveLetter("Milian.ComponentRecipeFound".Translate(componentDef.label), letterDesc, LetterDefOf.PositiveEvent, null, hyperlinkThingDefs: new List<ThingDef> { componentDef });
//             }
//             else
//             {
//                 Find.LetterStack.ReceiveLetter("Milian.ResearchRecipeFailed".Translate(billDoer.LabelShort), "Milian.ResearchRecipeFailedDesc".Translate(billDoer.LabelShort), LetterDefOf.NeutralEvent);
//                 foreach(var resource in resources)
//                 {
//                     ThingDef def = resource.Key;
//                     int amount = (int)(0.5f * resource.Value);    
//                     if(amount > 0)
//                     {
//                         Thing thing = ThingMaker.MakeThing(def, null);
//                         thing.stackCount = amount;
//                         GenPlace.TryPlaceThing(thing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near, out _, null, null, default(Rot4));
//                         MilianComponentResearchUtility.Notify_RemoveFromResourceTotal(def, amount);
//                     }
//                 }
//             }
//             foreach (var component in MilianComponentResearchUtility.FindApproximateComponent(resourcePoint))
//             {
//                 Log.Message($"{component.Key.label} {component.Value.ToStringPercent()}");
//             }
//         }
//
//         public static Dictionary<ThingDef, int> ConvertIngredientsToDictionary(List<IngredientCount> ingredients)
//         {
//             var result = new Dictionary<ThingDef, int>();
//
//             foreach (var ingredient in ingredients)
//             {
//                 foreach (var thingDef in ingredient.filter.AllowedThingDefs)
//                 {
//                     int count = Mathf.CeilToInt(ingredient.GetBaseCount());
//                     if (result.ContainsKey(thingDef))
//                     {
//                         result[thingDef] += count;
//                     }
//                     else
//                     {
//                         result[thingDef] = count;
//                     }
//                 }
//             }
//             return result;
//         }
//     }
// }
