using ExaminationSystem.Abstractions.Consts;


namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        //default data 
        builder.HasData(
            new IdentityUserRole<string>
            {
                RoleId = DefaultRoles.Admin.Id,
                UserId = DefaultUsers.AdminId
            }
            );
    }
}
