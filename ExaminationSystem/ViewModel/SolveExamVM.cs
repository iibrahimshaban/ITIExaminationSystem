namespace ExaminationSystem.ViewModel
{
public class SolveExamVM
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = null!;
    public int DurationInMinutes { get; set; }
    public List<QuestionVM> Questions { get; set; } = [];
}


}
