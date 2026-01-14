using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Required fields with length constraints
        builder.Property(e => e.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(e => e.DurationInMinutes)
               .IsRequired();

        builder.Property(e => e.TotalPoints)
               .IsRequired();




        // 2. Relationship: Exam ← created by → Instructor (using CreatedBy as FK)
        builder.HasOne(e => e.Instructor)
               .WithMany()                              
               .HasForeignKey(e => e.CreatedBy)         
               .OnDelete(DeleteBehavior.SetNull);       
                                                        
                                                        

        // Optional: Add index on CreatedBy for faster queries
        builder.HasIndex(e => e.CreatedBy);

        // Optional: Index for published exams
        builder.HasIndex(e => e.IsPublished);

        // Navigation collections (explicit configuration - optional but good)
        builder.HasMany(e => e.Questions)
               .WithOne(q => q.Exam)
               .HasForeignKey(q => q.ExamId)
               .OnDelete(DeleteBehavior.Cascade);     // Questions should be deleted if exam is deleted

        builder.HasMany(e => e.Submissions)
               .WithOne(s => s.Exam)
               .HasForeignKey(s => s.ExamId)
               .OnDelete(DeleteBehavior.SetNull);     // Submissions deleted if exam deleted
    }
}
