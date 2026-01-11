namespace ExaminationSystem.Entities;

public class CourseInstructor
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int InstructorId { get; set; }
    public string? Role { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool HadLeft { get; set; } = false;

    public Course Course { get; set; } = default!;
    public Instructor Instructor { get; set; } = default!;
}
