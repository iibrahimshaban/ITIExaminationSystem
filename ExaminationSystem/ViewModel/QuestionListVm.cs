namespace ExaminationSystem.ViewModel
{
    public class QuestionListVm
    {
        public int Id { get; set; }
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Points { get; set; }
        public bool? CorrectAnswer { get; set; } // For True/False
        public List<ChoiceDisplayVm> Choices { get; set; } = new List<ChoiceDisplayVm>();
    }

    public class ChoiceDisplayVm
    {
        public string ChoiceLetter { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
