namespace ExaminationSystem.ViewModel
{
    public class InstructorExamVm
    {
        public int ExamId { get; set; }
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;
        public string ExamTitle { get; set; } = string.Empty;

        public int DurationInMinutes { get; set; }
        public int TotalPoints { get; set; }

        public bool IsPublished { get; set; }

        public int MCQCount { get; set; }
        public int TrueFalseCount { get; set; }
    }
}
