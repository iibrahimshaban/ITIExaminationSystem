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

        public async Task<InstructorCourseDetailsVm?> GetCourseDetailsAsync(
     string userId,
     int courseId)
        {
            // 1️⃣ Instructor
            var instructor = await _context.Instructors
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.UserId == userId);

            if (instructor == null)
                return null;

            // 2️⃣ Course + Topics + Track
            var course = await _context.Courses
                .Include(c => c.Track)
                .Include(c => c.Topics)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return null;

            // 3️⃣ Student count (branch + track)
            var studentsCount = await _context.StudentCourses
                .CountAsync(sc =>
                    sc.CourseId == course.Id &&
                    sc.Student.BranchId == instructor.BranchId &&
                    sc.Student.TrackId == course.TrackId);

            // 4️⃣ Map to VM
            return new InstructorCourseDetailsVm
            {
                CourseId = course.Id,
                CourseCode = course.CourseCode,
                CourseTitle = course.Title,
                Description = course.Description,
                DurationDays = course.DurationDays,

                BranchId = instructor.BranchId,
                BranchName = instructor.Branch.Name,

                TrackId = course.TrackId,
                TrackName = course.Track?.Title,

                StudentsCount = studentsCount,

                Topics = course.Topics
                    .OrderBy(t => t.Order)
                    .Select(t => new CourseTopicVm
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Order = t.Order,
                        IsRequired = t.IsRequired
                    })
                    .ToList()
            };
        }



    }
}
