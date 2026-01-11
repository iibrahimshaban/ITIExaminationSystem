using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Primary Key
        builder.HasKey(s => s.Id);

        // Required fields
        builder.Property(s => s.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.LastName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.UserId)
               .IsRequired()
               .HasMaxLength(450); // standard for ASP.NET Identity UserId

        
        // Relationships
        builder.HasOne(s => s.Branch)
               .WithMany(b => b.Students)           // if Branch has ICollection<Student> Students
               .HasForeignKey(s => s.BranchId)
               .OnDelete(DeleteBehavior.Restrict);  // don't delete students when branch is deleted

        builder.HasOne(s => s.Track)
               .WithMany(t => t.Students)           // if Track has ICollection<Student>
               .HasForeignKey(s => s.TrackId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        // 1:1 with Identity User
        builder.HasOne(s => s.User)
               .WithOne()                           // no collection on ApplicationUser side
               .HasForeignKey<Student>(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);   // usually cascade - if user deleted → student deleted

        // Optional: if you want to prevent duplicate UserId
        builder.HasIndex(s => s.UserId)
               .IsUnique();

        // Navigation collections (optional but good for clarity)
        builder.HasMany(s => s.StudentCourses)
               .WithOne(sc => sc.Student)
               .HasForeignKey(sc => sc.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Submissions)
               .WithOne(sub => sub.Student)
               .HasForeignKey(sub => sub.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
