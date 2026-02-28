namespace ExaminationSystem.ViewModel
{
	[Keyless]
	public class StudentExamQuestionRaw
	{
		public int ExamId { get; set; }
		public string ExamTitle { get; set; } = null!;
		public int DurationInMinutes { get; set; }

		public int QuestionOrder { get; set; }
		public int QuestionId { get; set; }
		public string QuestionBody { get; set; } = null!;
		public string QuestionType { get; set; } = null!;

		public int? ChoiceId { get; set; }
		public string? ChoiceBody { get; set; }
	}

}
