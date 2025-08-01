using UnityEngine;
using Verse;

namespace RimWorldProj.TestLogic
{
    public abstract class DefaultWindow : Window
    {
        protected virtual float InnerPadding => 150;

        public DefaultWindow()
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = false;
            forcePause = false;
            doWindowBackground = false;
            drawShadow = false;
        }

        protected virtual Rect GetInnerRect(Rect trueRect)
        {
            return new Rect(
                InnerPadding, 
                InnerPadding, 
                trueRect.width - 2 * InnerPadding, 
                trueRect.height - 2 * InnerPadding);
        }
        protected abstract void DrawOuterBottom(Rect trueRect);
        protected abstract void DrawInner(Rect innerRect);
        protected abstract void DrawOuterTop(Rect trueRect);

        public override void DoWindowContents(Rect inRect)
        {
            Rect trueRect = windowRect.AtZero();
            DrawOuterBottom(trueRect);
            Rect innerRect = GetInnerRect(trueRect);
            GUI.BeginGroup(innerRect);
            DrawInner(new Rect(0, 0, innerRect.width, innerRect.height));
            GUI.EndGroup();
            DrawOuterTop(trueRect);
        }
    }
}
