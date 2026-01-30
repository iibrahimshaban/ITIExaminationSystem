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

        // ================= Exam Flow =================
        public async Task<long> StartExamAsync(int studentId, int examId)
        {
            var submission = new Submission
            {
                StudentId = studentId,
                ExamId = examId,
                StartTime = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return submission.Id;
        }

        public async Task SubmitExamAsync(
    long submissionId,
    Dictionary<int, string> answers)
        {
            foreach (var answer in answers)
            {
                var questionId = answer.Key;
                var value = answer.Value;

                var studentAnswer = new StudentAnswer
                {
                    SubmissionId = submissionId,
                    QuestionId = questionId,
                    AnsweredAt = DateTime.UtcNow
                };

                // MCQ
                if (int.TryParse(value, out int choiceId))
                {
                    studentAnswer.SelectedChoiceId = choiceId;
                }
                // True / False
                else if (bool.TryParse(value, out bool tf))
                {
                    studentAnswer.TFAnswer = tf;
                }

                _context.StudentAnswers.Add(studentAnswer);
            }

            await _context.SaveChangesAsync();

            var submission = await _context.Submissions
                .FirstAsync(s => s.Id == submissionId);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_CorrectExam @ExamId, @StudentId",
                new SqlParameter("@ExamId", submission.ExamId),
                new SqlParameter("@StudentId", submission.StudentId)
            );


        }

        // ================= Results =================
        public async Task<Submission?> GetSubmissionResultAsync(long submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Exam)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<List<Submission>> GetPreviousSubmissionsAsync(int studentId)
        {
            return await _context.Submissions
                .Include(s => s.Exam)
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        //

        public async Task<SolveExamVM> StartExamWithQuestionsAsync(int studentId, int examId)
{
    // 1️⃣ Check: Exam exists & published
    var exam = await _context.Exams
        .Include(e => e.Questions)
            .ThenInclude(q => q.Choices)
        .FirstOrDefaultAsync(e => e.Id == examId && e.IsPublished);

    if (exam == null)
        throw new Exception("Exam not found or not published");

    // 2️⃣ Check: Has student already taken this exam (non-corrective)?
    var alreadyTaken = await _context.Submissions.AnyAsync(s =>
        s.StudentId == studentId &&
        s.ExamId == examId &&
        s.IsCorrective == false
    );

    if (alreadyTaken)
        throw new Exception("You already took this exam");

    // 3️⃣ Randomize questions
    var randomQuestions = exam.Questions
        .OrderBy(q => Guid.NewGuid())
        .ToList();

    // 4️⃣ Map to ViewModel (NO Submission creation here)
    return new SolveExamVM
    {
        ExamId = exam.Id,
        ExamTitle = exam.Title,
        DurationInMinutes = exam.DurationInMinutes,

        Questions = randomQuestions.Select(q => new QuestionVM
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



        public async Task<long> SubmitExamUsingSpAsync(
    int studentId,
    int examId,
    Dictionary<int, string> answers)
        {
            var answersTable = BuildAnswersDataTable(answers);

            var startTime = DateTime.UtcNow;   // أو تجيبها من Session
            var endTime = DateTime.UtcNow;

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
              Value = answersTable
          },
          new SqlParameter("@StartTime", startTime),
          submissionIdParam
      );


            return (long)submissionIdParam.Value;
        }


        private DataTable BuildAnswersDataTable(Dictionary<int, string> answers)
        {
            var table = new DataTable();
            table.Columns.Add("QuestionId", typeof(int));
            table.Columns.Add("SelectedChoiceId", typeof(int));

            foreach (var answer in answers)
            {
                if (int.TryParse(answer.Value, out int choiceId))
                {
                    table.Rows.Add(answer.Key, choiceId);
                }
            }

            return table;
        }

        public async Task<bool> CanStartExamAsync(int studentId, int examId)
        {
            return !await _context.Submissions.AnyAsync(s =>
                s.StudentId == studentId &&
                s.ExamId == examId &&
                s.IsCorrective == false
            );
        }

    }
}
