using ExaminationSystem.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.Controllers
{
    [ApiController]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LookupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= STUDENT =================
        [HttpGet("tracks-by-branch/{branchId}")]
        public IActionResult GetTracksByBranch(int branchId)
        {
            var tracks = _context.BranchTracks
                .Where(bt => bt.BranchId == branchId && bt.IsCurrentlyOffered)
                .Select(bt => new
                {
                    id = bt.Track.Id,
                    name = bt.Track.Title
                })
                .Distinct()
                .ToList();

            return Ok(tracks);
        }

        // ================= INSTRUCTOR =================
        [HttpGet("courses-by-branch/{branchId}")]
        public IActionResult GetCoursesByBranch(int branchId)
        {
            var courses = _context.BranchTracks
                .Where(bt => bt.BranchId == branchId && bt.IsCurrentlyOffered)
                .SelectMany(bt => bt.Track.Courses)
                .Where(c => c.IsActive)
                .Distinct()
                .Select(c => new { id = c.Id, name = c.Title })
                .ToList();

            return Ok(courses);
        }

    }
}
