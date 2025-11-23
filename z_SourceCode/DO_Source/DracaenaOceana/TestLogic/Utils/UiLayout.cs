// RwUi.Shared/UiLayout.cs
using UnityEngine;

namespace RwUi.Shared
{
    [CreateAssetMenu(menuName = "RW UI/Layout", fileName = "UiLayout")]
    public class UiLayout : ScriptableObject
    {
        public Vector2Int referenceResolution = new Vector2Int(1280, 720);
        public GUISkin defaultSkin;
        public Element[] elements;

        public enum ElementType { Label, Button, Image, TextField }
        public enum TextAlign { Left, Center, Right }

        [System.Serializable]
        public struct Element
        {
            public string id;
            public ElementType type;
            public Rect rect;
            public string text;
            public Texture2D image;
            public string styleName;

            public bool clickable;
            public string bindingKey;

            public Font font;
            public int fontSize;     // 0 = inherit
            public bool enabled;     // default true

            public TextAlign textAlign;     // default Left
            public bool overrideTextColor;  // if false, use style/skin color
            public Color textColor;         // used only when overrideTextColor = true
        }
    }
}