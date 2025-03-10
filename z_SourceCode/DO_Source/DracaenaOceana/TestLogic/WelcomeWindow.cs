using UnityEngine;
using Verse;

namespace RimWorldProj.WelcomeScreen
{
    public class WelcomeWindow : Window
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
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = false;
            doWindowBackground = false;
            drawShadow = false;

            // Load the title and character images.
            titleImage = ContentFinder<Texture2D>.Get("UI/WndWelcome/title", true);
            characterImage = ContentFinder<Texture2D>.Get("UI/WndWelcome/char", true);
            background = ContentFinder<Texture2D>.Get("UI/WndWelcome/windowbg", true);
            stamp = ContentFinder<Texture2D>.Get("UI/WndWelcome/welcome", true);

            // Initialize the content and button image arrays.
            contentImages = new Texture2D[4];
            buttonImages = new Texture2D[4];

            for (int i = 0; i < 4; i++)
            {
                // Assumes image names like "ContentImage1", "ContentImage2", etc.
                contentImages[i] = ContentFinder<Texture2D>.Get($"UI/WndWelcome/faq{i + 1}", true);
                buttonImages[i] = ContentFinder<Texture2D>.Get($"UI/WndWelcome/flipnote{i + 1}", true);
            }
        }

        public override Vector2 InitialSize => new Vector2(1205, 630);

        public override void DoWindowContents(Rect inRect)
        {
                        
            Rect actualGroupRect = new Rect(150, 0, inRect.width, inRect.height);
            GUI.BeginGroup(actualGroupRect);
            GUI.DrawTexture(inRect, background, ScaleMode.StretchToFill);
            Rect titleRect = new Rect((inRect.width - titleImage.width) / 2, 10, titleImage.width, titleImage.height); // Adjust size as needed.
            GUI.DrawTexture(titleRect, titleImage, ScaleMode.StretchToFill);

            Rect stampRect = new Rect(inRect.width - stamp.width - 200, 300, stamp.width, stamp.height); // Adjust size and position as needed.
            GUI.DrawTexture(stampRect, stamp, ScaleMode.StretchToFill);
            
            Rect contentRect = new Rect(inRect.width - contentImages[currentIndex].width, titleImage.height, contentImages[currentIndex].width, contentImages[currentIndex].height);
            GUI.DrawTexture(contentRect, contentImages[currentIndex], ScaleMode.StretchToFill);
            GUI.EndGroup();
            
            Rect characterRect = new Rect(0, 0, characterImage.width, characterImage.height); // Adjust size and position as needed.
            GUI.DrawTexture(characterRect, characterImage, ScaleMode.StretchToFill);
            
            Rect buttonRect = new Rect(inRect.width - buttonImages[currentIndex].width, 10, buttonImages[currentIndex].width, buttonImages[currentIndex].height); // Adjust size as needed.
            if (Widgets.ButtonImage(buttonRect, buttonImages[currentIndex]))
            {
                // Cycle to the next image. Loops back to image 1 after image 4.
                currentIndex = (currentIndex + 1) % 4;
            }
        }
    }
}
