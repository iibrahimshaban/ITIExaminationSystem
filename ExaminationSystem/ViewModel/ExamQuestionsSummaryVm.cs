namespace ExaminationSystem.ViewModel
{
    public class ExamQuestionsSummaryVm
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int MCQCount { get; set; }
        public int TrueFalseCount { get; set; }
        public bool CanPublish => MCQCount >= 7 && TrueFalseCount >= 3;
    }
}
