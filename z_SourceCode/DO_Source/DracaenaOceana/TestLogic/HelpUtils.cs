using System;
using UnityEngine;
using Verse;

namespace RimWorldProj.TestLogic;

public class HelpUtils
{
    public static Texture2D LoadTexture(string path)
    {
        return ContentFinder<Texture2D>.Get(path, true);
    }
    
    public static void DrawButton(Rect rect, Texture2D texture, Action onClick)
    {
        if (Widgets.ButtonImage(rect, texture))
        {
            onClick?.Invoke();
            
        }
    }
}