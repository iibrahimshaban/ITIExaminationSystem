using ExaminationSystem.Entities;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExaminationSystem.Abstractions.Interfaces
{
    public interface IUserProvisioningService
    {
        IEnumerable<SelectListItem> GetRoles();
        IEnumerable<SelectListItem> GetBranches();
        IEnumerable<SelectListItem> GetTracks();
        IEnumerable<SelectListItem> GetCourses();

        Task CreateDomainProfileAsync(ApplicationUser user, string role, RegisterVm model);
    }
}
