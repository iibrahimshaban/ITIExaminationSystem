using ExaminationSystem.ViewModel;

namespace ExaminationSystem.Abstractions.Interfaces.Instructor
{
   
        public interface IInstructorExamService
        {
            /// <summary>
            /// Retrieves all available exams for courses taught by the specified instructor.
            /// Uses stored procedure: dbo.FindAvilableCourseAndExams
            /// </summary>
            /// <param name="instructorUserId">The UserId of the instructor (from Identity)</param>
            /// <returns>List of available exams with course and question details</returns>
            Task<List<InstructorAvailableExamVm>> GetAvailableExamsAsync(string instructorUserId);
        }
    
}
