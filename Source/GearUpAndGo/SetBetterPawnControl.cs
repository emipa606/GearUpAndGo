using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterPawnControl;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GearUpAndGo
{
    [StaticConstructorOnStartup]
    internal static class SetBetterPawnControl
    {
        private static bool warned;


        private static readonly Type assignManager;
        private static readonly Type policyType;
        private static readonly FieldInfo policiesInfo;
        private static readonly MethodInfo LoadStateInfo;

        private static readonly MethodInfo GetActivePolicyInfo =
            AccessTools.Method(AccessTools.TypeByName("AssignManager"), "GetActivePolicy", new Type[] { });

        static SetBetterPawnControl()
        {
            assignManager = AccessTools.TypeByName("BetterPawnControl.AssignManager");
            Log.Message($"BCP assignManager: {assignManager}");
            policyType = AccessTools.TypeByName("BetterPawnControl.Policy");
            Log.Message($"BCP policyType: {policyType}");
            policiesInfo = AccessTools.Field(assignManager, "policies");
            Log.Message($"BCP policiesInfo: {policiesInfo}");
            LoadStateInfo = AccessTools.Method(assignManager, "LoadState", new[] { policyType });
            Log.Message($"BCP LoadStateInfo: {LoadStateInfo}");
        }

        public static bool Active()
        {
            return ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Better Pawn Control");
        }

        //Okay, I want it really bad, so let's do this.
        public static void SetPawnControlPolicy(string policyName)
        {
            if (!Active())
            {
                return;
            }

            try
            {
                SetPawnControlPolicyEx(policyName);
            }
            catch (Exception e)
            {
                if (!warned)
                {
                    var desc =
                        "Gear Up And Go Failed to set Better Pawn Control Policy, I have no idea why, probably needs a re-compile, so check for an update or tell me (sorry this is really confusing.)";
                    Find.LetterStack.ReceiveLetter("Gear+Go failed to set Pawn Control", desc,
                        LetterDefOf.NegativeEvent, e.ToStringSafe());
                    Verse.Log.Error($"{desc}\n\n{e.ToStringSafe()}");
                    warned = true;
                }
            }
        }

        public static void SetPawnControlPolicyEx(string policyName)
        {
            var assignPolicies = (List<Policy>)policiesInfo.GetValue(default);
            Log.Message($"assignPolicies are: {assignPolicies.ToStringSafeEnumerable()}");
            var policy = assignPolicies.FirstOrDefault(p => p.label == policyName);

            if (policy == null)
            {
                return;
            }

            Log.Message($"using policy: {policy}");
            //MainTabWindow_Assign_Policies
            //private static void LoadState(Policy policy)
            LoadStateInfo.Invoke(default, new object[] { policy });
        }

        public static string CurrentPolicy()
        {
            if (!Active())
            {
                return "";
            }

            try
            {
                return CurrentPolicyEx();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string CurrentPolicyEx()
        {
            Log.Message("Resetting policies");

            return (GetActivePolicyInfo.Invoke(null, new object[] { }) as Policy)?.label ?? "";
        }

        public static List<string> PolicyList()
        {
            if (!Active())
            {
                return null;
            }

            try
            {
                return PolicyListEx();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> PolicyListEx()
        {
            //Hitting can't load type errors if I make this an easier yield return, so messy lists it is
            var policyNames = new List<string>();
            var assignPolicies = (List<Policy>)policiesInfo.GetValue(default);
            Log.Message($"assignPolicies are: {assignPolicies.ToStringSafeEnumerable()}");
            foreach (var p in assignPolicies)
            {
                policyNames.Add(p.label);
            }

            return policyNames;
        }
    }
}