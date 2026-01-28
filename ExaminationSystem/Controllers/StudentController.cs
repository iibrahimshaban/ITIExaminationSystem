using ExaminationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExaminationSystem.Web.Controllers
{
    [Authorize(Roles = "StudentRole")]
    // NEW
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var student = await _studentService.GetStudentByUserIdAsync(userId);

            if (student == null)
                return NotFound("Student profile not found");

            return View(student);
        }

        public async Task<IActionResult> Courses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _studentService.GetStudentByUserIdAsync(userId!);

            var courses = await _studentService.GetStudentCoursesAsync(student!.Id);

            return View(courses);
        }

        public async Task<IActionResult> Exams()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _studentService.GetStudentByUserIdAsync(userId!);

            var exams = await _studentService.GetAvailableExamsAsync(student!.Id);

            return View(exams);
        }


    }
}
