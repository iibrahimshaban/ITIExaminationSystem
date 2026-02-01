namespace ExaminationSystem.ViewModel
{
    public class ExamAssignmentResultVm
    {
        public int ExamId { get; set; }
        public int StudentsProcessed { get; set; }
        public int TrueFalseAssigned { get; set; }
        public int MCQAssigned { get; set; }
        public int TotalAssignments { get; set; }
    }
}
