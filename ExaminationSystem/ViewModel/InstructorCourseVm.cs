namespace ExaminationSystem.ViewModel
{
    public class InstructorCourseVm
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DurationDays { get; set; }
        public string? Role { get; set; }   // Instructor role in course
    }
}
