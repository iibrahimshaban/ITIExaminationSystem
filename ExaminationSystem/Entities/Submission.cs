namespace ExaminationSystem.Entities;

public class Submission
{
    public long Id { get; set; }
    public int StudentId { get; set; }
    public int? ExamId { get; set; }
    public bool IsCorrective { get; set; } = false;
    public DateTime? StartTime { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? DurationTakenSeconds { get; set; }
    public decimal? Grade { get; set; }

    public Student Student { get; set; } = default!;
    public Exam Exam { get; set; } = default!;
    public ICollection<StudentAnswer> Answers { get; set; } = [];
}
