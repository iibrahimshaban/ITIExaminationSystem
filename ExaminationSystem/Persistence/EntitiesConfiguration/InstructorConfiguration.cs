using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        // Primary Key
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Salary)
               .HasPrecision(8, 2);

        // Required fields
        builder.Property(i => i.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.LastName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.UserId)
               .HasMaxLength(450);

        // Relationships
        builder.HasOne(i => i.Branch)
               .WithMany(b => b.Instructors)        // if Branch has ICollection<Instructor>
               .HasForeignKey(i => i.BranchId)
               .OnDelete(DeleteBehavior.Restrict);  // important - don't cascade

        // 1:1 with Identity User
        builder.HasOne(i => i.User)
               .WithOne()                           // no collection on ApplicationUser
               .HasForeignKey<Instructor>(i => i.UserId)
               .OnDelete(DeleteBehavior.SetNull);   

        // Navigation collection
        builder.HasMany(i => i.CourseInstructors)
               .WithOne(ci => ci.Instructor)
               .HasForeignKey(ci => ci.InstructorId)
               .OnDelete(DeleteBehavior.Cascade);

        // Optional unique index on UserId
        builder.HasIndex(i => i.UserId)
               .IsUnique();
    }
}
