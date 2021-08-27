using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace GearUpAndGo
{
    public class Command_GearUpAndGo : Command
    {
        public Command_GearUpAndGo()
        {
            defaultLabel = "TD.GearAndGo".Translate();
            defaultDesc = "TD.GearAndGoDesc".Translate();
            alsoClickIfOtherInGroupClicked = false;
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                var list = SetBetterPawnControl.PolicyList();
                if (list == null)
                {
                    yield break;
                }

                foreach (var policy in list)
                {
                    yield return new FloatMenuOption(policy, () => Target(policy));
                }
            }
        }

        public static void Target(string policy = null)
        {
            Find.Targeter.BeginTargeting(new TargetingParameters { canTargetLocations = true },
                target => Go(target, policy));
        }

        public static void Go(LocalTargetInfo target, string policy)
        {
            Log.Message($"GearUpAndGo to {target}, setting {policy}");

            if (!Event.current.alt)
            {
                Current.Game.GetComponent<GearUpPolicyComp>().Set(policy);
            }

            foreach (var p in Find.Selector.SelectedObjects
                .Where(o => o is Pawn { IsColonistPlayerControlled: true }).Cast<Pawn>())
            {
                p.jobs.TryTakeOrderedJob(new Job(GearUpAndGoJobDefOf.GearUpAndGo, target), JobTag.DraftedOrder);
            }
        }

        public static void End()
        {
            Current.Game.GetComponent<GearUpPolicyComp>().Revert();
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            if (ev.shift && Current.Game.GetComponent<GearUpPolicyComp>().IsOn())
            {
                End();
            }
            else
            {
                Target();
            }
        }
    }

    //Backwardcompat
}