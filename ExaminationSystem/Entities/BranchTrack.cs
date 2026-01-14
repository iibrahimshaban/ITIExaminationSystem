namespace ExaminationSystem.Entities;

public class BranchTrack
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int TrackId { get; set; }
    public DateOnly? CreationDate { get; set; }
    public bool IsCurrentlyOffered { get; set; } = true;
    public int NumberOfStudents { get; set; } = 0;

    public Branch Branch { get; set; } = null!;
    public Track Track { get; set; } = null!;
}
