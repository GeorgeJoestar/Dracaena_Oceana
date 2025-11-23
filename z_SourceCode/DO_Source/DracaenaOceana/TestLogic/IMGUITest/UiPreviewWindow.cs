using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RwUi.Shared;

namespace RimWorldProj.TestLogic.UI
{
    public sealed class UiPreviewWindow : DefaultWindow
    {
        private readonly UiLayout layout;
        private UiRuntimeRenderer renderer;
        private UiLayoutController ui;

        // --- Tabs ---
        private enum Tab { Welcome, FAQ, Credit }
        private Tab currentTab = Tab.Welcome;

        // tab ids
        private const string NavWelcome      = "NavTabWelcome";
        private const string NavWelcomeClose = "NavTabWelcomeClose";
        private const string NavFAQ          = "NavTabFAQ";
        private const string NavFAQClose     = "NavTabFAQClose";
        private const string NavCredit       = "NavTabCredit";
        private const string NavCreditClose  = "NavTabCreditClose";

        private const string TitleWelcome = "TitleWelcome";
        private const string TitleFAQ     = "TitleFAQ";
        private const string TitleCredit  = "TitleCredit";
        private const string TextWelcome  = "TextWelcome";
        private const string TextFAQ      = "TextFAQ";
        private const string TextCredit   = "TextCredit";

        // --- Character ---
        private const string CharDefaultId = "char";
        private const string CharClickId   = "char_click";
        private const string CharIdleId    = "char_idle";

        private const float IdleInterval = 10f;
        private const float IdleDuration = 0.5f;
        private float nextIdleAt;
        private float idleEndAt;

        private enum CharState { Default, Click, Idle }
        private CharState charState = CharState.Default;

        // --- Localization (English fallback) ---
        private static readonly Dictionary<string, string> EnglishFallback = new Dictionary<string, string>
        {
            // Top header labels
            { "Title",    "Drago Daily" },
            { "SubTitle", "Day 1" },
            { "Edition",  "EDITION I" },
            { "Price",    "ONE SILVER" },

            // Tab titles
            { TitleWelcome, "Welcome" },
            { TitleFAQ,     "FAQ" },
            { TitleCredit,  "Credits" },

            // Tab bodies (replace with your real English text)
            { TextWelcome, "Welcome to the mod.\n\nHere you can find an overview of features and updates." },
            { TextFAQ,     "FAQ\n\nQ: ...\nA: ..." },
            { TextCredit,  "Developer：Jinkeloid\n\nDesign&Art：LonelySound" },
        };

        public override Vector2 InitialSize => new Vector2(1205 + Margin * 2, 630 + Margin * 2);
        protected override float InnerPadding => 0;

        public UiPreviewWindow(UiLayout lyt)
        {
            layout = lyt;
            doCloseX = true;
            absorbInputAroundWindow = true;

            ui = new UiLayoutController(layout);

            nextIdleAt = Time.realtimeSinceStartup + IdleInterval;
            idleEndAt  = 0f;

            ApplyLocalizationIfNeeded();
            ApplyTabState();
            ApplyCharState(CharState.Default);
        }

        protected override void DrawOuterBottom(Rect trueRect) { }
        protected override void DrawInner(Rect innerRect) { }
        protected override void DrawOuterTop(Rect trueRect) { }

        public override void DoWindowContents(Rect inRect)
        {
            if (renderer == null)
            {
                renderer = new UiRuntimeRenderer(layout);
                renderer.GetTextBinding    = ui.GetText;
                renderer.GetImageBinding   = ui.GetImage;
                renderer.GetEnabledBinding = ui.GetEnabled;
                renderer.GetVisibleBinding = ui.GetVisible;
                renderer.OnClick           = HandleClick;
            }

            HandleCharacterTiming();

            renderer.RecalculateScale(inRect.width, inRect.height);
            GUI.BeginGroup(inRect);
            renderer.Draw();
            GUI.EndGroup();
        }

        // ---------------- Click logic ----------------

        private void HandleClick(string id)
        {
            Log.Message($"Clicked on {id}");
            if (string.IsNullOrEmpty(id)) return;

            // Tabs (treat both open + close images as tab selectors)
            if (id == NavWelcome || id == NavWelcomeClose) { SwitchTab(Tab.Welcome); return; }
            if (id == NavFAQ     || id == NavFAQClose)     { SwitchTab(Tab.FAQ);     return; }
            if (id == NavCredit  || id == NavCreditClose)  { SwitchTab(Tab.Credit);  return; }

            // Character click
            if (id == CharDefaultId || id == CharClickId || id == CharIdleId)
            {
                ApplyCharState(CharState.Click);
                nextIdleAt = Time.realtimeSinceStartup + IdleInterval;
                idleEndAt  = Time.realtimeSinceStartup + IdleDuration;
                return;
            }
        }

        private void SwitchTab(Tab t)
        {
            if (currentTab == t) return;
            currentTab = t;
            ApplyTabState();
        }

        // ---------------- Tab state ----------------

        private void ApplyTabState()
        {
            // nav visuals
            bool w = currentTab == Tab.Welcome;
            bool f = currentTab == Tab.FAQ;
            bool c = currentTab == Tab.Credit;

            ui.SetVisible(NavWelcomeClose, !w);
            ui.SetVisible(NavFAQClose, !f);
            ui.SetVisible(NavCreditClose, !c);

            ui.SetVisible(TextWelcome,  w);
            ui.SetVisible(TextFAQ,  f);
            ui.SetVisible(TextCredit,  c);
        }

        // ---------------- Character timing ----------------

        private void HandleCharacterTiming()
        {
            float now = Time.realtimeSinceStartup;

            if (now >= nextIdleAt && charState != CharState.Idle)
            {
                ApplyCharState(CharState.Idle);
                idleEndAt = now + IdleDuration;
            }

            if (charState != CharState.Default && now >= idleEndAt)
            {
                ApplyCharState(CharState.Default);
                nextIdleAt = now + IdleInterval;
            }
        }

        private void ApplyCharState(CharState st)
        {
            charState = st;

            ui.SetVisible(CharDefaultId, st == CharState.Default);
            ui.SetVisible(CharClickId,   st == CharState.Click);
            ui.SetVisible(CharIdleId,    st == CharState.Idle);
        }

        // ---------------- Localization ----------------

        private void ApplyLocalizationIfNeeded()
        {
            if (IsChineseLanguage()) return;

            foreach (var kv in EnglishFallback)
            {
                if (ui.Has(kv.Key))
                    ui.SetText(kv.Key, kv.Value);
            }
        }

        private static bool IsChineseLanguage()
        {
            var lang = LanguageDatabase.activeLanguage;
            if (lang == null) return false;

            var folder = (lang.folderName ?? "").ToLowerInvariant();
            if (folder.Contains("chinese")) return true;

            // extra safety for custom packs
            var name = (lang.FriendlyNameNative ?? "").ToLowerInvariant();
            return name.Contains("中文") || name.Contains("chinese");
        }

        // ---------------- Open ----------------

        public static bool TryOpenFromJsonAsset()
        {
            if (!UiLayoutJsonLoader.TryLoadFromBundles("WelcomeWnd", out var l))
            {
                Log.Warning("[RWUI] JSON WelcomeWnd not found.");
                return false;
            }

            Find.WindowStack.Add(new UiPreviewWindow(l));
            return true;
        }
    }
}
