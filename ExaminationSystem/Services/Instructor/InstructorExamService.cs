using ExaminationSystem.Abstractions.Interfaces.Instructor;
using ExaminationSystem.Abstractions.ResultPattern;
using ExaminationSystem.Errors;
using ExaminationSystem.Persistence;
using ExaminationSystem.ViewModel;
using Microsoft.Data.SqlClient;

namespace ExaminationSystem.Services.Instructor
{
    public class InstructorExamService : IInstructorExamService
    {
        private readonly ApplicationDbContext _context;

        public InstructorExamService(ApplicationDbContext context)
        {
            _context = context;
        }

        

        /// <summary>
        /// Publishes an exam after validating requirements
        /// </summary>
        public async Task PublishExamAsync(int examId, string instructorUserId)
        {
            // 1. Validate instructor owns the exam
            var exam = await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseInstructors)
                .Include(e => e.Questions.Where(q => q.IsActive))
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new InvalidOperationException("Exam not found");

           
            // 2. Check if already published
            if (exam.IsPublished)
                throw new InvalidOperationException("This exam is already published");

            // 3. Validate question requirements
            var mcqCount = exam.Questions.Count(q => q.Type == "MCQ");
            var trueFalseCount = exam.Questions.Count(q => q.Type == "TrueFalse");

            if (mcqCount < 7)
                throw new InvalidOperationException($"Cannot publish exam. Minimum 7 MCQ questions required (current: {mcqCount})");

            if (trueFalseCount < 3)
                throw new InvalidOperationException($"Cannot publish exam. Minimum 3 True/False questions required (current: {trueFalseCount})");

            // 4. Validate that all MCQ questions have exactly 4 choices with one correct answer
            var mcqQuestions = exam.Questions.Where(q => q.Type == "MCQ").ToList();
            foreach (var question in mcqQuestions)
            {
                var choices = await _context.Choices
                    .Where(c => c.QuestionId == question.Id)
                    .ToListAsync();

                if (choices.Count != 4)
                    throw new InvalidOperationException($"Question '{question.Body.Substring(0, Math.Min(50, question.Body.Length))}...' must have exactly 4 choices");

                var correctChoicesCount = choices.Count(c => c.IsCorrect);
                if (correctChoicesCount != 1)
                    throw new InvalidOperationException($"Question '{question.Body.Substring(0, Math.Min(50, question.Body.Length))}...' must have exactly one correct answer");
            }

            // 5. Validate that all True/False questions have a correct answer
            var trueFalseQuestions = exam.Questions.Where(q => q.Type == "TrueFalse").ToList();
            foreach (var question in trueFalseQuestions)
            {
                if (!question.CorrectAnswer.HasValue)
                    throw new InvalidOperationException($"True/False question '{question.Body.Substring(0, Math.Min(50, question.Body.Length))}...' must have a correct answer");
            }

            // 6. Publish the exam
            exam.IsPublished = true;

