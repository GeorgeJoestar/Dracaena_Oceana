// using Milira;
// using RimWorld;
// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Linq;
// using System.Security.Cryptography;
// using System.Security.Policy;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;
// using Verse;
//
// namespace MilianModification
// {
//     public enum ResourceProperty : byte
//     {
//         Structure,
//         Component,
//         Crystal,
//         Energy
//     }
//
//     //public class ResourcesPoint
//     //{
//     //    public ThingDef Def { get; set; }
//     //    public ResourceProperty Property { get; set; }
//     //    public int Value { get; set; }
//
//     //    public ResourcesPoint(ThingDef def, ResourceProperty property, int value)
//     //    {
//     //        Def = def;
//     //        Property = property;
//     //        Value = value;
//     //    }
//     //}
//
//     public class ResourcesPoint
//     {
//         public ThingDef Def { get; set; }
//         public Dictionary<ResourceProperty, int> PropertyValues { get; set; }
//
//         public ResourcesPoint(ThingDef def, Dictionary<ResourceProperty, int> propertyValues)
//         {
//             Def = def;
//             PropertyValues = propertyValues;
//         }
//
//         public void SetValue(ResourceProperty property, int value)
//         {
//             PropertyValues[property] = value;
//         }
//
//         public int GetValue(ResourceProperty property)
//         {
//             return PropertyValues.TryGetValue(property, out var value) ? value : 0;
//         }
//     }
//
//     public class MilianComponentResearchUtility
//     {
//
//         public static MiliraGameComponent_MilianComponentRecipe miliraGC => Current.Game.GetComponent<MiliraGameComponent_MilianComponentRecipe>();
//
//         //材料属性对照表
//         private static Dictionary<ThingDef, ResourcesPoint> materialTable = new Dictionary<ThingDef, ResourcesPoint>
//         {
//             { 
//                 MiliraDefOf.Milira_SunPlateSteel,
//                 new ResourcesPoint(MiliraDefOf.Milira_SunPlateSteel, new Dictionary<ResourceProperty, int>
//                 {
//                     { ResourceProperty.Structure, 5 },
//                     { ResourceProperty.Component, 1 },
//                     { ResourceProperty.Crystal, 1 },
//                     { ResourceProperty.Energy, 0 }
//                 }) 
//             },
//             { 
//                 MiliraDefOf.Plasteel,
//                 new ResourcesPoint(MiliraDefOf.Plasteel, new Dictionary<ResourceProperty, int>
//                 {
//                     { ResourceProperty.Structure, 3 },
//                     { ResourceProperty.Component, 1 },
//                     { ResourceProperty.Crystal, 0 },
//                     { ResourceProperty.Energy, 0 }
//                 })
//             },
//             { 
//                 MilianDefOf.Milira_SplendidSteel,
//                 new ResourcesPoint(MilianDefOf.Milira_SplendidSteel, new Dictionary<ResourceProperty, int>
//                 {
//                     { ResourceProperty.Structure, 1 },
//                     { ResourceProperty.Component, 0 },
//                     { ResourceProperty.Crystal, 0 },
//                     { ResourceProperty.Energy, 0 }
//                 })
//             },
//             { 
//                 MiliraDefOf.Milira_SolarCrystal,
//                 new ResourcesPoint(MiliraDefOf.Milira_SolarCrystal, new Dictionary<ResourceProperty, int>
//                 {
//                     { ResourceProperty.Structure, 0 },
//                     { ResourceProperty.Component, 0 },
//                     { ResourceProperty.Crystal, 10 },
//                     { ResourceProperty.Energy, 0 }
//                 })
//             },
//             { 
//                 ThingDefOf.ComponentIndustrial,
//                 new ResourcesPoint(ThingDefOf.ComponentIndustrial, new Dictionary<ResourceProperty, int>
//                 {
//                     { ResourceProperty.Structure, 0 },
//                     { ResourceProperty.Component, 10 },
//                     { ResourceProperty.Crystal, 0 },
//                     { ResourceProperty.Energy, 0 }
//                 })
//             },
//             { 
//                 ThingDefOf.ComponentSpacer,
//                 new ResourcesPoint(ThingDefOf.ComponentSpacer, new Dictionary<ResourceProperty, int>
//                 {
//                     { ResourceProperty.Structure, 0 },
//                     { ResourceProperty.Component, 50 },
//                     { ResourceProperty.Crystal, 0 },
//                     { ResourceProperty.Energy, 0 }
//                 })
//             },
//             { 
//                 MiliraDefOf.Milira_SunLightFuel,
//                 new ResourcesPoint(ThingDefOf.ComponentSpacer, new Dictionary<ResourceProperty, int>
//                 {
//                     { ResourceProperty.Structure, 0 },
//                     { ResourceProperty.Component, 0 },
//                     { ResourceProperty.Crystal, 0 },
//                     { ResourceProperty.Energy, 2 }
//                 })
//             }
//         };
//
//         //输入材料与配件要求材料差值转化为配件满足率
//         private static readonly SimpleCurve InputResourceToRateCurve = new SimpleCurve
//         {
//             new CurvePoint(0f, 0f),
//             new CurvePoint(0.7f, 0f),
//             new CurvePoint(0.8f, 0.6f),
//             new CurvePoint(0.9f, 0.9f),
//             new CurvePoint(1.0f, 1.0f),
//             new CurvePoint(1.2f, 0.9f),
//             new CurvePoint(1.4f, 0.2f),
//             new CurvePoint(1.5f, 0f),
//             new CurvePoint(2.0f, 0f)
//         };
//
//         public static List<MilianComponentDef> milianComponentDefs => DefDatabase<MilianComponentDef>.AllDefsListForReading.ToList();
//
//         //计算每个配件对应的材料值
//         public static Dictionary<MilianComponentDef, Dictionary<ResourceProperty, int>> GetAllComponentResourcePoint()
//         {
//             var result = new Dictionary<MilianComponentDef, Dictionary<ResourceProperty, int>>();
//             foreach (var componentDef in milianComponentDefs)
//             {
//                 var resources = componentDef.costList?.ToDictionary(c => c.thingDef,c => c.count) ?? new Dictionary<ThingDef, int>();
//                 result[componentDef] = CalculateResourcePoints(resources);
//             }
//             return result;
//         }
//
//         //单个thingdef获取材料值表
//         public static Dictionary<ResourceProperty, int> GetResourcePoint(ThingDef thingDef)
//         {
//             var result = new Dictionary<ResourceProperty, int>
//             {
//                 { ResourceProperty.Structure, 0 },
//                 { ResourceProperty.Component, 0 },
//                 { ResourceProperty.Crystal, 0 },
//                 { ResourceProperty.Energy, 0 }
//             };
//             var costList = thingDef?.costList;
//             if (costList == null || costList.Count == 0)
//             {
//                 return result;
//             }
//             foreach (var costItem in costList)
//             {
//                 var def = costItem.thingDef;
//                 var count = costItem.count;
//                 if (materialTable.TryGetValue(def, out var resourceCategory))
//                 {
//                     result[ResourceProperty.Structure] += resourceCategory.GetValue(ResourceProperty.Structure) * count;
//                     result[ResourceProperty.Component] += resourceCategory.GetValue(ResourceProperty.Component) * count;
//                     result[ResourceProperty.Crystal] += resourceCategory.GetValue(ResourceProperty.Crystal) * count;
//                     result[ResourceProperty.Energy] += resourceCategory.GetValue(ResourceProperty.Energy) * count;
//                 }
//             }
//             return result;
//         }
//
//         //材料表获取材料值表
//         public static Dictionary<ResourceProperty, int> GetResourcePoint(Dictionary<ThingDef, int> resources)
//         {
//             return CalculateResourcePoints(resources);
//         }
//
//         //计算材料值
//         private static Dictionary<ResourceProperty, int> CalculateResourcePoints(Dictionary<ThingDef, int> resources)
//         {
//             var result = new Dictionary<ResourceProperty, int>
//             {
//                 { ResourceProperty.Structure, 0 },
//                 { ResourceProperty.Component, 0 },
//                 { ResourceProperty.Crystal, 0 },
//                 { ResourceProperty.Energy, 0 }
//             };
//             foreach (var entry in resources)
//             {
//                 var thingDef = entry.Key;
//                 var count = entry.Value;
//                 if (materialTable.TryGetValue(thingDef, out var resourceCategory))
//                 {
//                     result[ResourceProperty.Structure] += resourceCategory.GetValue(ResourceProperty.Structure) * count;
//                     result[ResourceProperty.Component] += resourceCategory.GetValue(ResourceProperty.Component) * count;
//                     result[ResourceProperty.Crystal] += resourceCategory.GetValue(ResourceProperty.Crystal) * count;
//                     result[ResourceProperty.Energy] += resourceCategory.GetValue(ResourceProperty.Energy) * count;
//                 }
//             }
//             return result;
//         }
//
//         public static void AddResourceToResearchRecipe(RecipeDef recipe, ThingDef thingDef, int amount)
//         {
//             IngredientCount ingredient = new IngredientCount();
//             ingredient.SetBaseCount(amount);
//             ingredient.filter.SetAllow(thingDef, true);
//             recipe.ingredients.Add(ingredient);
//         }
//
//         public static Dictionary<MilianComponentDef, float> FindApproximateComponent(Dictionary<ResourceProperty, int> resources)
//         {
//             var result = new Dictionary<MilianComponentDef, float>();
//             foreach (var kvp in miliraGC.milianComponentResearchPoint)
//             {
//                 MilianComponentDef componentDef = kvp.Key;
//                 Dictionary<ResourceProperty, int> componentResources = kvp.Value;
//
//                 float num = 4 - componentResources.Count(d => d.Value == 0);
//                 float weight = 1 / num;
//
//                 float totalRate = 0f;
//                 bool flag = false;
//
//                 foreach (ResourceProperty resourceType in Enum.GetValues(typeof(ResourceProperty)))
//                 {
//                     float numRequire = componentResources.TryGetValue(resourceType, out int requiredAmount) ? requiredAmount : 0;
//                     float resourceAmount = resources.TryGetValue(resourceType, out int availableAmount) ? availableAmount : 0;
//
//                     float rate = numRequire == 0 ? 0 : weight * InputResourceToRateCurve.Evaluate(resourceAmount / numRequire);
//                     totalRate += rate;
//                     if ((resourceAmount == 0 && numRequire != 0) || (resourceAmount != 0 && numRequire == 0))
//                     {
//                         flag = true;
//                     }
//                 }
//                 result[componentDef] = flag ? 0 : totalRate;
//                 Log.Message($"Total rate: {totalRate}");
//                 Log.Message($"{componentDef.label} requirements: {string.Join(", ", componentResources.Select(r => $"{r.Key}: {r.Value}"))}");
//             }
//             return result;
//         }
//
//         public static void TryPlaceComponentOnResearchRecipeDone(MilianComponentDef def, IntVec3 pos, Map map)
//         {
//             Thing thing = ThingMaker.MakeThing(def, null);
//             Log.Message(pos.ToString() + map + thing.def);
//             GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near, out _, null, null, default(Rot4));
//         }
//
//         public static DiaNode ResearchGuide()
//         {
//             string guide = "Milian.ResearchGuideDesc".Translate();
//             for (int i = 0; i < 15; i++)
//             {
//                 string guideDesc = "Milian.ResearchGuideDescPage" + i;
//                 guide += "\n\n" + guideDesc.Translate();
//             }
//
//             DiaNode diaNode = new DiaNode(guide);
//             //返回
//             DiaOption option_Close = new DiaOption("Ancot.Finish".Translate())
//             {
//                 resolveTree = true
//             };
//             diaNode.options.Add(option_Close);
//             return diaNode;
//         }
//
//         public static void Notify_AddedToResourceTotal(Dictionary<ThingDef, int> things)
//         {
//             foreach (var thing in things)
//             {
//                 if (miliraGC.consumedResources.ContainsKey(thing.Key))
//                 {
//                     miliraGC.consumedResources[thing.Key] += thing.Value;
//                 }
//             }
//         }
//         
//         public static void Notify_RemoveFromResourceTotal(ThingDef thingdef, int num)
//         {
//             if (miliraGC.consumedResources.ContainsKey(thingdef))
//             {
//                 miliraGC.consumedResources[thingdef] -= num;
//             }
//         }
//     }
// }
