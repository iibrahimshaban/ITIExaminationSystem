namespace ExaminationSystem.Entities;

public class Question
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string Body { get; set; } = null!;
    public string Type { get; set; } = null!;       // "MCQ", "TF", ...
    public int? Points { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool? CorrectAnswer { get; set; }        // for TF

    public Exam Exam { get; set; } = default!;
    public ICollection<Choice> Choices { get; set; } = [];
    public ICollection<StudentAnswer> StudentAnswers { get; set; } = [];
}
