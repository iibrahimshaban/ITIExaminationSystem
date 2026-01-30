namespace ExaminationSystem.ViewModel
{
    public class InstructorCourseDetailsVm
    {
        // Course Info
        public int CourseId { get; set; }
        public string? CourseCode { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DurationDays { get; set; }

        // Context Info
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public int? TrackId { get; set; }
        public string? TrackName { get; set; }

        // Stats
        public int StudentsCount { get; set; }

        // ✅ NEW
        public List<CourseTopicVm> Topics { get; set; } = [];
    }

}