            // Optional: Set publish date if you have that field
            // exam.PublishedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task<List<InstructorExamVm>> GetUnpublishedExamsAsync(string instructorUserId)
        {
            var instructorId = await _context.Instructors
                .Where(i => i.UserId == instructorUserId)
                .Select(i => i.Id)
                .FirstOrDefaultAsync();

            if (instructorId == 0)
                return new();

            return await _context.Exams
                .Where(e =>
                    !e.IsPublished &&
                    _context.CourseInstructors.Any(ci =>
                        ci.CourseId == e.CourseId &&
                        ci.InstructorId == instructorId &&
                        !ci.HadLeft
                    )
                )
                .Select(e => new InstructorExamVm
                {
                    ExamId = e.Id,
                    CourseId = e.CourseId!.Value,
                    CourseTitle = e.Course.Title,
                    ExamTitle = e.Title,
                    DurationInMinutes = e.DurationInMinutes,
                    TotalPoints = e.TotalPoints,
                    IsPublished = e.IsPublished,

                    MCQCount = e.Questions.Count(q => q.Type == "MCQ"),
                    TrueFalseCount = e.Questions.Count(q => q.Type == "TrueFalse")
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<InstructorAvailableExamVm>> GetAvailableExamsAsync(string instructorUserId)
        {
            if (string.IsNullOrWhiteSpace(instructorUserId))
            {
                return new List<InstructorAvailableExamVm>();
            }

            var userIdParam = new SqlParameter("@InstructorUserId", instructorUserId);

            var exams = await _context.Database
                .SqlQueryRaw<InstructorAvailableExamVm>(
                    "EXEC dbo.FindAvilableCourseAndExams @InstructorUserId",
                    userIdParam)
                .ToListAsync();

            return exams;
        }

        // ================================
        // Prepare Create Exam View
        // ================================
        public async Task<CreateExamVm?> PrepareCreateExamAsync(int courseId)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return null;

            return new CreateExamVm
            {
                CourseId = course.Id,
                CourseTitle = course.Title
            };
        }

        // ================================
        // Create Exam
        // ================================
        public async Task<int> CreateExamAsync(string instructorUserId, CreateExamVm model)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == instructorUserId);

            if (instructor == null)
                throw new UnauthorizedAccessException("Instructor not found.");

            var exam = new Exam
            {
                CourseId = model.CourseId,
                Title = model.Title,
                DurationInMinutes = model.DurationInMinutes,
                TotalPoints = model.TotalPoints,
                IsRandomized = model.IsRandomized,
                IsPublished = false,
                CreatedBy = instructor.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            return exam.Id;
        }


        public async Task<Result<ExamAssignmentResultVm>> GenerateAndAssignRandomExamAsync(
       int examId,
       int numberOfMCQ,
       int numberOfTrueFalse,
       string? userId,
       int maxStudents = 20,
       CancellationToken cancellationToken = default)
        {
            var results = await _context.Set<ExamAssignmentResultVm>()
                .FromSqlRaw(
                    "EXEC dbo.AssignRandomQuestionsToAllStudents @ExamId, @MCQCount, @TrueFalseCount,@userId,@MaxStudents",
                    new SqlParameter("@ExamId", examId),
                    new SqlParameter("@MCQCount", numberOfMCQ),
                    new SqlParameter("@TrueFalseCount", numberOfTrueFalse),
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@MaxStudents", maxStudents)

                )
                .AsNoTracking()
                .ToListAsync(cancellationToken);  

            var result = results.FirstOrDefault();

            if (result == null || result.StudentsProcessed == 0)
            {
                return Result.Failure<ExamAssignmentResultVm>(InstructorErrors.InvalidSp);
            }

            return Result.Success(result);
        }




        public async Task<InstructorExamDetailsVm?> GetExamDetailsAsync(string instructorUserId,int examId)
        {
            

            // 2️⃣ Load exam details via EF Core
            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                return null;

            // 3️⃣ Map to ViewModel
            return new InstructorExamDetailsVm
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                CourseTitle = exam.Course.Title,
                DurationInMinutes = exam.DurationInMinutes,

                TotalQuestions = exam.Questions.Count,
                MCQ_Count = exam.Questions.Count(q => q.Type == "MCQ"),
                TF_Count = exam.Questions.Count(q => q.Type == "TrueFalse"),

                // defaults for form
                NumberOfMCQ = 7,
                NumberOfTrueFalse = 3,
                MaxStudents = 20
            };
        }

        public async Task AddQuestionAsync(CreateQuestionVm model, string instructorUserId)
        {
            // 1. Validate that the exam exists and belongs to the instructor
            var exam = await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseInstructors)
                .FirstOrDefaultAsync(e => e.Id == model.ExamId);

            if (exam == null)
                throw new InvalidOperationException("Exam not found");

            //var isInstructor = exam.Course.CourseInstructors
            //    .Any(ci => ci.InstructorId == instructorUserId);

            //if (!isInstructor)
            //    throw new UnauthorizedAccessException("You are not authorized to add questions to this exam");

