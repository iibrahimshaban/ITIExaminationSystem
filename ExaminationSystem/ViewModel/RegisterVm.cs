using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.ViewModel
{
    public class RegisterVm
    {
        // Identity

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(5, ErrorMessage = "Minimum Password Length is 5")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmPassword is required")]
        [System.ComponentModel.DataAnnotations.Compare(nameof(Password), ErrorMessage = "Confirm Password doesn't match Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public string? RedirectUrl { get; set; }

        // Role
        public string Role { get; set; } = string.Empty;

        // 🔥 Instructor-specific
        public List<int>? SelectedCourseIds { get; set; }

        // 🔥 Student-specific
        public int? BranchId { get; set; }
        public int? TrackId { get; set; }

        // UI helpers (later)
        public IEnumerable<SelectListItem>? CourseList { get; set; }
        public IEnumerable<SelectListItem>? BranchList { get; set; }
        public IEnumerable<SelectListItem>? RoleList { get; set; }
        public IEnumerable<SelectListItem>? TrackList { get; set; }
    }

}
