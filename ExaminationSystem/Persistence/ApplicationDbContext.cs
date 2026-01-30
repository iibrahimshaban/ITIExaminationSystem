using ExaminationSystem.Entities;
using ExaminationSystem.Persistence.SpDto;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace ExaminationSystem.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<CouresExamForInstructor>(e => { e.HasNoKey().ToView(null); });

        base.OnModelCreating(modelBuilder);
    }

    // 1. Main Entities
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }

    // 2. Junction / Relationship Tables
    public DbSet<BranchTrack> BranchTracks { get; set; }
    public DbSet<CourseInstructor> CourseInstructors { get; set; }
    public DbSet<StudentCourse> StudentCourses { get; set; }   // Enrollment

    // 3. Exam Execution / Submission related
    public DbSet<Submission> Submissions { get; set; }        // Exam attempts
    public DbSet<StudentAnswer> StudentAnswers { get; set; }

    // for Sp tables
    public DbSet<CouresExamForInstructor> CouresExamForInstructors { get; set; }
    }
