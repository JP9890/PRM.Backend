using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRMTool.Application.Interfaces;

namespace PRMTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        /// <summary>Get the resource dashboard: bench and active team members.</summary>
        [HttpGet("dashboard/{managerId}")]
        public async Task<IActionResult> GetDashboard(int managerId)
        {
            var dashboard = await _managerService.GetDashboardAsync(managerId);
            return Ok(dashboard);
        }

        /// <summary>Get all projects owned by this manager (with computed health status).</summary>
        [HttpGet("projects/{managerId}")]
        public async Task<IActionResult> GetProjects(int managerId)
        {
            var projects = await _managerService.GetManagerProjectsAsync(managerId);
            return Ok(projects);
        }

        /// <summary>Get full project detail: milestones, allocations, risk flags, and health.</summary>
        [HttpGet("projects/{managerId}/{projectId}/detail")]
        public async Task<IActionResult> GetProjectDetail(int managerId, int projectId)
        {
            var detail = await _managerService.GetProjectDetailAsync(managerId, projectId);
            if (detail == null)
                return NotFound(new { message = "Project not found or you are not its manager." });

            return Ok(detail);
        }

        /// <summary>Get all team employees for allocation selection (includes skills and current utilization).</summary>
        [HttpGet("employees/{managerId}")]
        public async Task<IActionResult> GetTeamEmployees(int managerId)
        {
            var employees = await _managerService.GetTeamEmployeesAsync(managerId);
            return Ok(employees);
        }
    }
}
