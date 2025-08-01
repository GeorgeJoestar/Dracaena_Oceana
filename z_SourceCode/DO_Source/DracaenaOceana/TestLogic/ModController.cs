using RimWorld;
using UnityEngine;
using Verse;
using RimWorldProj.WelcomeScreen;  
using HarmonyLib;

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
                Rect buttonRect = new Rect(UI.screenWidth - buttonWidth - 10f, UI.screenHeight - buttonHeight - 10f, buttonWidth, buttonHeight);
                
                if (Widgets.ButtonText(buttonRect, "Reopen Window"))
                {
                    Find.WindowStack.Add(new WelcomeWindow());
                }
            }
        }
    }
}