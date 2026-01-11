using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        // Primary Key
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Grade)
               .HasPrecision(4, 2);

        // Relationships
        builder.HasOne(s => s.Student)
               .WithMany(st => st.Submissions)
               .HasForeignKey(s => s.StudentId)
               .OnDelete(DeleteBehavior.Cascade);  // Delete submissions if student is deleted

        builder.HasOne(s => s.Exam)
               .WithMany(e => e.Submissions)
               .HasForeignKey(s => s.ExamId)
               .OnDelete(DeleteBehavior.Cascade);  // Delete submissions if exam is deleted

        // Default/index for faster result queries
        builder.HasIndex(s => new { s.StudentId, s.ExamId });
    }
}
