using ExaminationSystem.Abstractions.Consts;
using ExaminationSystem.Abstractions.Interfaces;
using ExaminationSystem.Entities;
using ExaminationSystem.Persistence;

namespace ExaminationSystem.Services.Admin
{
    public class UserProvisioningService : IUserProvisioningService
    {
        private readonly ApplicationDbContext _context;

        public UserProvisioningService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateDomainProfileAsync(ApplicationUser user, string role)
        {
            if (role == DefaultRoles.InstructorRole.Name)
            {
                var instructor = new  ExaminationSystem.Entities.Instructor
                {
                    UserId = user.Id,
                    FirstName = user.Name,
                    LastName = string.Empty,
                    BranchId = 1, // temporary default
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                _context.Instructors.Add(instructor);
            }
            else if (role == DefaultRoles.StudentRole.Name)
            {
                var student = new Student
                {
                    UserId = user.Id
                };

                _context.Students.Add(student);
            }

            await _context.SaveChangesAsync();
        }
    }
}
