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


        // Relationship: Branch has one Manager (Instructor)
        builder.HasOne(b => b.Manager)
               .WithMany()                           // Instructor has no collection for managed branches
               .HasForeignKey(b => b.ManagerId)
               .OnDelete(DeleteBehavior.SetNull);   // Prevent deleting manager if branch exists


        // Relationship: Branch → BranchTracks (junction)
        builder.HasMany(b => b.BranchTracks)
               .WithOne(bt => bt.Branch)
               .HasForeignKey(bt => bt.BranchId)
               .OnDelete(DeleteBehavior.Cascade);   //  Delete branch-track records if branch deleted

        // Optional: Index for faster branch name search
        builder.HasIndex(b => b.Name)
               .IsUnique();
    }
}