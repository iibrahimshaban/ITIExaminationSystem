using ExaminationSystem.Services;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExaminationSystem.Web.Controllers
{
    [Authorize(Roles = "StudentRole")]
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
            var student = await _studentService.GetStudentByUserIdAsync(userId!);
            return View(student);
        }

        public async Task<IActionResult> Courses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _studentService.GetStudentByUserIdAsync(userId!);
            return View(await _studentService.GetStudentCoursesAsync(student!.Id));
        }

        public async Task<IActionResult> Exams()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _studentService.GetStudentByUserIdAsync(userId!);
            return View(await _studentService.GetAvailableExamsAsync(student!.Id));
        }

        public async Task<IActionResult> StartExam(int examId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _studentService.GetStudentByUserIdAsync(userId!);

            try
            {
                var model = await _studentService
                    .StartExamWithQuestionsAsync(student!.Id, examId);

                return View("SolveExam", model);
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = ex.Message switch
                {
                    "EXAM_ALREADY_TAKEN" => "You already took this exam and cannot retake it.",
                    "NOT_ENOUGH_QUESTIONS" => "This exam is not ready yet.",
                    _ => "Something went wrong."
                };

                return RedirectToAction("Exams");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExam(SubmitExamVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _studentService.GetStudentByUserIdAsync(userId!);
            if (model.Answers == null || model.Answers.Count == 0)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "❗ Please answer at least one question.";
                return RedirectToAction("Exams");
            }

            try
            {
                var submissionId = await _studentService.SubmitExamUsingSpAsync(
                    student!.Id,
                    model.ExamId,
                    model.Answers
                );

                return RedirectToAction("Result", new { submissionId });
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";

                TempData["ToastMessage"] = ex.Message switch
                {
                    "NO_ANSWERS" =>
                        "❗ You must answer the exam before submitting.",

                    "EXAM_ALREADY_SUBMITTED" =>
                        "❌ You already submitted this exam.",

                    _ =>
                        "⚠️ Something went wrong. Please try again."
                };

                return RedirectToAction("Exams");
            }
        }


        public async Task<IActionResult> Result(long submissionId)
        {
            return View(await _studentService.GetSubmissionResultAsync(submissionId));
        }
    }
}
