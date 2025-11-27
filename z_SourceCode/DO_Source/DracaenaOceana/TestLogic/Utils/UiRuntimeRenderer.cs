// RwUi.Shared/UiRuntimeRenderer.cs
using System.Collections.Generic;
using UnityEngine;

namespace RwUi.Shared
{
    public sealed class UiRuntimeRenderer
    {
        public UiLayout Layout { get; }
        public System.Action<string> OnClick;

        public System.Func<string, string> GetTextBinding;
        public System.Func<string, Texture2D> GetImageBinding;
        public System.Func<string, bool> GetEnabledBinding;
        public System.Func<string, bool> GetVisibleBinding;

        float sx = 1f, sy = 1f;

        static GUIStyle _invisibleButton;
        static GUIStyle InvisibleButton
        {
            get
            {
                if (_invisibleButton == null)
                    _invisibleButton = new GUIStyle(GUIStyle.none);
                return _invisibleButton;
            }
        }
        
        static string NormalizeText(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s
                .Replace("\\r\\n", "\n")
                .Replace("\\n", "\n")
                .Replace("\\r", "\n");
        }

        readonly Dictionary<int, GUIStyle> styleCache = new Dictionary<int, GUIStyle>();

        public UiRuntimeRenderer(UiLayout layout) { Layout = layout; }

        public void RecalculateScale(float width, float height)
        {
            if (Layout.referenceResolution.x <= 0 || Layout.referenceResolution.y <= 0)
            { sx = sy = 1f; return; }

            sx = width  / Layout.referenceResolution.x;
            sy = height / Layout.referenceResolution.y;
        }

        Rect S(Rect r) => new Rect(r.x * sx, r.y * sy, r.width * sx, r.height * sy);

        string Key(UiLayout.Element e) =>
            string.IsNullOrEmpty(e.bindingKey) ? e.id : e.bindingKey;

        bool InvisibleClick(Rect r) =>
            GUI.Button(r, GUIContent.none, InvisibleButton);

        GUIStyle ResolveStyle(UiLayout.Element e, GUIStyle baseStyle)
        {
            bool needFont = e.font != null || e.fontSize > 0;
            bool needColor = e.overrideTextColor;

            if (!needFont && !needColor) return baseStyle;

            int hash = baseStyle.GetHashCode();
            hash = hash * 31 + (e.font ? e.font.GetInstanceID() : 0);
            hash = hash * 31 + e.fontSize;
            hash = hash * 31 + (needColor ? 1 : 0);
            if (needColor) hash = hash * 31 + e.textColor.GetHashCode();

            if (styleCache.TryGetValue(hash, out var cached)) return cached;

            var st = new GUIStyle(baseStyle);

            if (e.font) st.font = e.font;
            if (e.fontSize > 0) st.fontSize = e.fontSize;

            if (e.overrideTextColor)
            {
                st.normal.textColor   = e.textColor;
                st.active.textColor   = e.textColor;
                st.focused.textColor  = e.textColor;
                st.hover.textColor    = e.textColor;
            }
            styleCache[hash] = st;
            return st;
        }

        GUIStyle ApplyTextAlign(GUIStyle baseStyle, UiLayout.Element el)
        {
            var style = new GUIStyle(baseStyle);

            switch (el.textAlign)
            {
                case UiLayout.TextAlign.Center:
                    style.alignment = TextAnchor.UpperCenter;
                    break;
                case UiLayout.TextAlign.Right:
                    style.alignment = TextAnchor.UpperRight;
                    break;
                default:
                    style.alignment = TextAnchor.UpperLeft;
                    break;
            }

            return style;
        }
        
        public void Draw()
        {
            if (Layout.defaultSkin) GUI.skin = Layout.defaultSkin;

            foreach (var e in Layout.elements)
            {
                var r = S(e.rect);

                GUIStyle st = null;
                if (!string.IsNullOrEmpty(e.styleName) && GUI.skin != null)
                    st = GUI.skin.FindStyle(e.styleName);

                var key = Key(e);
                
                if (GetVisibleBinding != null && !GetVisibleBinding(key))
                    continue;

                bool enabled = e.enabled;
                if (GetEnabledBinding != null)
                    enabled = GetEnabledBinding(key);

                var oldEnabled = GUI.enabled;
                GUI.enabled = enabled;

                switch (e.type)
                {
                    case UiLayout.ElementType.Label:
                    {
                        var baseSt = st ?? GUI.skin.label;
                        var drawSt = ResolveStyle(e, baseSt);
                        var style = ApplyTextAlign(drawSt, e);
                        style.wordWrap = true;

                        var txt = GetTextBinding != null ? GetTextBinding(key) : (e.text ?? "");
                        txt = NormalizeText(txt);
                        GUI.Label(r, txt, style);

                        if (enabled && e.clickable && InvisibleClick(r))
                            OnClick?.Invoke(key);
                        break;
                    }

                    case UiLayout.ElementType.Button:
                    {
                        var baseSt = st ?? GUI.skin.button;
                        var drawSt = ResolveStyle(e, baseSt);
                        var style = ApplyTextAlign(drawSt, e);
                        style.wordWrap = true;

                        var txt = GetTextBinding != null ? GetTextBinding(key) : (e.text ?? "");
                        txt = NormalizeText(txt);
                        if (GUI.Button(r, txt, style))
                            OnClick?.Invoke(key);
                        break;
                    }

                    case UiLayout.ElementType.Image:
                    {
                        var tex = GetImageBinding?.Invoke(key) ?? e.image;
                        if (tex) GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true);

                        if (enabled && e.clickable && InvisibleClick(r))
                            OnClick?.Invoke(key);
                        break;
                    }

                    case UiLayout.ElementType.TextField:
                    {
                        var baseSt = st ?? GUI.skin.textField;
                        var drawSt = ResolveStyle(e, baseSt);
                        var style = ApplyTextAlign(drawSt, e);

                        var cur = GetTextBinding != null ? GetTextBinding(key) : (e.text ?? "");
                        GUI.TextField(r, cur, style);
                        break;
                    }
                }

                GUI.enabled = oldEnabled;
            }
        }
    }
}
