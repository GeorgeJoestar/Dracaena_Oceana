using System.Collections.Generic;
using UnityEngine;
using RwUi.Shared;

namespace RimWorldProj.TestLogic.UI
{
    /// <summary>
    /// RimWorld-side helper for addressing UiLayout elements by id.
    /// Uses binding overrides (recommended) while still allowing direct layout mutation if desired.
    /// </summary>
    public sealed class UiLayoutController
    {
        private readonly UiLayout layout;
        private readonly Dictionary<string, int> indexById;

        private readonly Dictionary<string, string> textOverrides;
        private readonly Dictionary<string, Texture2D> imageOverrides;
        private readonly Dictionary<string, bool> enabledOverrides;
        private readonly Dictionary<string, bool> visibleOverrides;

        public UiLayoutController(UiLayout layout)
        {
            this.layout = layout;

            indexById = new Dictionary<string, int>();
            textOverrides = new Dictionary<string, string>();
            imageOverrides = new Dictionary<string, Texture2D>();
            enabledOverrides = new Dictionary<string, bool>();
            visibleOverrides = new Dictionary<string, bool>();

            if (layout?.elements == null) return;

            for (int i = 0; i < layout.elements.Length; i++)
            {
                var id = layout.elements[i].id;
                if (!string.IsNullOrEmpty(id) && !indexById.ContainsKey(id))
                    indexById.Add(id, i);
            }
        }

        public bool Has(string id) => !string.IsNullOrEmpty(id) && indexById.ContainsKey(id);

        public bool TryGetElement(string id, out UiLayout.Element el)
        {
            el = default;
            if (!Has(id) || layout?.elements == null) return false;
            el = layout.elements[indexById[id]];
            return true;
        }

        // ----- Binding getters for UiRuntimeRenderer -----

        public string GetText(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";

            if (textOverrides.TryGetValue(id, out var v))
                return v ?? "";

            return TryGetElement(id, out var el) ? (el.text ?? "") : "";
        }

        public Texture2D GetImage(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (imageOverrides.TryGetValue(id, out var v))
                return v;

            return TryGetElement(id, out var el) ? el.image : null;
        }

        public bool GetEnabled(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;

            if (enabledOverrides.TryGetValue(id, out var v))
                return v;

            return TryGetElement(id, out var el) ? el.enabled : true;
        }

        public bool GetVisible(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;
            if (visibleOverrides.TryGetValue(id, out var v)) return v;
            return true;
        }
        
        // ----- Override setters (preferred) -----

        public void SetText(string id, string value)
        {
            if (!Has(id)) return;
            textOverrides[id] = value ?? "";
        }

        public void ClearText(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            textOverrides.Remove(id);
        }

        public void SetImage(string id, Texture2D tex)
        {
            if (!Has(id)) return;
            imageOverrides[id] = tex;
        }

        public void ClearImage(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            imageOverrides.Remove(id);
        }

        public void SetEnabled(string id, bool enabled)
        {
            if (!Has(id)) return;
            enabledOverrides[id] = enabled;
        }

        public void ClearEnabled(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            enabledOverrides.Remove(id);
        }

        public void SetVisible(string id, bool visible)
        {
            if (Has(id)) visibleOverrides[id] = visible;
        }
        
        public void ClearVisible(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            visibleOverrides.Remove(id);
        }

        // ----- Direct layout mutation (optional) -----

        public bool SetLayoutText(string id, string value)
        {
            if (!Has(id) || layout?.elements == null) return false;

            var arr = layout.elements;
            int i = indexById[id];
            var el = arr[i];
            el.text = value ?? "";
            arr[i] = el;
            layout.elements = arr;
            return true;
        }

        public bool SetLayoutImage(string id, Texture2D tex)
        {
            if (!Has(id) || layout?.elements == null) return false;

            var arr = layout.elements;
            int i = indexById[id];
            var el = arr[i];
            el.image = tex;
            arr[i] = el;
            layout.elements = arr;
            return true;
        }

        public bool SetLayoutEnabled(string id, bool enabled)
        {
            if (!Has(id) || layout?.elements == null) return false;

            var arr = layout.elements;
            int i = indexById[id];
            var el = arr[i];
            el.enabled = enabled;
            arr[i] = el;
            layout.elements = arr;
            return true;
        }
    }
}
