using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace GearUpAndGo
{
    public class JobDriver_GearUpAndGo : JobDriver
    {
        //Combat Extended Support
        public static Type CEloadoutGiverType = AccessTools.TypeByName("JobGiver_UpdateLoadout");
        public static MethodInfo CEloadoutGetter;

        public static MethodInfo TryIssueJobPackageInfo =
            AccessTools.Method(typeof(ThinkNode), nameof(ThinkNode.TryIssueJobPackage));

        //Weapon of Choice support
        public static Type WOCGiverType = AccessTools.TypeByName("JobGiver_OptimizeEquipment");
        public static MethodInfo WOCGetter;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var toilActor = toil.actor;
                if (toilActor.thinker == null)
                {
                    return;
                }

                //Find apparel
                var optimizer = toilActor.thinker.TryGetMainTreeThinkNode<JobGiver_OptimizeApparel>();
                if (optimizer == null)
                {
                    return;
                }

                toilActor.mindState?.Notify_OutfitChanged(); // Lie so that it re-equips things
                var result = optimizer.TryIssueJobPackage(toilActor, new JobIssueParams()); //TryGiveJob is protected :(

                //Find loadout, Combat Extended
                if (result == ThinkResult.NoJob)
                {
                    if (CEloadoutGiverType != null)
                    {
                        if (CEloadoutGetter == null)
                        {
                            CEloadoutGetter = AccessTools
                                .Method(typeof(Pawn_Thinker), nameof(Pawn_Thinker.TryGetMainTreeThinkNode))
                                .MakeGenericMethod(CEloadoutGiverType);
                        }

                        if (CEloadoutGetter != null)
                        {
                            var CELoadoutGiver = CEloadoutGetter.Invoke(toilActor.thinker, new object[] { });
                            if (CELoadoutGiver != null)
                            {
                                result = (ThinkResult)TryIssueJobPackageInfo.Invoke(CELoadoutGiver,
                                    new object[] { toilActor, new JobIssueParams() });
                            }
                        }
                    }
                }

                //Find weapons, Weapons of Choice
                if (result == ThinkResult.NoJob)
                {
                    if (WOCGiverType != null)
                    {
                        if (WOCGetter == null)
                        {
                            WOCGetter = AccessTools
                                .Method(typeof(Pawn_Thinker), nameof(Pawn_Thinker.TryGetMainTreeThinkNode))
                                .MakeGenericMethod(WOCGiverType);
                        }

                        if (WOCGetter != null)
                        {
                            var WOCLoadoutGiver = WOCGetter.Invoke(toilActor.thinker, new object[] { });
                            if (WOCLoadoutGiver != null)
                            {
                                result = (ThinkResult)TryIssueJobPackageInfo.Invoke(WOCLoadoutGiver,
                                    new object[] { toilActor, new JobIssueParams() });
                            }
                        }
                    }
                }

                //Okay, nothing to do, go to target
                if (result == ThinkResult.NoJob)
                {
                    var intVec = RCellFinder.BestOrderedGotoDestNear(TargetA.Cell, toilActor);
                    var newJob = new Job(JobDefOf.Goto, intVec);
                    if (toilActor.Map.exitMapGrid.IsExitCell(UI.MouseCell()))
                    {
                        newJob.exitMapOnArrival = true; // I guess
                    }

                    if (!toilActor.Drafted)
                    {
                        //Drafting clears the job queue. We want to keep the queue.
                        //It'll also return jobs to the pool, and clear each job too.
                        //So extract each job and clear the queue manually, then re-queue them all.

                        var queue = new List<QueuedJob>();
                        while (toilActor.jobs.jobQueue.Count > 0)
                        {
                            queue.Add(toilActor.jobs.jobQueue.Dequeue());
                        }

                        toilActor.drafter.Drafted = true;

                        toilActor.jobs.StartJob(newJob, JobCondition.Succeeded);

                        foreach (var qj in queue)
                        {
                            toilActor.jobs.jobQueue.EnqueueLast(qj.job, qj.tag);
                        }
                    }
                    else
                    {
                        toilActor.jobs.StartJob(newJob, JobCondition.Succeeded);
                    }

                    FleckMaker.Static(intVec, toilActor.Map, FleckDefOf.FeedbackGoto);
                }
                //Queue up the Gear job, then do another Gear+Go job
                else
                {
                    var optJob = result.Job;
                    Log.Message($"{toilActor} JobDriver_GearUpAndGo job {optJob}");
                    if (optJob.def == JobDefOf.Wear)
                    {
                        toilActor.Reserve(optJob.targetA, optJob);
                    }

                    toilActor.jobs.jobQueue.EnqueueFirst(new Job(GearUpAndGoJobDefOf.GearUpAndGo, TargetA));
                    toilActor.jobs.jobQueue.EnqueueFirst(optJob);
                }
            };
            yield return toil;
        }
    }
}