using ExaminationSystem.Abstractions.Interfaces;
using ExaminationSystem.Abstractions.Interfaces.Instructor;
using ExaminationSystem.Services.Admin;
using ExaminationSystem.Services.Instructor;

namespace ExaminationSystem;

public static class DependancyInjection
{
    public static IServiceCollection AddDependacies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbContextConfiguration(configuration)
            .AddIdentityConfiguration()
            .AddServiceRegistration();
        return services;
    }

    private static IServiceCollection AddDbContextConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Default Connection is not found");

        services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

        return services;
    }

    private static IServiceCollection AddServiceRegistration(this IServiceCollection services)
    {

        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IUserProvisioningService, UserProvisioningService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<IInstructorExamService, InstructorExamService>();

		

		return services;
    }
    private static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
    
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        });
        return services;
    }
}


