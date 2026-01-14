namespace ExaminationSystem.Entities;

public class Exam
{
    public int Id { get; set; }
    public int? CourseId { get; set; }
    public string Title { get; set; } = null!;
    public int DurationInMinutes { get; set; }
    public int TotalPoints { get; set; }
    public bool IsRandomized { get; set; } = false;
    public bool IsPublished { get; set; } = false;
    public int? CreatedBy { get; set; }   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Course Course { get; set; } = default!;
    public Instructor Instructor { get; set; } = default!;
    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<Submission> Submissions { get; set; } = [];
}
