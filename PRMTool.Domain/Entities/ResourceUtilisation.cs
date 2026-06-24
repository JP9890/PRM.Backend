using System;

namespace PRMTool.Domain.Entities
{
    /// <summary>
    /// Scheduler-computed snapshot of each resource's current utilisation.
    /// One live row per resource (UNIQUE on ResourceId).
    /// </summary>
    public class ResourceUtilisation
    {
        public int Id { get; private set; }

        /// <summary>UNIQUE — one live row per resource.</summary>
        public int ResourceId { get; private set; }
        public ResourceProfile? Resource { get; private set; }

        /// <summary>FK → AllocationStatus (BENCH | ALLOCATED | OVER_ALLOCATED)</summary>
        public int AllocationStatusId { get; private set; }
        public AllocationStatus? AllocationStatus { get; private set; }

        public int CurrentUtilisationPct { get; private set; }
        public DateTime ComputedAt { get; private set; }

        protected ResourceUtilisation() { }

        public ResourceUtilisation(int resourceId, int allocationStatusId, int currentUtilisationPct)
        {
            ResourceId = resourceId;
            AllocationStatusId = allocationStatusId;
            CurrentUtilisationPct = currentUtilisationPct;
            ComputedAt = DateTime.UtcNow;
        }

        public void Update(int allocationStatusId, int currentUtilisationPct)
        {
            AllocationStatusId = allocationStatusId;
            CurrentUtilisationPct = currentUtilisationPct;
            ComputedAt = DateTime.UtcNow;
        }
    }
}
