using UnityEngine;
using Verse;
using RimWorldProj.TestLogic;
using RimWorldProj.TestLogic.Utils;

namespace RimWorldProj.WelcomeScreen
{
    public class WelcomeWindow : DefaultWindow
    {

        private Texture2D titleImage;
        
        private Texture2D characterDefault;
        private Texture2D characterIdle;
        private Texture2D characterClick;
        
        private const float IdleInterval = 10f;
        private const float IdleDuration = 0.5f;
        private float nextIdleAt;
        private float idleEndAt;
        private Texture2D currentChar;
        
        private readonly GUIStyle labelStyle;
        
        private Texture2D background;
        private Texture2D stamp;

        public WelcomeWindow()
        {
            titleImage = ContentFinder<Texture2D>.Get("UI/WndWelcome/title", true);
            
            background = ContentFinder<Texture2D>.Get("UI/WndWelcome/paper1", true);
            stamp = ContentFinder<Texture2D>.Get("UI/WndWelcome/welcome", true);

            
            characterDefault= ContentFinder<Texture2D>.Get("UI/WndWelcome/char",  true);
            characterIdle   = ContentFinder<Texture2D>.Get("UI/WndWelcome/char_idle",  true);
            characterClick  = ContentFinder<Texture2D>.Get("UI/WndWelcome/char_click", true);
            currentChar = characterDefault;
            nextIdleAt  = Time.realtimeSinceStartup + IdleInterval;
            
            // Font customFont = ContentFinder<Font>.Get("Fonts/Huiwen-mincho.otf", true);
            Font customFont = null;
            if (FontLoader.TryGet("Huiwen-mincho", out var f)) customFont = f;
            Log.Message(characterClick.name);
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                font      = customFont,
                fontSize  = 28,
                richText  = true,
                alignment = TextAnchor.UpperLeft,
                normal    = { textColor = Color.black }
            };
        }

        public override Vector2 InitialSize => new Vector2(1410, 910);
        protected override float InnerPadding => 205;
        
        protected override void DrawInner(Rect innerRect)
        {
            GUI.DrawTexture(new Rect(0, 0, innerRect.width, innerRect.height), background, ScaleMode.StretchToFill);
            GUI.Label(new Rect(innerRect.width/2+30, 160, 800, 80),
                "<b>海龙日报 <color=#6f5a3c>第一版</color></b>",
                labelStyle);

            GUI.Label(new Rect(innerRect.width/2+30, 220, 800, 60),
                "<size=20>Version 1.0</size>",
                labelStyle);
        }

        protected override void DrawOuterTop(Rect trueRect)
        {
            HandleCharacterTiming();

            var charRect = new Rect(0, 100, currentChar.width, currentChar.height);
            GUI.DrawTexture(charRect, currentChar, ScaleMode.StretchToFill);

            if (Widgets.ButtonInvisible(charRect))
            {
                currentChar = characterClick;     // switch to click sprite
                nextIdleAt  = Time.realtimeSinceStartup + IdleInterval; // postpone next idle blink
                idleEndAt   = Time.realtimeSinceStartup + IdleDuration; // set idle end time
            }
        }

        protected override void DrawOuterBottom(Rect trueRect)
        {
        }
        
        private void HandleCharacterTiming()
        {
            float now = Time.realtimeSinceStartup;

            if (now >= nextIdleAt && currentChar != characterIdle)
            {
                currentChar = characterIdle;
                idleEndAt   = now + IdleDuration;
            }

            if (currentChar != characterDefault && now >= idleEndAt)
            {
                currentChar = characterDefault;
                nextIdleAt  = now + IdleInterval;
            }
        }
    }
}
