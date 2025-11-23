using RimWorld;
using UnityEngine;
using Verse;
using RimWorldProj.WelcomeScreen;  
using HarmonyLib;
using RimWorldProj.TestLogic.UI;

// using RimWorldProj.TestLogic.UI;

namespace RimWorldProj.TestLogic
{
    [StaticConstructorOnStartup]
    public static class MainMenuPatches
    {
        static MainMenuPatches()
        {
            Harmony harmony = new ("com.rimworldproj.testlogic");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
        public static class MainMenuDrawerDoMainMenuControlsPatch
        {
            public static void Postfix()
            {
                float buttonWidth = 150f;
                float buttonHeight = 35f;
                Rect buttonRect = new Rect(Verse.UI.screenWidth - buttonWidth - 10f, Verse.UI.screenHeight - buttonHeight - 10f, buttonWidth, buttonHeight);
                
                if (Widgets.ButtonText(buttonRect, "Reopen Window"))
                {
                    UiPreviewWindow.TryOpenFromJsonAsset();
                    // Find.WindowStack.Add(new WelcomeWindow());
                }
            }
        }
    }
}