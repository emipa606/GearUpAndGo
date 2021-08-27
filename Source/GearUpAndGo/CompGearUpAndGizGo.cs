using System.Collections.Generic;
using Verse;

namespace GearUpAndGo
{
    public class CompGearUpAndGizGo : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent is not Pawn gizmoPawn)
            {
                yield break;
            }

            var component = Current.Game.GetComponent<GearUpPolicyComp>();
            yield return new Command_GearUpAndGo
            {
                icon = component.lastPolicy != "" ? TexGearUpAndGo.guagIconActive : TexGearUpAndGo.guagIcon
            };
        }
    }
}