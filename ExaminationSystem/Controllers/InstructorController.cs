using ExaminationSystem.Abstractions.Interfaces.Instructor;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> UnpublishedExams()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exams = await _instructorExamService
                .GetUnpublishedExamsAsync(userId);

            return View(exams);
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

        [HttpGet]
        public async Task<IActionResult> Create([FromQuery]int courseId)
        {
            var vm = await _instructorExamService.PrepareCreateExamAsync(courseId);

            if (vm == null)
                return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExamVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var examId = await _instructorExamService.CreateExamAsync(userId, model);

            return RedirectToAction("UnpublishedExams");
        }




        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AssignExam(InstructorExamDetailsVm model)
        {
            if (!ModelState.IsValid)
            {
                return View("ExamDetails", model);
            }

            var result = await _instructorExamService.GenerateAndAssignRandomExamAsync( model.ExamId,model.NumberOfMCQ,model.NumberOfTrueFalse, model.MaxStudents);

            if (result.IsFailur)
            {
                ModelState.AddModelError("", "No students were assigned.");
                return View("ExamDetails", model);
            }

            TempData["Success"] =
                $"Assigned exam to {result.Value.StudentsProcessed} students.";

            return RedirectToAction("Exams");
        }

        // GET: /Instructor/CreateQuestion?examId=5
        [HttpGet]
        public async Task<IActionResult> CreateQuestion(int examId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Verify exam exists and instructor has access
                var summary = await _instructorExamService.GetExamQuestionsSummaryAsync(examId, userId);
                if (summary == null)
                    return NotFound();

                var model = new CreateQuestionVm
                {
                    ExamId = examId,
                    Type = "MCQ",
                    Points = 1,
                    Choices = new List<CreateChoiceVm>
                    {
                        new CreateChoiceVm { ChoiceLetter = "A" },
                        new CreateChoiceVm { ChoiceLetter = "B" },
                        new CreateChoiceVm { ChoiceLetter = "C" },
                        new CreateChoiceVm { ChoiceLetter = "D" }
                    }
                };

                ViewBag.ExamTitle = summary.ExamTitle;
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // POST: /Instructor/CreateQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(CreateQuestionVm model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Custom validation based on question type
            if (model.Type == "MCQ")
            {
                if (model.Choices == null || model.Choices.Count != 4)
                {
                    ModelState.AddModelError("", "MCQ questions must have exactly 4 choices");
                }
                else
                {
                    var correctCount = model.Choices.Count(c => c.IsCorrect);
                    if (correctCount != 1)
                        ModelState.AddModelError("", "Please select exactly one correct answer");

                    // Validate all choices have text
                    for (int i = 0; i < model.Choices.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(model.Choices[i].Body))
                            ModelState.AddModelError($"Choices[{i}].Body", "Choice text is required");
                    }
                }
            }
            else if (model.Type == "TrueFalse")
            {
                // FIXED: For True/False, clear the Choices list and only validate CorrectAnswer
                model.Choices = null; // Clear choices for True/False questions

                if (!model.CorrectAnswer.HasValue)
                    ModelState.AddModelError("CorrectAnswer", "Please select the correct answer");

                // Remove any validation errors related to Choices for True/False questions
                var choiceErrors = ModelState.Keys.Where(k => k.StartsWith("Choices")).ToList();
                foreach (var key in choiceErrors)
                {
                    ModelState.Remove(key);
                }
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var summary = await _instructorExamService.GetExamQuestionsSummaryAsync(model.ExamId, userId);
                    ViewBag.ExamTitle = summary?.ExamTitle;
                }
                catch { }

                return View(model);
            }

            try
            {
                await _instructorExamService.AddQuestionAsync(model, userId);
                TempData["SuccessMessage"] = "Question added successfully!";
                return RedirectToAction(nameof(ListQuestions), new { examId = model.ExamId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);

                try
                {
                    var summary = await _instructorExamService.GetExamQuestionsSummaryAsync(model.ExamId, userId);
                    ViewBag.ExamTitle = summary?.ExamTitle;
                }
                catch { }

                return View(model);
            }
        }

        // GET: /Instructor/ListQuestions?examId=5
        [HttpGet]
        public async Task<IActionResult> ListQuestions(int examId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var summary = await _instructorExamService.GetExamQuestionsSummaryAsync(examId, userId);
                if (summary == null)
                    return NotFound();

                var questions = await _instructorExamService.GetExamQuestionsAsync(examId, userId);

                ViewBag.Summary = summary;
                return View(questions);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


       

        /// <summary>
        /// POST: Publish the exam
        /// /Instructor/Publish
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int examId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _instructorExamService.PublishExamAsync(examId, userId);
                TempData["SuccessMessage"] = "Exam published successfully! It is now visible to students.";
                return RedirectToAction(nameof(UnpublishedExams));
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You are not authorized to publish this exam.";
                return RedirectToAction(nameof(UnpublishedExams));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(ListQuestions), new { examId });
            }
        }
    }
}
