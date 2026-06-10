using System;
using PRMTool.Domain.Enums;

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
        public MilestoneStatus Status { get; private set; } = MilestoneStatus.NOT_STARTED;
        public int SortOrder { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        protected Milestone() { }

        public Milestone(int projectId, string title, DateTime dueDate, int storyPoints, int sortOrder)
        {
            ProjectId = projectId;
            Title = title;
            DueDate = dueDate;
            StoryPoints = storyPoints;
            SortOrder = sortOrder;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(MilestoneStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
