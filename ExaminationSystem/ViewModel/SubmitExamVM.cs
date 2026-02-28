namespace ExaminationSystem.ViewModel
{
    public class SubmitExamVM
    {
        public int ExamId { get; set; }
        public Dictionary<int, string> Answers { get; set; } = [];
    }

}
