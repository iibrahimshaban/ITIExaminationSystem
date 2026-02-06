using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.ViewModel
{
    public class CreateQuestionVm
    {
        public int ExamId { get; set; }

        [Required(ErrorMessage = "Question body is required")]
        [StringLength(1000, ErrorMessage = "Question body cannot exceed 1000 characters")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Question type is required")]
        public string Type { get; set; } = "MCQ"; // Default to MCQ

        [Required(ErrorMessage = "Points are required")]
        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
        public int Points { get; set; }

        // For True/False questions only
        public bool? CorrectAnswer { get; set; }

        // For MCQ questions only (no validation attributes here - validated in controller)
        public List<CreateChoiceVm>? Choices { get; set; } = new List<CreateChoiceVm>();
    }
}
