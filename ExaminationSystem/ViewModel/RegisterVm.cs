using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.ViewModel
{
    // Base ViewModel for common registration fields
    public class RegisterVm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(5)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? RedirectUrl { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        // UI helpers
        public IEnumerable<SelectListItem>? RoleList { get; set; }
        public IEnumerable<SelectListItem>? BranchList { get; set; }

        // Role-specific nested models
        public InstructorDetailsVm? InstructorDetails { get; set; }
        public StudentDetailsVm? StudentDetails { get; set; }
    }

    // Instructor-specific details
    public class InstructorDetailsVm
    {
        public int? BranchId { get; set; }
        public List<int>? SelectedCourseIds { get; set; }
    }

    // Student-specific details
    public class StudentDetailsVm
    {
        public int? BranchId { get; set; }
        public int? TrackId { get; set; }
        public IEnumerable<SelectListItem>? TrackList { get; set; }
    }

}
