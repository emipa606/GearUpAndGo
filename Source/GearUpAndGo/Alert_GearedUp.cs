using RimWorld;
using UnityEngine;
using Verse;

namespace GearUpAndGo
{
    public class Alert_GearedUp : Alert
    {
        private const float Padding = 6f;

        public Alert_GearedUp()
        {
            defaultExplanation = "TD.GearUpPolicySetAlert".Translate();
        }

        public override AlertReport GetReport()
        {
            return Current.Game.GetComponent<GearUpPolicyComp>().IsOn();
        }

        public override Rect DrawAt(float topY, bool minimized)
        {
            //float height = TexGearUpAndGo.guagIconActive.height;	//The read out really doesn't handle custom heights :/
            var height = Height;
            var rect = new Rect(UI.screenWidth - Padding - height, topY, height, height);
            GUI.color = Color.white;
            GUI.DrawTexture(rect, TexGearUpAndGo.guagIconActive);
            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }

            if (Widgets.ButtonInvisible(rect, false))
            {
                Current.Game.GetComponent<GearUpPolicyComp>().Revert();
            }

            return rect;
        }
    }
}