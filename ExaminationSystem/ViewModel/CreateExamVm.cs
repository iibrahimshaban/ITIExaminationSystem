using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.ViewModel
{
    public class CreateExamVm
    {
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Range(10, 300)]
        public int DurationInMinutes { get; set; }

        [Range(1, 500)]
        public int TotalPoints { get; set; }

        public bool IsRandomized { get; set; }

     
    }
}
