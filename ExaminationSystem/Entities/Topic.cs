namespace ExaminationSystem.Entities;

public class Topic
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string? Title { get; set; } = null!;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public Course Course { get; set; } = default!;
}
