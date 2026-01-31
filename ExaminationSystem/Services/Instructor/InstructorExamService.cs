using ExaminationSystem.Abstractions.Interfaces.Instructor;
using ExaminationSystem.Abstractions.ResultPattern;
using ExaminationSystem.Errors;
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

        public async Task<Result> GenerateAndAssignRandomExamAsync(int ExamId, int NumberOfMCQ, int NumberOfTrueFalse, int MaxStudnet = 20, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.Database.ExecuteSqlRawAsync(
                "EXEC AssignRandomQuestionsToAllStudents @ExamId, @MCQCount, @TrueFalseCount, @MaxStudents",
                new SqlParameter("@ExamId", ExamId),
                new SqlParameter("@MCQCount", NumberOfMCQ),
                new SqlParameter("@TrueFalseCount", NumberOfTrueFalse),
                new SqlParameter("@MaxStudents", MaxStudnet)
            );

            // You may want to return a Result based on affectedRows, e.g.:
            return affectedRows > 0
                ? Result.Success()
                : Result.Failure(InstructorErrors.InvalidSp);
        }


        public async Task<InstructorExamDetailsVm?> GetExamDetailsAsync(string instructorUserId,int examId)
        {
            

            // 2️⃣ Load exam details via EF Core
            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                return null;

            // 3️⃣ Map to ViewModel
            return new InstructorExamDetailsVm
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                CourseTitle = exam.Course.Title,
                DurationInMinutes = exam.DurationInMinutes,

                TotalQuestions = exam.Questions.Count,
                MCQ_Count = exam.Questions.Count(q => q.Type == "MCQ"),
                TF_Count = exam.Questions.Count(q => q.Type == "TrueFalse"),

                // defaults for form
                NumberOfMCQ = 7,
                NumberOfTrueFalse = 3,
                MaxStudents = 20
            };
        }



    }
}