            // 2. Validate question type
            if (model.Type != "MCQ" && model.Type != "TrueFalse")
                throw new InvalidOperationException("Invalid question type");

            // 3. Validate MCQ-specific rules
            if (model.Type == "MCQ")
            {
                if (model.Choices == null || model.Choices.Count != 4)
                    throw new InvalidOperationException("MCQ questions must have exactly 4 choices");

                var correctChoicesCount = model.Choices.Count(c => c.IsCorrect);
                if (correctChoicesCount != 1)
                    throw new InvalidOperationException("MCQ questions must have exactly one correct answer");

                // Validate choice letters
                var expectedLetters = new[] { "A", "B", "C", "D" };
                var providedLetters = model.Choices.Select(c => c.ChoiceLetter).OrderBy(l => l).ToArray();
                if (!expectedLetters.SequenceEqual(providedLetters))
                    throw new InvalidOperationException("MCQ choices must have letters A, B, C, D");
            }

            // 4. Validate True/False rules
            if (model.Type == "TrueFalse")
            {
                if (!model.CorrectAnswer.HasValue)
                    throw new InvalidOperationException("True/False questions must have a correct answer");
            }

            // 5. Create the Question entity
            var question = new Question
            {
                ExamId = model.ExamId,
                Body = model.Body,
                Type = model.Type,
                Points = model.Points,
                IsActive = true,
                CorrectAnswer = model.Type == "TrueFalse" ? model.CorrectAnswer : null
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // 6. Create Choices if MCQ
            if (model.Type == "MCQ" && model.Choices != null)
            {
                var choices = model.Choices.Select((choice, index) => new Choice
                {
                    QuestionId = question.Id,
                    Body = choice.Body,
                    ChoiceLetter = choice.ChoiceLetter,
                    DisplayOrder = Array.IndexOf(new[] { "A", "B", "C", "D" }, choice.ChoiceLetter) + 1,
                    IsCorrect = choice.IsCorrect
                }).ToList();

                _context.Choices.AddRange(choices);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ExamQuestionsSummaryVm?> GetExamQuestionsSummaryAsync(int examId, string instructorUserId)
        {
            // Validate instructor owns the exam
            var exam = await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseInstructors)
                .Include(e => e.Questions.Where(q => q.IsActive))
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                return null;

            //var isInstructor = exam.Course.CourseInstructors
            //    .Any(ci => ci.InstructorId == instructorUserId);

            //if (!isInstructor)
            //    throw new UnauthorizedAccessException("You are not authorized to view this exam");

            var mcqCount = exam.Questions.Count(q => q.Type == "MCQ");
            var trueFalseCount = exam.Questions.Count(q => q.Type == "TrueFalse");

            return new ExamQuestionsSummaryVm
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                MCQCount = mcqCount,
                TrueFalseCount = trueFalseCount
            };
        }

        public async Task<List<QuestionListVm>> GetExamQuestionsAsync(int examId, string instructorUserId)
        {
            // Validate instructor owns the exam
            var exam = await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseInstructors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new InvalidOperationException("Exam not found");

            //var isInstructor = exam.Course.CourseInstructors
            //    .Any(ci => ci.InstructorId == instructorUserId);

            //if (!isInstructor)
            //    throw new UnauthorizedAccessException("You are not authorized to view this exam");

            // Get all active questions with choices
            var questions = await _context.Questions
                .Where(q => q.ExamId == examId && q.IsActive)
                .Include(q => q.Choices)
                .OrderBy(q => q.Id)
                .Select(q => new QuestionListVm
                {
                    Id = q.Id,
                    Body = q.Body,
                    Type = q.Type,
                    Points = q.Points ?? 0,
                    CorrectAnswer = q.CorrectAnswer,
                    Choices = q.Choices
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new ChoiceDisplayVm
                        {
                            ChoiceLetter = c.ChoiceLetter ?? "",
                            Body = c.Body,
                            IsCorrect = c.IsCorrect
                        }).ToList()
                })
                .ToListAsync();

            return questions;
        }

    }
}
