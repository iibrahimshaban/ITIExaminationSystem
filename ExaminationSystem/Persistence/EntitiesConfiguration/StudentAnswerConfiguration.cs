using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class StudentAnswerConfiguration : IEntityTypeConfiguration<StudentAnswer>
{
    public void Configure(EntityTypeBuilder<StudentAnswer> builder)
    {
        // Primary Key
        builder.HasKey(sa => sa.Id);

        // Relationships
        builder.HasOne(sa => sa.Submission)
               .WithMany(s => s.Answers)
               .HasForeignKey(sa => sa.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);   // Answers deleted if submission deleted

        builder.HasOne(sa => sa.Question)
               .WithMany(q => q.StudentAnswers)
               .HasForeignKey(sa => sa.QuestionId)
               .OnDelete(DeleteBehavior.Restrict);   // Don't delete answers if question is deleted

        builder.HasOne(sa => sa.SelectedChoice)
               .WithMany()
               .HasForeignKey(sa => sa.SelectedChoiceId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);    // If choice deleted → answer becomes invalid but kept

        // Optional: Index for fast retrieval of answers per submission
        builder.HasIndex(sa => sa.SubmissionId);

        // Optional: Index for per-question statistics (how many chose what)
        builder.HasIndex(sa => new { sa.QuestionId, sa.SelectedChoiceId });
    }
}
