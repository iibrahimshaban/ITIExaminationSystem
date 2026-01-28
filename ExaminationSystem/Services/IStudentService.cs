using ExaminationSystem.Entities;

namespace ExaminationSystem.Services
{
    public interface IStudentService
    {
        // Student Profile
        Task<Student?> GetStudentByUserIdAsync(string userId);

        // Courses
        Task<List<Course>> GetStudentCoursesAsync(int studentId);

        // Exams
        Task<List<Exam>> GetAvailableExamsAsync(int studentId);

        // Exam Flow
        Task<long> StartExamAsync(int studentId, int examId);

        Task SubmitExamAsync(
            long submissionId,
            Dictionary<int, object> answers   // QuestionId → Answer
        );

        Task<Submission?> GetSubmissionResultAsync(long submissionId);

        // Optional
        Task<List<Submission>> GetPreviousSubmissionsAsync(int studentId);
    }
}
