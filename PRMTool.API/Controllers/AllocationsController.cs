using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;

namespace PRMTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AllocationsController : ControllerBase
    {
        private readonly IAllocationService _allocationService;

        public AllocationsController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        /// <summary>Admin: View all active allocations (optionally filtered).</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int? employeeId, [FromQuery] int? projectId)
        {
            var allocations = await _allocationService.GetAllActiveAsync(employeeId, projectId);
            return Ok(allocations);
        }

        /// <summary>Manager/Admin: View active allocations for a specific project.</summary>
        [HttpGet("project/{projectId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetByProject(int projectId)
        {
            var allocations = await _allocationService.GetActiveByProjectAsync(projectId);
            return Ok(allocations);
        }

        /// <summary>Employee: View own allocations by userId from JWT token.</summary>
        [HttpGet("employee/{userId}")]
        [Authorize(Roles = "Employee,Manager,Admin")]
        public async Task<IActionResult> GetMyAllocations(int userId)
        {
            var allocations = await _allocationService.GetByUserIdAsync(userId);
            return Ok(allocations);
        }

        /// <summary>Manager/Admin: Create a new allocation (validates max 100% utilization).</summary>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateAllocation([FromBody] CreateAllocationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var allocation = await _allocationService.CreateAllocationAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { }, allocation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Manager/Admin: End an existing allocation immediately (sets to_date to today).</summary>
        [HttpPost("{id}/end")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> EndAllocation(int id)
        {
            try
            {
                var result = await _allocationService.EndAllocationAsync(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

