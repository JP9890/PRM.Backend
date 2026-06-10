using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Enums;

namespace PRMTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? department)
        {
            EmployeeStatus? statusFilter = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<EmployeeStatus>(status, true, out var parsed))
                statusFilter = parsed;

            var employees = await _employeeService.GetAllAsync(statusFilter, department);
            return Ok(employees);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            return Ok(employee);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var employee = await _employeeService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _employeeService.UpdateAsync(id, dto);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            return Ok(employee);
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var employee = await _employeeService.DeactivateAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            return Ok(new { message = "Employee deactivated successfully", employee });
        }

        [HttpPost("assign-manager")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignManager([FromBody] AssignManagerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var employee = await _employeeService.AssignManagerAsync(dto);
                return Ok(employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{employeeId}/skills")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSkill(int employeeId, [FromBody] AddSkillDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var skill = await _employeeService.AddSkillAsync(employeeId, dto);
                return Ok(skill);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{employeeId}/skills/{skillId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSkillProficiency(int employeeId, int skillId, [FromBody] UpdateSkillProficiencyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var skill = await _employeeService.UpdateSkillProficiencyAsync(employeeId, skillId, dto);
            if (skill == null)
                return NotFound(new { message = "Skill not found" });

            return Ok(skill);
        }

        [HttpDelete("{employeeId}/skills/{skillId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSkill(int employeeId, int skillId)
        {
            var success = await _employeeService.RemoveSkillAsync(employeeId, skillId);
            if (!success)
                return NotFound(new { message = "Skill not found" });

            return Ok(new { message = "Skill removed successfully" });
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Employee,Manager,Admin")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var employee = await _employeeService.GetByUserIdAsync(userId);
            if (employee == null)
                return NotFound(new { message = "Employee not found for the given user ID." });

            return Ok(employee);
        }
    }
}
