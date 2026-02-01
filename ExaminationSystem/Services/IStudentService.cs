using ExaminationSystem.Entities;
using ExaminationSystem.ViewModel;

namespace ExaminationSystem.Services
{
    public interface IStudentService
    {
        // ================= Student =================
        Task<Student?> GetStudentByUserIdAsync(string userId);

        // ================= Courses =================
        Task<List<Course>> GetStudentCoursesAsync(int studentId);

        // ================= Exams =================
        Task<List<Exam>> GetAvailableExamsAsync(int studentId);

        // ================= Exam Flow =================

        /// <summary>
        /// Load exam with 10 random questions after validating:
        /// - Exam is published
        /// - Student did not take the exam before (non-corrective)
        /// - Exam has enough questions
        /// </summary>
        Task<SolveExamVM> StartExamWithQuestionsAsync(int studentId, int examId);

        /// <summary>
        /// Submit exam answers using stored procedure.
        /// This method:
        /// - Prevents double submission
        /// - Stores answers
        /// - Calculates grade
        /// - Returns SubmissionId
        /// </summary>
        Task<long> SubmitExamUsingSpAsync(
            int studentId,
            int examId,
            Dictionary<int, string> answers
        );

        // ================= Results =================
        Task<Submission?> GetSubmissionResultAsync(long submissionId);
    }
}
