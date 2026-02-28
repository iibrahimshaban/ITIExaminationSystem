namespace ExaminationSystem.ViewModel
{
    public class QuestionVM
    {
        public int QuestionId { get; set; }
        public string Body { get; set; } = null!;
        public string Type { get; set; } = null!; // MCQ / TF
        public List<QuestionOptionVM>? Choices { get; set; }
    }

}
