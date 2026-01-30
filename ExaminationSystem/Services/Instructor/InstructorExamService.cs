using ExaminationSystem.Abstractions.Interfaces.Instructor;
using ExaminationSystem.Persistence;
using ExaminationSystem.ViewModel;
using Microsoft.Data.SqlClient;

namespace ExaminationSystem.Services.Instructor
{
    public class InstructorExamService : IInstructorExamService
    {
        private readonly ApplicationDbContext _context;

        public InstructorExamService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InstructorAvailableExamVm>> GetAvailableExamsAsync(string instructorUserId)
        {
            if (string.IsNullOrWhiteSpace(instructorUserId))
            {
                return new List<InstructorAvailableExamVm>();
            }

            var userIdParam = new SqlParameter("@InstructorUserId", instructorUserId);

            var exams = await _context.Database
                .SqlQueryRaw<InstructorAvailableExamVm>(
                    "EXEC dbo.FindAvilableCourseAndExams @InstructorUserId",
                    userIdParam)
                .ToListAsync();

            return exams;
        }
    }
}
