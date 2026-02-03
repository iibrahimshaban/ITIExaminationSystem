using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.Views.Shared.Components.Sidebar
{

    public class SidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
