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
    [Authorize(Roles = "Admin")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projectService.GetAllAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var project = await _projectService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var project = await _projectService.UpdateAsync(id, dto);
                if (project == null)
                    return NotFound(new { message = "Project not found" });

                return Ok(project);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{projectId}/milestones")]
        public async Task<IActionResult> AddMilestone(int projectId, [FromBody] CreateMilestoneDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var milestone = await _projectService.AddMilestoneAsync(projectId, dto);
                return Ok(milestone);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{projectId}/milestones/{milestoneId}/status")]
        public async Task<IActionResult> UpdateMilestoneStatus(int projectId, int milestoneId, [FromBody] UpdateMilestoneStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var milestone = await _projectService.UpdateMilestoneStatusAsync(projectId, milestoneId, dto);
            if (milestone == null)
                return NotFound(new { message = "Milestone not found" });

            return Ok(milestone);
        }
    }
}
