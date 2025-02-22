using UnityEngine;
using Verse;

namespace RimWorldProj.TestLogic
{
    public class TestWindow
    {
        public class HelloWorldWindow : Window
        {
            private Texture2D emptyImage;

            public HelloWorldWindow()
            {
                doCloseButton = true;
                doCloseX = true;
                closeOnClickedOutside = true;
                absorbInputAroundWindow = true;
                forcePause = false;

                emptyImage = new Texture2D(100, 100);
                Color[] pixels = new Color[100 * 100];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = Color.white;
                emptyImage.SetPixels(pixels);
                emptyImage.Apply();
            }

            public override Vector2 InitialSize => new Vector2(400, 300);

            public override void DoWindowContents(Rect inRect)
            {
                Rect imageRect = new Rect(inRect.x, inRect.y, 100, 100);
                GUI.DrawTexture(imageRect, emptyImage);
                Rect textRect = new Rect(inRect.x, inRect.y + 110, inRect.width, 30);
                Widgets.Label(textRect, "Hello, RimWorld!");
            }
        }
    }
}