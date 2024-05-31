using System.Collections.Generic;
using Verse.AI;

namespace DoorsExpanded
{
    public class JobDriver_UseRemoteButton : JobDriver
    {
        private Building_DoorRemoteButton Remote => (Building_DoorRemoteButton)TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => Remote.IsDisabledForJob(job.playerForced, out var _));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            // Note: 15 tick delay copied from JobDriver_Flick.
            yield return Toils_General.Wait(15).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.Do(() =>
            {
                var remote = Remote;
                if (!remote.IsDisabledForJob(job.playerForced, out var _))
                    remote.PushButton();
            });
        }
    }
}
