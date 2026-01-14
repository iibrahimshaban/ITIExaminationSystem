namespace ExaminationSystem.Entities;

public class Instructor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string? JobTitles { get; set; }
    public decimal? Salary { get; set; }
    public bool? Gender { get; set; }
    public DateOnly? HireDate { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Qualification { get; set; }
    public string? UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public Branch Branch { get; set; } = default!;
    public ICollection<CourseInstructor> CourseInstructors { get; set; } =  [];
}
