using ExaminationSystem.Entities;
using ExaminationSystem.Persistence;
using ExaminationSystem.ViewModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

namespace ExaminationSystem.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= Student =================
        public async Task<Student?> GetStudentByUserIdAsync(string userId)
        {
            return await _context.Students
                .Include(s => s.Track)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // ================= Courses =================
        public async Task<List<Course>> GetStudentCoursesAsync(int studentId)
        {
            return await _context.StudentCourses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.Course)
                .ToListAsync();
        }

        // ================= Exams =================
        public async Task<List<Exam>> GetAvailableExamsAsync(int studentId)
        {
            var availableExamDtos = await _context
                .Set<ExamListDto>()
                .FromSqlRaw(
                    "EXEC [dbo].[GetAvailableExamsForStudent] @StudentId",
                    new SqlParameter("@StudentId", studentId)
                )
                .AsNoTracking()
                .ToListAsync();

            if (!availableExamDtos.Any())
            {
                return new List<Exam>(); // or throw if you prefer strict behavior
            }

            // Step 2: Extract the Exam IDs
            var examIds = availableExamDtos
                .Select(dto => dto.ExamId)
                .Distinct()
                .ToList();

            // Step 3: Load the full Exam entities (with whatever includes you need)
            var exams = await _context.Exams
                .Where(e => examIds.Contains(e.Id))
                .Include(e => e.Questions)           // ← add if needed for preview
                    .ThenInclude(q => q.Choices)     // ← optional
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt) // match SP ordering
                .ThenByDescending(e => e.Id)
                .ToListAsync();

            return exams;
        }

        // ================= Start Exam (SP) =================
        public async Task<SolveExamVM> StartExamUsingSpAsync(int studentId)
        {
            var data = await _context
                .Set<StudentExamQuestionRaw>()
                .FromSqlRaw(
                    "EXEC dbo.GetAllAssignedExamsForStudent @StudentId",
                    new SqlParameter("@StudentId", studentId)
                )
                .AsNoTracking()
                .ToListAsync();

            if (!data.Any())
                throw new Exception("NO_EXAMS_ASSIGNED");

            var firstRow = data.First();

            var questions = data
    .GroupBy(x => x.QuestionId)
    .OrderBy(g => g.First().QuestionOrder)
    .Select(g =>
    {
        var first = g.First();

        var vm = new QuestionVM
        {
            QuestionId = g.Key,
            Body = first.QuestionBody,
            Type = first.QuestionType
        };

        // MCQ only
        if (first.QuestionType == "MCQ")
        {
            vm.Choices = g
                .Where(x => x.ChoiceId != null)
    .GroupBy(x => x.ChoiceId)   // 
    .Select(g2 => new QuestionOptionVM
    {
        ChoiceId = g2.Key!.Value,
        Body = g2.First().ChoiceBody!
    })
                .ToList();
        }

        return vm;
    })
    .ToList();


            if (questions.Count == 0)
                throw new Exception("NO_QUESTIONS");

            return new SolveExamVM
            {
                ExamId = firstRow.ExamId,
                ExamTitle = firstRow.ExamTitle,
                DurationInMinutes = firstRow.DurationInMinutes,
                Questions = questions
            };
        }

        // ================= Submit Exam (SP) =================
        public async Task<long> SubmitExamUsingSpAsync(
            int studentId,
            int examId,
            int durationTakenSeconds,
            Dictionary<int, string> answers)
        {
            if (answers == null || answers.Count == 0)
                throw new Exception("NO_ANSWERS");

            var table = new DataTable();
            table.Columns.Add("QuestionId", typeof(int));
            table.Columns.Add("SelectedChoiceId", typeof(int));
            table.Columns.Add("TFAnswer", typeof(bool));

            foreach (var a in answers)
            {
                if (int.TryParse(a.Value, out int choiceId))
                    table.Rows.Add(a.Key, choiceId, DBNull.Value);
                else if (bool.TryParse(a.Value, out bool tf))
                    table.Rows.Add(a.Key, DBNull.Value, tf);
            }

            if (table.Rows.Count == 0)
                throw new Exception("NO_VALID_ANSWERS");

            var submissionIdParam = new SqlParameter
            {
                ParameterName = "@SubmissionId",
                SqlDbType = SqlDbType.BigInt,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC dbo.SubmitStudentExamAnswers
                    @StudentId,
                    @ExamId,
                    @DurationTakenSeconds,
                    @Answers,
                    @SubmissionId OUTPUT",
                new SqlParameter("@StudentId", studentId),
                new SqlParameter("@ExamId", examId),
                new SqlParameter("@DurationTakenSeconds", durationTakenSeconds),
                new SqlParameter
                {
                    ParameterName = "@Answers",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.StudentAnswerWithChoicesType",
                    Value = table
                },
                submissionIdParam
            );

            var submissionId = (long)submissionIdParam.Value;

            // Evaluate
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.EvaluateSubmissionAndUpdateGrades @SubmissionId",
                new SqlParameter("@SubmissionId", submissionId)
            );

            return submissionId;
        }

        // ================= Result =================
        public async Task<Submission?> GetSubmissionResultAsync(long submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Exam)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }
    }
}
