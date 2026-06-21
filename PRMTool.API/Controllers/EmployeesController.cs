using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;

namespace PRMTool.API.Controllers
{
    /// <summary>
    /// Manages resource profiles (employees and managers).
    /// Route kept as /api/employees for backward compatibility with frontend.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IResourceProfileService _resourceProfileService;

        public EmployeesController(IResourceProfileService resourceProfileService)
        {
            _resourceProfileService = resourceProfileService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAll()
        {
            var profiles = await _resourceProfileService.GetAllAsync();
            return Ok(profiles);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetById(int id)
        {
            var profile = await _resourceProfileService.GetByIdAsync(id);
            if (profile == null)
                return NotFound(new { message = "Resource profile not found." });

            return Ok(profile);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeBasicDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var profile = await _resourceProfileService.UpdateEmployeeAsync(id, dto);
            if (profile == null)
                return NotFound(new { message = "Resource profile not found." });

            return Ok(profile);
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var profile = await _resourceProfileService.DeactivateAsync(id);
                if (profile == null)
                    return NotFound(new { message = "Resource profile not found." });

                return Ok(new { message = "Resource deactivated successfully.", profile });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("assign-manager")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignManager([FromBody] AssignManagerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var profile = await _resourceProfileService.AssignManagerAsync(dto);
                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{resourceId}/skills")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> AddSkill(int resourceId, [FromBody] AddSkillDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var loggedInUserId))
                return Unauthorized();

            var targetProfile = await _resourceProfileService.GetByIdAsync(resourceId);
            if (targetProfile == null)
                return NotFound(new { message = "Resource profile not found." });

            if (User.IsInRole("Employee"))
            {
                if (targetProfile.UserId != loggedInUserId)
                    return StatusCode(403, new { message = "Employees can only manage their own skills." });
            }
            else if (User.IsInRole("Manager"))
            {
                if (targetProfile.UserId != loggedInUserId && targetProfile.ManagerId != loggedInUserId)
                    return StatusCode(403, new { message = "Managers can only manage skills for themselves or their direct team members." });
            }

            try
            {
                var skill = await _resourceProfileService.AddSkillAsync(resourceId, dto);
                return Ok(skill);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{resourceId}/skills/{skillId}")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UpdateSkillProficiency(int resourceId, int skillId, [FromBody] UpdateSkillProficiencyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var loggedInUserId))
                return Unauthorized();

            var targetProfile = await _resourceProfileService.GetByIdAsync(resourceId);
            if (targetProfile == null)
                return NotFound(new { message = "Resource profile not found." });

            if (User.IsInRole("Employee"))
            {
                if (targetProfile.UserId != loggedInUserId)
                    return StatusCode(403, new { message = "Employees can only manage their own skills." });
            }
            else if (User.IsInRole("Manager"))
            {
                if (targetProfile.UserId != loggedInUserId && targetProfile.ManagerId != loggedInUserId)
                    return StatusCode(403, new { message = "Managers can only manage skills for themselves or their direct team members." });
            }

            var skill = await _resourceProfileService.UpdateSkillProficiencyAsync(resourceId, skillId, dto);
            if (skill == null)
                return NotFound(new { message = "Skill not found." });

            return Ok(skill);
        }

        [HttpDelete("{resourceId}/skills/{skillId}")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> RemoveSkill(int resourceId, int skillId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var loggedInUserId))
                return Unauthorized();

            var targetProfile = await _resourceProfileService.GetByIdAsync(resourceId);
            if (targetProfile == null)
                return NotFound(new { message = "Resource profile not found." });

            if (User.IsInRole("Employee"))
            {
                if (targetProfile.UserId != loggedInUserId)
                    return StatusCode(403, new { message = "Employees can only manage their own skills." });
            }
            else if (User.IsInRole("Manager"))
            {
                if (targetProfile.UserId != loggedInUserId && targetProfile.ManagerId != loggedInUserId)
                    return StatusCode(403, new { message = "Managers can only manage skills for themselves or their direct team members." });
            }

            var success = await _resourceProfileService.RemoveSkillAsync(resourceId, skillId);
            if (!success)
                return NotFound(new { message = "Skill not found." });

            return Ok(new { message = "Skill removed successfully." });
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Employee,Manager,Admin")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var profile = await _resourceProfileService.GetByUserIdAsync(userId);
            if (profile == null)
                return NotFound(new { message = "Resource profile not found for the given user ID." });

            return Ok(profile);
        }

        // ── Skill/Proficiency Lookup Endpoints ──────────────────

        [HttpGet("skills")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetSkills()
        {
            var skills = await _resourceProfileService.GetSkillsAsync();
            return Ok(skills);
        }

        [HttpGet("skill-categories")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetSkillCategories()
        {
            var categories = await _resourceProfileService.GetSkillCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("proficiency-levels")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetProficiencyLevels()
        {
            var levels = await _resourceProfileService.GetProficiencyLevelsAsync();
            return Ok(levels);
        }
    }
}
