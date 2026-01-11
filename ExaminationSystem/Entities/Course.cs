namespace ExaminationSystem.Entities;

public class Course
{
    public int Id { get; set; }
    public string? CourseCode { get; set; }          
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? DurationDays { get; set; }
    public bool IsActive { get; set; } = true;
    public int? TrackId { get; set; }

    public Track Track { get; set; } = default!;
    public ICollection<Topic> Topics { get; set; } = [];
    public ICollection<CourseInstructor> CourseInstructors { get; set; } = [];
    public ICollection<Exam> Exams { get; set; } = [];
    public ICollection<StudentCourse> StudentCourses { get; set; } =  [];
}
