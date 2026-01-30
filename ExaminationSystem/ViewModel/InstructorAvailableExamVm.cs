namespace ExaminationSystem.ViewModel
{
    public class InstructorAvailableExamVm
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string ExamTitle { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public int TF_Count { get; set; }
        public int MCQ_Count { get; set; }
    }
}
