using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        // Primary Key
        builder.HasKey(b => b.Id);

        // Required fields
        builder.Property(b => b.Name)
               .IsRequired()
               .HasMaxLength(200);

        // ManagerId is required (you made it non-nullable in class)
        builder.Property(b => b.ManagerId)
               .IsRequired();

        // Relationship: Branch has one Manager (Instructor)
        builder.HasOne(b => b.Manager)
               .WithMany()                           // Instructor has no collection for managed branches
               .HasForeignKey(b => b.ManagerId)
               .OnDelete(DeleteBehavior.Restrict);   // Prevent deleting manager if branch exists

        // Relationship: Branch → Instructors (many instructors in one branch)
        builder.HasMany(b => b.Instructors)
               .WithOne(i => i.Branch)
               .HasForeignKey(i => i.BranchId)
               .OnDelete(DeleteBehavior.Restrict);   // Don't delete instructors when branch deleted

        // Relationship: Branch → Students
        builder.HasMany(b => b.Students)
               .WithOne(s => s.Branch)
               .HasForeignKey(s => s.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Branch → BranchTracks (junction)
        builder.HasMany(b => b.BranchTracks)
               .WithOne(bt => bt.Branch)
               .HasForeignKey(bt => bt.BranchId)
               .OnDelete(DeleteBehavior.Restrict);   // Delete branch-track records if branch deleted

        // Optional: Index for faster branch name search
        builder.HasIndex(b => b.Name)
               .IsUnique();
    }
}