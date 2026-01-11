using ExaminationSystem.Abstractions.Consts;
using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.HasData(
            [
            new ApplicationRole() {
                Id = DefaultRoles.Admin.Id,
                Name = DefaultRoles.Admin.Name,
                NormalizedName = DefaultRoles.Admin.Name.ToUpper(),
                ConcurrencyStamp = DefaultRoles.Admin.ConcurrencyStamp
            },
            new ApplicationRole() {
                Id = DefaultRoles.InstructorRole.Id,
                Name = DefaultRoles.InstructorRole.Name,
                NormalizedName = DefaultRoles.InstructorRole.Name.ToUpper(),
                ConcurrencyStamp = DefaultRoles.InstructorRole.ConcurrencyStamp
            },
            new ApplicationRole() {
                Id = DefaultRoles.StudentRole.Id,
                Name = DefaultRoles.StudentRole.Name,
                NormalizedName = DefaultRoles.StudentRole.Name.ToUpper(),
                ConcurrencyStamp = DefaultRoles.StudentRole.ConcurrencyStamp,
                IsDefault = true
            }
            ]);

    }
}
