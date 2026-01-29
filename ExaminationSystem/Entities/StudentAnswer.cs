namespace ExaminationSystem.Entities;

public class StudentAnswer
{
    public long Id { get; set; }
    public long SubmissionId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedChoiceId { get; set; }      // for MCQ
    public bool? TFAnswer { get; set; }             // for T/F
    public int Point { get; set; } 

    public Submission Submission { get; set; } = default!;
    public Question Question { get; set; } = default!;
    public Choice? SelectedChoice { get; set; } = default!;
}
