namespace ExaminationSystem.Persistence.SpDto;

public class ExamListDto
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string ExamTitle { get; set; } = null!;
    public int DurationInMinutes { get; set; }
    public int TotalPoints { get; set; }
    public bool IsRandomized { get; set; }
    public bool IsPublished { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
