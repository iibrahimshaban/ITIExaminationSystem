using ExaminationSystem.Entities;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExaminationSystem.Abstractions.Interfaces
{
    public interface IUserProvisioningService
    {
        // 🔹 Create Instructor / Student with relations
        Task CreateDomainProfileAsync(
            ApplicationUser user,
            string role,
            RegisterVm model
        );

        // 🔹 Lookups for Register page
        IEnumerable<SelectListItem> GetRoles();
        IEnumerable<SelectListItem> GetBranches();
        IEnumerable<SelectListItem> GetTracks();
        IEnumerable<SelectListItem> GetCourses();
    }
}
