namespace ExaminationSystem.ViewModel
{
    public class InstructorExamDetailsVm
    {
        public int ExamId { get; set; }

        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;

        public string ExamTitle { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }

        public int TotalQuestions { get; set; }
        public int MCQ_Count { get; set; }
        public int TF_Count { get; set; }

        // Assignment inputs (FORM)
        public int NumberOfMCQ { get; set; }
        public int NumberOfTrueFalse { get; set; }
        public int MaxStudents { get; set; }
    }

}
