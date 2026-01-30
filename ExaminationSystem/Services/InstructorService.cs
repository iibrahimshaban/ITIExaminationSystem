using ExaminationSystem.Abstractions.ResultPattern;
using ExaminationSystem.Errors;
using ExaminationSystem.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;



namespace ExaminationSystem.Services;

public class InstructorService(ApplicationDbContext context) : IInstructorServcie
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<IEnumerable<CouresExamForInstructor>>> FindAvilabeExamsAsync(string InstructorId, CancellationToken cancellationToken)
    {
        var result = await _context.CouresExamForInstructors.
            FromSql($"EXEC FindAvilableCourseAndExams @UserId={InstructorId}").ToListAsync(cancellationToken);

        if (result.Count < 1)
            return Result.Failure<IEnumerable<CouresExamForInstructor>>(InstructorErrors.NoExamsFound);

        return Result.Success<IEnumerable<CouresExamForInstructor>>(result);
    }

    public async Task<Result> GenerateAndAssignRandomExamAsync(int ExamId, int NumberOfMCQ, int NumberOfTrueFalse, int MaxStudnet = 20, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _context.Database.ExecuteSqlRawAsync(
            "EXEC AssignRandomQuestionsToAllStudents @ExamId, @MCQCount, @TrueFalseCount, @MaxStudents",
            new SqlParameter("@ExamId", ExamId),
            new SqlParameter("@MCQCount", NumberOfMCQ),
            new SqlParameter("@TrueFalseCount", NumberOfTrueFalse),
            new SqlParameter("@MaxStudents", MaxStudnet)
        );

        // You may want to return a Result based on affectedRows, e.g.:
        return affectedRows > 0
            ? Result.Success()
            : Result.Failure(InstructorErrors.InvalidSp);
    }
}
