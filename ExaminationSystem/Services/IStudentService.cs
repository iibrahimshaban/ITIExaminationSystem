using ExaminationSystem.Entities;
using ExaminationSystem.ViewModel;

namespace ExaminationSystem.Services
{
    // new running 
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
      Dictionary<int, string> answers
     );


        Task<Submission?> GetSubmissionResultAsync(long submissionId);

        // Optional
        Task<List<Submission>> GetPreviousSubmissionsAsync(int studentId);


        // 
        Task<SolveExamVM> StartExamWithQuestionsAsync(int studentId, int examId);
        Task<long> SubmitExamUsingSpAsync(
  int studentId,
  int examId,   
  Dictionary<int, string> answers
);


        Task<bool> CanStartExamAsync(int studentId, int examId);

    }
}
