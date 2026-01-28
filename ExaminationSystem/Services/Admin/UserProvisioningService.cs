using ExaminationSystem.Abstractions.Consts;
using ExaminationSystem.Abstractions.Interfaces;
using ExaminationSystem.Entities;
using ExaminationSystem.Persistence;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                    if (!model.BranchId.HasValue || model.SelectedCourseIds == null || !model.SelectedCourseIds.Any())
                        throw new InvalidOperationException("Instructor must have at least one course.");

                    var instructor = new ExaminationSystem.Entities.Instructor
                    {
                        UserId = user.Id,
                        FirstName = user.Name ?? "instructor",
                        HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        BranchId = model.BranchId.Value
                    };

                    _context.Instructors.Add(instructor);
                    await _context.SaveChangesAsync();

                    foreach (var courseId in model.SelectedCourseIds)
                    {
                        _context.CourseInstructors.Add(new CourseInstructor
                        {
                            CourseId = courseId,
                            InstructorId = instructor.Id,
                            Role="Teaching",
                            AssignedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                        });
                    }
                }
                else if (role == DefaultRoles.StudentRole.Name)
                {
                    if (!model.BranchId.HasValue || !model.TrackId.HasValue)
                        throw new InvalidOperationException("Student must have Branch and Track.");

                    // 1️⃣ Create Student
                    var student = new Student
                    {
                        UserId = user.Id,
                        BranchId = model.BranchId.Value,
                        TrackId = model.TrackId.Value,
                        FirstName=model.Name,
                        EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        Status = "Active"
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync(); // get Student.Id

                    // 2️⃣ Get all active courses for the selected track
                    var trackCourses = await _context.Courses
                        .Where(c =>
                            c.IsActive &&
                            c.TrackId == model.TrackId.Value)
                        .Select(c => c.Id)
                        .ToListAsync();

                    // 3️⃣ Enroll student in all track courses
                    foreach (var courseId in trackCourses)
                    {
                        _context.StudentCourses.Add(new StudentCourse
                        {
                            StudentId = student.Id,
                            CourseId = courseId,
                            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                            Status = "Enrolled"
                        });
                    }
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
    }
}
    
