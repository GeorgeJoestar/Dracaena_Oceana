// RwUi.Shared/UiLayoutJson.cs
using System;
using UnityEngine;

namespace RwUi.Shared
{
    [Serializable]
    public class UiLayoutJsonRoot
    {
        public int referenceWidth = 1280;
        public int referenceHeight = 720;
        public string skin;
        public UiLayoutJsonElement[] elements;
    }

    [Serializable]
    public struct RectDTO
    {
        public float x, y, width, height;
        public Rect ToRect() => new Rect(x, y, width, height);
        public static RectDTO FromRect(Rect r) => new RectDTO
            { x = r.x, y = r.y, width = r.width, height = r.height };
    }

    [Serializable]
    public struct ColorDTO
    {
        public float r, g, b, a;
        public Color ToColor() => new Color(r, g, b, a);
        public static ColorDTO FromColor(Color c) => new ColorDTO
            { r = c.r, g = c.g, b = c.b, a = c.a };
    }

    [Serializable]
    public class UiLayoutJsonElement
    {
        public string id;
        public string type;
        public RectDTO rect;
        public string text;
        public string image;
        public string styleName;

        public bool clickable;
        public string bindingKey;

        public string font;
        public int fontSize;
        public bool enabled;

        // NEW: align + color
        public string textAlign;         // "Left","Center","Right"
        public bool overrideTextColor;
        public ColorDTO textColor;
    }
}