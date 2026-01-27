using ExaminationSystem.Entities;

namespace ExaminationSystem.Abstractions.Interfaces
{
    public interface IUserProvisioningService
    {
        Task CreateDomainProfileAsync(ApplicationUser user, string role);
    }
}
