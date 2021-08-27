using Verse;

namespace GearUpAndGo
{
    public class GearUpPolicyComp : GameComponent
    {
        public string lastPolicy = "";

        public GearUpPolicyComp(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref lastPolicy, "lastPolicy", "");
        }

        public void Set(string policy)
        {
            if (lastPolicy == "")
            {
                lastPolicy = SetBetterPawnControl.CurrentPolicy();
            }

            SetBetterPawnControl.SetPawnControlPolicy(policy ?? Settings.Get().betterPawnControlBattlePolicy);
        }

        public void Revert()
        {
            if (lastPolicy == "")
            {
                return;
            }

            SetBetterPawnControl.SetPawnControlPolicy(lastPolicy);

            lastPolicy = "";
        }

        public bool IsOn()
        {
            return lastPolicy != "";
        }
    }
}