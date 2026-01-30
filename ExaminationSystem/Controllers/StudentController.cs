using ExaminationSystem.Persistence;
using ExaminationSystem.Services;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExaminationSystem.Web.Controllers
{
    [Authorize(Roles = "StudentRole")]
    // NEW running
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ApplicationDbContext _context;

        public StudentController(IStudentService studentService,ApplicationDbContext context)
        {
            _studentService = studentService;
            _context = context;
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

        public async Task<IActionResult> StartExam(int examId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _studentService.GetStudentByUserIdAsync(userId!);

            // هل امتحن قبل كده؟
            var previousSubmission = await _context.Submissions
                .Include(s => s.Exam)
                .FirstOrDefaultAsync(s =>
                    s.StudentId == student!.Id &&
                    s.ExamId == examId &&
                    s.IsCorrective == false
                );

            if (previousSubmission != null)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] =
                    $"You already took this exam.<br/>" +
                    $"Grade: <b>{previousSubmission.Grade}%</b><br/>" +
                    $"Status: <b>{(previousSubmission.Grade >= 50 ? "Passed 🎉" : "Failed ❌")}</b>";

                return RedirectToAction("Exams");
            }

            var model = await _studentService
                .StartExamWithQuestionsAsync(student.Id, examId);

            return View("SolveExam", model);
        }



        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SubmitExam(SubmitExamVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _studentService.GetStudentByUserIdAsync(userId!);

            if (model.ExamId <= 0)
                return BadRequest("ExamId not received from view");


            var submissionId = await _studentService.SubmitExamUsingSpAsync(
                student!.Id,
                model.ExamId,
                model.Answers
            );

            return RedirectToAction("Result", new { submissionId });
        }


        public async Task<IActionResult> Result(long submissionId)
        {
            var submission = await _studentService
                .GetSubmissionResultAsync(submissionId);

            return View(submission);
        }


    }
}
