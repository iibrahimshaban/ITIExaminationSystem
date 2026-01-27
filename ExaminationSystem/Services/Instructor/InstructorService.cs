using ExaminationSystem.Abstractions.Interfaces.Instructor;
using ExaminationSystem.Persistence;
using ExaminationSystem.ViewModel;

namespace ExaminationSystem.Services.Instructor
{
    public class InstructorService : IInstructorService
    {
        private readonly ApplicationDbContext _context;

        public InstructorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InstructorCourseVm>> GetInstructorCoursesAsync(string userId)
        {
            return await _context.CourseInstructors
                .Where(ci =>
                    ci.Instructor.UserId == userId &&
                    !ci.HadLeft &&
                    ci.Course.IsActive)
                .Select(ci => new InstructorCourseVm
                {
                    CourseId = ci.Course.Id,
                    CourseCode = ci.Course.CourseCode ?? "",
                    Title = ci.Course.Title,
                    Description = ci.Course.Description,
                    DurationDays = ci.Course.DurationDays,
                    Role = ci.Role
                })
                .ToListAsync();
        }
    }
}
