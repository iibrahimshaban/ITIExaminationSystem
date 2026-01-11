using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        // Primary Key
        builder.HasKey(c => c.Id);

        // Required fields
        builder.Property(c => c.Title)
               .IsRequired()
               .HasMaxLength(200);



        // Relationship: Course → Topics (cascade - topics belong to course)
        builder.HasMany(c => c.Topics)
               .WithOne(t => t.Course)
               .HasForeignKey(t => t.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relationship: Course → CourseInstructors (junction)
        builder.HasMany(c => c.CourseInstructors)
               .WithOne(ci => ci.Course)
               .HasForeignKey(ci => ci.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relationship: Course → Exams
        builder.HasMany(c => c.Exams)
               .WithOne(e => e.Course)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Restrict);     // Prevent deleting course if exams exist

        // Relationship: Course → StudentCourses (enrollments)
        builder.HasMany(c => c.StudentCourses)
               .WithOne(sc => sc.Course)
               .HasForeignKey(sc => sc.CourseId)
               .OnDelete(DeleteBehavior.Cascade);      // Delete enrollments if course deleted
    }
}
