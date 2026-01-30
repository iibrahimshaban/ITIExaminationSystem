using ExaminationSystem.Abstractions.Interfaces;
using ExaminationSystem.Abstractions.Interfaces.Instructor;
using ExaminationSystem.Entities;
using ExaminationSystem.Persistence;
using ExaminationSystem.Services.Admin;
using ExaminationSystem.Services.Instructor;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddDependacies(builder.Configuration);
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<IUserProvisioningService, UserProvisioningService>();
builder.Services.AddScoped<IInstructorExamService, InstructorExamService>();
var app = builder.Build();

#region Apply Migrations
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying migrations");
}
#endregion

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Admin/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.Run();
