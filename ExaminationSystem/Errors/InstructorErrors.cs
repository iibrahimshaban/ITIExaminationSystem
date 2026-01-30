using ExaminationSystem.Abstractions.ResultPattern;

namespace ExaminationSystem.Errors;

public static class InstructorErrors
{
    public static readonly Error InvalidSp =
        new("Instructor.AssignRandomQuestionsToAllStudents", "an error occured at AssignRandomQuestionsToAllStudents sp and it didn't assigned exam for the students");
    
    public static readonly Error NoExamsFound =
        new("Instructor.NoExamsFound", "no exams found for the courses you are assigned at do you wanna add one ??");

}
