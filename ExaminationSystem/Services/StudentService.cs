using ExaminationSystem.Entities;
using ExaminationSystem.Persistence;
using ExaminationSystem.ViewModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
            return await _context.StudentCourses
                .Where(sc => sc.StudentId == studentId)
                .SelectMany(sc => sc.Course.Exams)
                .Where(e => e.IsPublished)
                .Distinct()
                .ToListAsync();
        }

        // ================= Start Exam =================
        public async Task<SolveExamVM> StartExamWithQuestionsAsync(int studentId, int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(e => e.Id == examId && e.IsPublished);

            if (exam == null)
                throw new Exception("Exam not found or not published");

            var alreadyTaken = await _context.Submissions.AnyAsync(s =>
                s.StudentId == studentId &&
                s.ExamId == examId &&
                s.IsCorrective == false
            );

            if (alreadyTaken)
                throw new Exception("EXAM_ALREADY_TAKEN");

            if (exam.Questions == null || exam.Questions.Count < 10)
                throw new Exception("NOT_ENOUGH_QUESTIONS");

            var questions = exam.Questions
                .OrderBy(q => Guid.NewGuid())
                .Take(10)
                .ToList();

            return new SolveExamVM
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                DurationInMinutes = exam.DurationInMinutes,
                Questions = questions.Select(q => new QuestionVM
                {
                    QuestionId = q.Id,
                    Body = q.Body,
                    Type = q.Type,
                    Choices = q.Type == "MCQ"
                        ? q.Choices.Select(c => new QuestionOptionVM
                        {
                            ChoiceId = c.Id,
                            Body = c.Body
                        }).ToList()
                        : null
                }).ToList()
            };
        }

        // ================= Submit Exam =================
        public async Task<long> SubmitExamUsingSpAsync(
            int studentId,
            int examId,
            Dictionary<int, string> answers)
        {
            var alreadySubmitted = await _context.Submissions.AnyAsync(s =>
                s.StudentId == studentId &&
                s.ExamId == examId &&
                s.IsCorrective == false
            );

            if (alreadySubmitted)
                throw new Exception("EXAM_ALREADY_SUBMITTED");

            var table = BuildAnswersDataTable(answers);

            if (table.Rows.Count == 0)
                throw new Exception("NO_ANSWERS");

            var submissionIdParam = new SqlParameter
            {
                ParameterName = "@SubmissionId",
                SqlDbType = SqlDbType.BigInt,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC dbo.SP_SubmitExamAnswers 
                    @ExamId,
                    @StudentId,
                    @Answers,
                    @StartTime,
                    @SubmissionId OUTPUT",
                new SqlParameter("@ExamId", examId),
                new SqlParameter("@StudentId", studentId),
                new SqlParameter
                {
                    ParameterName = "@Answers",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.StudentAnswerType",
                    Value = table
                },
                new SqlParameter("@StartTime", DateTime.UtcNow),
                submissionIdParam
            );

            return (long)submissionIdParam.Value;
        }

        private DataTable BuildAnswersDataTable(Dictionary<int, string> answers)
        {
            var table = new DataTable();
            table.Columns.Add("QuestionId", typeof(int));
            table.Columns.Add("SelectedChoiceId", typeof(int));

            foreach (var a in answers)
            {
                if (int.TryParse(a.Value, out int choiceId))
                    table.Rows.Add(a.Key, choiceId);
            }

            return table;
        }

        // ================= Results =================
        public async Task<Submission?> GetSubmissionResultAsync(long submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Exam)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }
    }
}
