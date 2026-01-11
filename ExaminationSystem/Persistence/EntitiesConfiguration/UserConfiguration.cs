using ExaminationSystem.Abstractions.Consts;
using ExaminationSystem.Entities;

namespace ExaminationSystem.Persistence.EntitiesConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        

        var appUser = new ApplicationUser
        {
            Id = DefaultUsers.AdminId,
            Email = DefaultUsers.AdminEmail,
            NormalizedEmail = DefaultUsers.AdminEmail.ToUpper(),
            UserName = "iibrahim",
            NormalizedUserName = "IIBRAHIM",
            SecurityStamp = DefaultUsers.AdminSequrityStamp,
            ConcurrencyStamp = DefaultUsers.AdminConcurrencyStamp,
            EmailConfirmed = true,
            PasswordHash = DefaultUsers.AdminHashedPassword
        };

        builder.HasData(appUser);

    }
}
