namespace ExaminationSystem.Entities;

public class Student
{
    public int Id { get; set; }                  
    public int BranchId { get; set; }
    public int? TrackId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; } = null!;

    public DateOnly EnrollmentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public string? Status { get; set; } = null!;

    public DateOnly? GraduationDate { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public Branch Branch { get; set; } = default!;
    public Track? Track { get; set; } = default!;
    public ICollection<StudentCourse> StudentCourses { get; set; } = [];

    public ICollection<Submission> Submissions { get; set; } = [];
}
