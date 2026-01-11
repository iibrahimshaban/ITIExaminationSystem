namespace ExaminationSystem.Entities;

public class Choice
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Body { get; set; } = null!;
    public string? ChoiceLetter { get; set; }       // A, B, C, D
    public int? DisplayOrder { get; set; }
    public bool IsCorrect { get; set; } = false;

    public Question Question { get; set; } = default!;
    public ICollection<StudentAnswer> StudentAnswers { get; set; } = [];
}
