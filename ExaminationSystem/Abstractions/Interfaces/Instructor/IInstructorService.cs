using ExaminationSystem.ViewModel;

namespace ExaminationSystem.Abstractions.Interfaces.Instructor
{
    public interface IInstructorService
    {
        Task<List<InstructorCourseVm>> GetInstructorCoursesAsync(string userId);
    }
}
