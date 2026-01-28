using ExaminationSystem.Entities;
using ExaminationSystem.Persistence;
using Microsoft.EntityFrameworkCore;

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
            Dictionary<int, object> answers)
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

                if (value is int choiceId)
                    studentAnswer.SelectedChoiceId = choiceId;
                else if (value is bool tf)
                    studentAnswer.TFAnswer = tf;

                _context.StudentAnswers.Add(studentAnswer);
            }

            await _context.SaveChangesAsync();
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
    }
}
