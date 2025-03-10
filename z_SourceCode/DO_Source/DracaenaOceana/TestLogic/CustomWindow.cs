using UnityEngine;
using Verse;

namespace RimWorldProj.TestLogic
{
    public class CustomWindow : Window
    {
        private Texture2D customBackground;
        private Texture2D customButton;
        private Texture2D characterImage;
        
        public CustomWindow()
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = false;
            doWindowBackground = false;
            drawShadow = false;
            
            // Load textures from your mod's Textures folder.
            customBackground = ContentFinder<Texture2D>.Get("Testwindow/CustomBackground", true);
            customButton = ContentFinder<Texture2D>.Get("Testwindow/CustomButton", true);
            characterImage = ContentFinder<Texture2D>.Get("Testwindow/Character", true);

            if (customBackground == null) Log.Error("CustomBackground texture not found!");
            if (customButton == null) Log.Error("customButton texture not found!");
            if (characterImage == null) Log.Error("Character texture not found!");
            
        }
        
        public override Vector2 InitialSize => new Vector2(600, 400);
        
        public override void DoWindowContents(Rect inRect)
        {
            doWindowBackground = false;
            // Draw the custom background.
            
            Rect actualGroupRect = new Rect(70, 0, inRect.width, inRect.height);
            GUI.BeginGroup(actualGroupRect);
            GUI.DrawTexture(inRect, customBackground, ScaleMode.StretchToFill);
            GUI.EndGroup();
            
            Rect extendedGroupRect = new Rect(0, 0, inRect.width, inRect.height);
            GUI.BeginGroup(extendedGroupRect);
            Rect oversizedRect = new Rect(0, 0, 400, 400);
            GUI.DrawTexture(oversizedRect, characterImage, ScaleMode.StretchToFill);
            GUI.EndGroup();
            
            
            // Draw a custom button.
            Rect buttonRect = new Rect(inRect.width/2, inRect.height - 50, 100, 40);
            if (Widgets.ButtonImage(buttonRect, customButton))
            {
                Log.Message("Custom button clicked!");
            }
            
            // Draw label text.
            // Rect labelRect = new Rect(inRect.x + 20, inRect.y + 70, inRect.width - 40, 30);
            // Widgets.Label(labelRect, "Custom window with an oversized image on the left.");
        }
    }
}