using ExaminationSystem.Abstractions.ResultPattern;

namespace ExaminationSystem.Services;

public interface IInstructorServcie
{
    public Task<Result<IEnumerable<CouresExamForInstructor>>> FindAvilabeExamsAsync(string InstructorId,CancellationToken cancellationToken);
    public Task<Result> GenerateAndAssignRandomExamAsync(int ExamId, 
        int NumberOfMCQ, int NumberOfTrueFalse, int MaxStudnet = 20,CancellationToken cancellationToken = default);
}
