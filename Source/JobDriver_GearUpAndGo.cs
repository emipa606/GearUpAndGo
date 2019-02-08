﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;

namespace GearUpAndGo
{
	public class JobDriver_GearUpAndGo : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		public static Type CEloadoutGiverType = AccessTools.TypeByName("JobGiver_UpdateLoadout");
		public static MethodInfo CEloadoutGetter;
		public static MethodInfo TryIssueJobPackageInfo = AccessTools.Method(typeof(ThinkNode), nameof(ThinkNode.TryIssueJobPackage));
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn pawn = toil.actor;
				if (pawn.thinker == null) return;
				
				JobGiver_OptimizeApparel optimizer = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_OptimizeApparel>();
				if (optimizer == null) return;

				pawn.mindState?.Notify_OutfitChanged();// Lie so that it re-equips things
				ThinkResult result = optimizer.TryIssueJobPackage(pawn, new JobIssueParams()); //TryGiveJob is protected :(
				if (result == ThinkResult.NoJob)
				{
					if (CEloadoutGiverType != null)
					{
						if (CEloadoutGetter == null)
							CEloadoutGetter = AccessTools.Method(typeof(Pawn_Thinker), nameof(Pawn_Thinker.TryGetMainTreeThinkNode)).MakeGenericMethod(new Type[] { CEloadoutGiverType });
						if (CEloadoutGetter != null)
						{
							object CELoadoutGiver = CEloadoutGetter.Invoke(pawn.thinker, new object[] { });
							if (CELoadoutGiver != null) 
								result = (ThinkResult)TryIssueJobPackageInfo.Invoke(CELoadoutGiver, new object[] { pawn, new JobIssueParams() });
						}
					}
				}
				if (result == ThinkResult.NoJob)
				{
					IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(TargetA.Cell, pawn);
					Job job = new Job(JobDefOf.Goto, intVec);
					if (pawn.Map.exitMapGrid.IsExitCell(UI.MouseCell()))
					{
						job.exitMapOnArrival = true; // I guess
					}

					//preserve the queue
					List<QueuedJob> queue = pawn.Drafted ? new List<QueuedJob>() : new List<QueuedJob>(pawn.jobs.jobQueue);

					pawn.drafter.Drafted = true;
					pawn.jobs.StartJob(job, JobCondition.Succeeded);
					foreach (QueuedJob qj in queue)
					{
						pawn.jobs.jobQueue.EnqueueLast(qj.job, qj.tag);
					}

					MoteMaker.MakeStaticMote(intVec, pawn.Map, ThingDefOf.Mote_FeedbackGoto, 1f);
				}
				else
				{
					Job optJob = result.Job;
					Log.Message($"{pawn} JobDriver_GearUpAndGo job {optJob}");
					if (optJob.def == JobDefOf.Wear)
						pawn.Reserve(optJob.targetA, optJob);
					pawn.jobs.jobQueue.EnqueueFirst(new Job(GearUpAndGoJobDefOf.GearUpAndGo, TargetA));
					pawn.jobs.jobQueue.EnqueueFirst(optJob);
				}
			};
			yield return toil;
		}
	}
}
