using ExaminationSystem.Entities;
using ExaminationSystem.ViewModel;

namespace ExaminationSystem.Services
{
	public interface IStudentService
	{
		// Student
		Task<Student?> GetStudentByUserIdAsync(string userId);

		// Courses
		Task<List<Course>> GetStudentCoursesAsync(int studentId);

		// Exams list
		Task<List<Exam>> GetAvailableExamsAsync(int studentId);

		// Start Exam (SP based)
		Task<SolveExamVM> StartExamUsingSpAsync(int studentId);

		// Submit Exam (SP based)
		Task<long> SubmitExamUsingSpAsync(
			int studentId,
			int examId,
			int durationTakenSeconds,
			Dictionary<int, string> answers
		);

		// Result
		Task<Submission?> GetSubmissionResultAsync(long submissionId);
	}
}
