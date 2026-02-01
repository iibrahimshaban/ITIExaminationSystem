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

        public async Task<List<InstructorExamVm>> GetUnpublishedExamsAsync(string instructorUserId)
        {
            var instructorId = await _context.Instructors
                .Where(i => i.UserId == instructorUserId)
                .Select(i => i.Id)
                .FirstOrDefaultAsync();

            if (instructorId == 0)
                return new();

            return await _context.Exams
                .Where(e =>
                    !e.IsPublished &&
                    _context.CourseInstructors.Any(ci =>
                        ci.CourseId == e.CourseId &&
                        ci.InstructorId == instructorId &&
                        !ci.HadLeft
                    )
                )
                .Select(e => new InstructorExamVm
                {
                    ExamId = e.Id,
                    CourseId = e.CourseId!.Value,
                    CourseTitle = e.Course.Title,
                    ExamTitle = e.Title,
                    DurationInMinutes = e.DurationInMinutes,
                    TotalPoints = e.TotalPoints,
                    IsPublished = e.IsPublished,

                    MCQCount = e.Questions.Count(q => q.Type == "MCQ"),
                    TrueFalseCount = e.Questions.Count(q => q.Type == "TrueFalse")
                })
                .AsNoTracking()
                .ToListAsync();
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

        // ================================
        // Prepare Create Exam View
        // ================================
        public async Task<CreateExamVm?> PrepareCreateExamAsync(int courseId)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return null;

            return new CreateExamVm
            {
                CourseId = course.Id,
                CourseTitle = course.Title
            };
        }

        // ================================
        // Create Exam
        // ================================
        public async Task<int> CreateExamAsync(string instructorUserId, CreateExamVm model)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == instructorUserId);

            if (instructor == null)
                throw new UnauthorizedAccessException("Instructor not found.");

            var exam = new Exam
            {
                CourseId = model.CourseId,
                Title = model.Title,
                DurationInMinutes = model.DurationInMinutes,
                TotalPoints = model.TotalPoints,
                IsRandomized = model.IsRandomized,
                IsPublished = false,
                CreatedBy = instructor.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            return exam.Id;
        }


        public async Task<Result<ExamAssignmentResultVm>> GenerateAndAssignRandomExamAsync(
       int examId,
       int numberOfMCQ,
       int numberOfTrueFalse,
       int maxStudents = 20,
       CancellationToken cancellationToken = default)
        {
            var results = await _context.Set<ExamAssignmentResultVm>()
                .FromSqlRaw(
                    "EXEC dbo.AssignRandomQuestionsToAllStudents @ExamId, @MCQCount, @TrueFalseCount, @MaxStudents",
                    new SqlParameter("@ExamId", examId),
                    new SqlParameter("@MCQCount", numberOfMCQ),
                    new SqlParameter("@TrueFalseCount", numberOfTrueFalse),
                    new SqlParameter("@MaxStudents", maxStudents)
                )
                .AsNoTracking()
                .ToListAsync(cancellationToken);   // ✅ MATERIALIZE HERE

            var result = results.FirstOrDefault();

            if (result == null || result.StudentsProcessed == 0)
            {
                return Result.Failure<ExamAssignmentResultVm>(InstructorErrors.InvalidSp);
            }

            return Result.Success(result);
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
