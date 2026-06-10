using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;

namespace PRMTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager,Admin")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("risk-summary/{projectId}")]
        public async Task<IActionResult> GetRiskSummary(int projectId)
        {
            var summary = await _aiService.GetRiskSummaryAsync(projectId);
            return Ok(new { summary });
        }

        [HttpGet("skill-match/{projectId}/{employeeId}")]
        public async Task<IActionResult> GetSkillMatch(int projectId, int employeeId)
        {
            var matchData = await _aiService.GetSkillMatchAsync(projectId, employeeId);
            return Ok(matchData);
        }

        [HttpPost("skill-search")]
        public async Task<IActionResult> SearchSkills([FromBody] AISearchRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var matches = await _aiService.SearchTeamResourcesAsync(request.ManagerId, request.Query, request.ProjectId);
            return Ok(matches);
        }
    }
}
