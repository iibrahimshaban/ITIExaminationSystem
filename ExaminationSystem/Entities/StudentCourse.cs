namespace ExaminationSystem.Entities;

public class StudentCourse
{
    public long Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateOnly? StartDate { get; set; }
    public string? Status { get; set; }              // Enrolled, InProgress, Completed...
    public decimal? ExamGrade { get; set; }
    public decimal? FinalGrade { get; set; }
    public decimal? CourseworkGrade { get; set; }
    public DateOnly? CompletionDate { get; set; }
    public string? Notes { get; set; }
    public bool Certificated { get; set; } = false;

    public Student Student { get; set; } = default!;
    public Course Course { get; set; } = default!;
}
