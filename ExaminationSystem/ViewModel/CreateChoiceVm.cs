using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.ViewModel
{
    public class CreateChoiceVm
    {
        [Required(ErrorMessage = "Choice text is required")]
        [StringLength(500, ErrorMessage = "Choice text cannot exceed 500 characters")]
        public string Body { get; set; } = string.Empty;

        [Required]
        public string ChoiceLetter { get; set; } = string.Empty; // A, B, C, D

        public bool IsCorrect { get; set; }
    }
}
