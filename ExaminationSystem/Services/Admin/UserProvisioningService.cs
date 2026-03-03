using ExaminationSystem.Abstractions.Consts;
using ExaminationSystem.Abstractions.Interfaces;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace ExaminationSystem.Services.Admin
{
    public class UserProvisioningService : IUserProvisioningService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserProvisioningService(
            ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        // ===================== LOOKUPS =====================

        public IEnumerable<SelectListItem> GetRoles() =>
            _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name!,
                Value = r.Name!
            });

        public IEnumerable<SelectListItem> GetBranches() =>
            _context.Branches.Select(b => new SelectListItem
            {
                Text = b.Name,
                Value = b.Id.ToString()
            });

        public IEnumerable<SelectListItem> GetTracks() =>
            _context.Tracks.Select(t => new SelectListItem
            {
                Text = t.Title,
                Value = t.Id.ToString()
            });

        public IEnumerable<SelectListItem> GetCourses() =>
            _context.Courses
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Text = c.Title,
                    Value = c.Id.ToString()
                });

        // ===================== DOMAIN CREATION =====================

        public async Task CreateDomainProfileAsync(ApplicationUser user, string role, RegisterVm model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (role == DefaultRoles.InstructorRole.Name)
                {
                    await CreateInstructorProfileAsync(user, model.InstructorDetails);
                }
                else if (role == DefaultRoles.StudentRole.Name)
                {
                    await CreateStudentProfileAsync(user, model.StudentDetails);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ===================== PRIVATE HELPERS =====================

        private async Task CreateInstructorProfileAsync(ApplicationUser user, InstructorDetailsVm? details)
        {
            if (details == null)
                throw new InvalidOperationException("Instructor details cannot be null.");

            if (!details.BranchId.HasValue)
                throw new InvalidOperationException("Instructor must have a branch assigned.");

            if (details.SelectedCourseIds == null || !details.SelectedCourseIds.Any())
                throw new InvalidOperationException("Instructor must have at least one course assigned.");

            var instructor = new ExaminationSystem.Entities.Instructor
            {
                UserId = user.Id,
                FirstName = user.Name!.Split(" ")[0],
                LastName = user.Name!.Split(" ")[1],
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                BranchId = details.BranchId.Value
            };

            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync(); // Get Instructor.Id

            foreach (var courseId in details.SelectedCourseIds)
            {
                _context.CourseInstructors.Add(new CourseInstructor
                {
                    CourseId = courseId,
                    InstructorId = instructor.Id,
                    Role = "Teaching",
                    AssignedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                });
            }
        }

        private async Task CreateStudentProfileAsync(ApplicationUser user, StudentDetailsVm? details)
        {
            if (details == null)
                throw new InvalidOperationException("Student details cannot be null.");

            if (!details.BranchId.HasValue)
                throw new InvalidOperationException("Student must have a branch assigned.");

            if (!details.TrackId.HasValue)
                throw new InvalidOperationException("Student must have a track assigned.");

            // 1️⃣ Create Student entity
            var student = new Student
            {
                UserId = user.Id,
                BranchId = details.BranchId.Value,
                TrackId = details.TrackId.Value,
                FirstName = user.Name!.Split(" ")[0],
                LastName = user.Name!.Split(" ")[1],
                Gender = true,
                DateOfBirth = new DateOnly(2001, 10, 12),
                EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "Active"
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync(); 

            var trackIdParam = new SqlParameter("@TrackId", details.TrackId.Value);
            var userIdParam = new SqlParameter("@UserId", user.Id);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_AssignTrackCoursesToStudent @TrackId, @UserId",
                trackIdParam,
                userIdParam
            );
        }
    }
}
    
