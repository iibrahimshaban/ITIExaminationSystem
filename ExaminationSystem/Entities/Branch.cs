namespace ExaminationSystem.Entities;

public class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public int ManagerId { get; set; }         // FK to Instructor
    public DateTime? EstablishDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Instructor Manager { get; set; } = default!;
    public ICollection<Instructor> Instructors { get; set; } = [];
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<BranchTrack> BranchTracks { get; set; } = [];
}
