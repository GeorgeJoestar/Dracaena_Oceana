using UnityEngine;
using Verse;
using RimWorldProj.TestLogic;

namespace RimWorldProj.WelcomeScreen
{
    public class WelcomeWindow : DefaultWindow
    {

        private Texture2D titleImage;
        private Texture2D characterImage;
        private Texture2D background;
        private Texture2D stamp;
        private Texture2D[] contentImages;
        private Texture2D[] buttonImages;
        private int currentIndex = 0;

        public WelcomeWindow()
        {
            titleImage = ContentFinder<Texture2D>.Get("UI/WndWelcome/title", true);
            characterImage = ContentFinder<Texture2D>.Get("UI/WndWelcome/char", true);
            background = ContentFinder<Texture2D>.Get("UI/WndWelcome/windowbg", true);
            stamp = ContentFinder<Texture2D>.Get("UI/WndWelcome/welcome", true);

            contentImages = new Texture2D[4];
            buttonImages = new Texture2D[4];
            for (int i = 0; i < 4; i++)
            {
                contentImages[i] = ContentFinder<Texture2D>.Get($"UI/WndWelcome/faq{i + 1}", true);
                buttonImages[i] = ContentFinder<Texture2D>.Get($"UI/WndWelcome/flipnote{i + 1}", true);
            }
        }

        public override Vector2 InitialSize => new Vector2(1205, 630);
        protected override float InnerPadding => 100;
        
        protected override void DrawInner(Rect innerRect)
        {
            GUI.DrawTexture(new Rect(0, 0, innerRect.width, innerRect.height), background, ScaleMode.StretchToFill);
            Rect titleRect = new Rect((innerRect.width - titleImage.width) / 2, 10, titleImage.width, titleImage.height);
            GUI.DrawTexture(titleRect, titleImage, ScaleMode.StretchToFill);
            Rect stampRect = new Rect(innerRect.width - stamp.width - 200, 300, stamp.width, stamp.height);
            GUI.DrawTexture(stampRect, stamp, ScaleMode.StretchToFill);
            Rect contentRect = new Rect(innerRect.width - contentImages[currentIndex].width, titleImage.height, contentImages[currentIndex].width, contentImages[currentIndex].height);
            GUI.DrawTexture(contentRect, contentImages[currentIndex], ScaleMode.StretchToFill);
            Rect buttonRect = new Rect(innerRect.width - buttonImages[currentIndex].width, 10, buttonImages[currentIndex].width, buttonImages[currentIndex].height);
            if (Widgets.ButtonImage(buttonRect, buttonImages[currentIndex]))
            {
                currentIndex = (currentIndex + 1) % 4;
            }
        }

        protected override void DrawOuterTop(Rect trueRect)
        {
            Rect characterRect = new Rect(0, 100, characterImage.width, characterImage.height + 100);
            GUI.DrawTexture(characterRect, characterImage, ScaleMode.StretchToFill);
        }

        protected override void DrawOuterBottom(Rect trueRect)
        {
        }
    }
}
