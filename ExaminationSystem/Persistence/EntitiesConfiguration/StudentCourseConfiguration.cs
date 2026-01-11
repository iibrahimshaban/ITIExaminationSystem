using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class StudentCourseConfiguration : IEntityTypeConfiguration<StudentCourse>
{
    public void Configure(EntityTypeBuilder<StudentCourse> builder)
    {
        // 1. Primary Key
        builder.HasKey(sc => sc.Id);

        // 2. Required fields (if any - most are optional here)
        // Status is often required in practice
        builder.Property(sc => sc.Status)
               .HasMaxLength(50)
               .IsRequired(false);  // ← change to .IsRequired() if you want it mandatory

        // 3. Decimal precision for grades (very important!)
        // decimal(5,2) → allows 0.00 to 100.00 (safe for percentages)
        builder.Property(sc => sc.ExamGrade)
               .HasPrecision(5, 2);

        builder.Property(sc => sc.FinalGrade)
               .HasPrecision(5, 2);

        // 4. Important: Prevent duplicate enrollments
        // (one student cannot enroll in the same course twice)
        builder.HasIndex(sc => new { sc.StudentId, sc.CourseId })
               .IsUnique()
               .HasDatabaseName("IX_StudentCourse_Unique_Enrollment");

        // 5. Relationships
        builder.HasOne(sc => sc.Student)
               .WithMany(s => s.StudentCourses)          // ← assumes Student has ICollection<StudentCourse>
               .HasForeignKey(sc => sc.StudentId)
               .OnDelete(DeleteBehavior.Restrict);        // Delete enrollment if student is deleted

        builder.HasOne(sc => sc.Course)
               .WithMany(c => c.StudentCourses)          // ← assumes Course has ICollection<StudentCourse>
               .HasForeignKey(sc => sc.CourseId)
               .OnDelete(DeleteBehavior.Restrict);        // Delete enrollment if course is deleted

        // 6. Optional: Default value for Certificated
        builder.Property(sc => sc.Certificated)
               .HasDefaultValue(false);

        // 7. Optional indexes for performance (common queries)
        builder.HasIndex(sc => sc.StudentId);
        builder.HasIndex(sc => sc.CourseId);
        builder.HasIndex(sc => sc.Status);               // useful for filtering by status
    }
}
