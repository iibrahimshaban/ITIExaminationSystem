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
			var s = await _studentService.GetStudentByUserIdAsync(userId!);
			return View(s);
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

		public async Task<IActionResult> StartExam()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var student = await _studentService.GetStudentByUserIdAsync(userId!);

			try
			{
				var model = await _studentService.StartExamUsingSpAsync(student!.Id);
				return View("SolveExam", model);
			}
			catch (Exception ex)
			{
				TempData["ToastType"] = "error";
				TempData["ToastMessage"] = ex.Message switch
				{
					"NO_EXAMS_ASSIGNED" => "No exams available right now.",
					"NO_QUESTIONS" => "Exam is not ready yet.",
					_ => "Unable to start exam."
				};

				return RedirectToAction("Exams");
			}
		}

		[HttpPost]
		public async Task<IActionResult> SubmitExam(SubmitExamVM model, int duration)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var student = await _studentService.GetStudentByUserIdAsync(userId!);

			try
			{
				var submissionId = await _studentService.SubmitExamUsingSpAsync(
					student!.Id,
					model.ExamId,
					duration,
					model.Answers
				);

				return RedirectToAction("Result", new { submissionId });
			}
			catch (Exception ex)
			{
				TempData["ToastType"] = "error";
				TempData["ToastMessage"] = ex.Message switch
				{
					"NO_ANSWERS" or "NO_VALID_ANSWERS" =>
						"You must answer the exam before submitting.",
					_ =>
						"Something went wrong. Please try again."
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
