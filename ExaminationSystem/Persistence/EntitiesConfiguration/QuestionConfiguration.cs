using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        // Primary Key
        builder.HasKey(q => q.Id);

        // Required fields
        builder.Property(q => q.Body)
               .IsRequired()
               .HasMaxLength(2000);  // reasonable for question text

        builder.Property(q => q.Type)
               .IsRequired()
               .HasMaxLength(10);    // enough for "MCQ", "TF", "Essay", etc.

        // Optional fields
        builder.Property(q => q.Points)
               .IsRequired(false);

        builder.Property(q => q.CorrectAnswer)
               .IsRequired(false);   // only for TF, null for MCQ

        // 1. Relationship: Question belongs to one Exam
        builder.HasOne(q => q.Exam)
               .WithMany(e => e.Questions)
               .HasForeignKey(q => q.ExamId)
               .OnDelete(DeleteBehavior.Cascade);  // Questions are deleted if Exam is deleted

        // 2. Choices (cascade delete - choices belong to question)
        builder.HasMany(q => q.Choices)
               .WithOne(c => c.Question)
               .HasForeignKey(c => c.QuestionId)
               .OnDelete(DeleteBehavior.Cascade);

        // 3. StudentAnswers (restrict - don't delete answers if question is deleted)
        builder.HasMany(q => q.StudentAnswers)
               .WithOne(sa => sa.Question)
               .HasForeignKey(sa => sa.QuestionId)
               .OnDelete(DeleteBehavior.Restrict);

        // Optional: Index for faster queries by exam
        builder.HasIndex(q => q.ExamId);
    }
}
