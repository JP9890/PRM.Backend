using System;

namespace PRMTool.Domain.Entities
{
    public class Milestone
    {
        public int Id { get; private set; }
        public int ProjectId { get; private set; }
        public Project? Project { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public DateTime DueDate { get; private set; }
        public int StoryPoints { get; private set; }

        /// <summary>FK → MilestoneStatus.Id (NOT_STARTED | IN_PROGRESS | DONE)</summary>
        public int MilestoneStatusId { get; private set; }
        public MilestoneStatus? Status { get; private set; }

        public int SortOrder { get; private set; }
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        protected Milestone() { }

        public Milestone(int projectId, string title, DateTime dueDate, int storyPoints, int sortOrder, int milestoneStatusId)
        {
            ProjectId = projectId;
            Title = title;
            DueDate = dueDate;
            StoryPoints = storyPoints;
            SortOrder = sortOrder;
            MilestoneStatusId = milestoneStatusId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(int milestoneStatusId)
        {
            MilestoneStatusId = milestoneStatusId;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>Convenience: returns true if this milestone's status code is DONE.</summary>
        public bool IsDone => Status?.StatusCode == "DONE";

        /// <summary>Convenience: returns true if past due and not done.</summary>
        public bool IsOverdue(DateTime today) => !IsDone && DueDate.Date < today;
    }
}
