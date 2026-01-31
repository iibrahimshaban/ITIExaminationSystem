using ExaminationSystem.Abstractions.Interfaces.Instructor;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExaminationSystem.Controllers
{
    //[Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly IInstructorService _courseService;
        private readonly IInstructorExamService _instructorExamService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorController(
            IInstructorService courseService,
            UserManager<ApplicationUser> userManager,
            IInstructorExamService instructorExamService)
        {
            _courseService = courseService;
            _userManager = userManager;
            _instructorExamService = instructorExamService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var courses = await _courseService.GetInstructorCoursesAsync(userId);

            return View(courses);
        }

        // GET: /Instructor/Course/5
        public async Task<IActionResult> CourseDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            InstructorCourseDetailsVm? model = await _courseService.GetCourseDetailsAsync(user.Id, id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        /// <summary>
        /// Displays all available exams for courses taught by the logged-in instructor
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Exams()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var availableExams = await _instructorExamService.GetAvailableExamsAsync(userId);

            return View(availableExams);
        }

        [HttpGet]
        public async Task<IActionResult> ExamDetails( int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var exam = await _instructorExamService.GetExamDetailsAsync(userId, id);

            if (exam == null)
                return NotFound();

            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AssignExam(InstructorExamDetailsVm model)
        {
            if (!ModelState.IsValid)
            {
                return View("ExamDetails", model);
            }

            var result = await _instructorExamService.GenerateAndAssignRandomExamAsync(
                model.ExamId,
                model.NumberOfMCQ,
                model.NumberOfTrueFalse,
                model.MaxStudents);

            if (result.IsFailur)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to assign exam. Please check question counts."
                );

                // 👇 stay on same page, same exam
                return View("ExamDetails", model);
            }

            return RedirectToAction("Exams");
        }


    }
}
