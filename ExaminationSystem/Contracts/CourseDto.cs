namespace ExaminationSystem.Contracts;

public record CourseDto(
    int InstructorId,
    string InstructorName,
    string CourseCode,
    string CourseTitle,
    string ExamTitle,
    int DurationInMinutes,
    int TotalQuestions,
    int TF_Count,
    int MCQ_Count
    );
