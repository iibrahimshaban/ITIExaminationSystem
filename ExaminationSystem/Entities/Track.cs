namespace ExaminationSystem.Entities;

public class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? DurationMonths { get; set; }
    public int? TotalCourses { get; set; }
    public int? TotalHours { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Course> Courses { get; set; } = [];
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<BranchTrack> BranchTracks { get; set; } = [];
}
