using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.Controllers
{
    public class InstructorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
