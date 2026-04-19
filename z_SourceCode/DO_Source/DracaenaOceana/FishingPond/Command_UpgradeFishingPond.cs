using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldProj.FishingPond
{
    public class Command_UpgradeFishingPond : Command_Action
    {
        private const float CornerIconScale = 0.34f;
        private const float CornerIconInset = 4f;

        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
        {
            base.DrawIcon(rect, buttonMat, parms);

            Rect overlayRect = new Rect(
                rect.xMax - rect.width * CornerIconScale - CornerIconInset,
                rect.y + CornerIconInset,
                rect.width * CornerIconScale,
                rect.height * CornerIconScale);

            Color oldColor = GUI.color;
            Color overlayColor = Color.white;
            if (parms.lowLight)
            {
                overlayColor = overlayColor.ToTransparent(0.6f);
            }
            else if (disabled)
            {
                overlayColor = overlayColor.ToTransparent(0.85f);
            }

            GUI.color = overlayColor;
            GUI.DrawTexture(overlayRect, TexCommand.Install);
            GUI.color = oldColor;
        }
    }
}
