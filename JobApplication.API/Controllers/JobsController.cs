using JobApplication.Application.DTOs;
using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Features.Jobs.Commands.CreateJob;
using JobApplication.Application.Features.Jobs.Queries.GetAllJobs;
using JobApplication.Application.Features.Jobs.Queries.GetJobById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplication.API.Controllers
{
    /// <summary>
    /// Manages job postings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Gets all job postings.
        /// </summary>
        /// <returns>The list of jobs.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var jobs = await _mediator.Send(new GetAllJobsQuery());
            return Ok(new { jobs });
        }

        /// <summary>
        /// Gets a single job posting by its id.
        /// </summary>
        /// <param name="id">The job id.</param>
        /// <returns>The matching job.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _mediator.Send(new GetJobByIdQuery() { Id = id });
            if (job is null) return NotFound(new
            {
                message = "invalid Id"
            });
            return Ok(new { job });
        }

        /// <summary>
        /// Creates a new job posting for the authenticated recruiter.
        /// </summary>
        /// <param name="createJobDto">The job title and description.</param>
        /// <returns>The id of the newly created job.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var recruiterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var id = await _mediator.Send(new CreateJobCommand() { Title = createJobDto.Title, Description = createJobDto.Description, RecruiterId = recruiterId });

            return Ok(new
            {
                id = id
            });
        }

        /// <summary>
        /// Closes a job posting owned by the authenticated recruiter.
        /// </summary>
        /// <param name="id">The job id.</param>
        /// <returns>No content when the job is closed.</returns>
        [HttpPut("{id}/close")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var recruiterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var job = await _mediator.Send(new CloseJobCommand() { Id = id, RecruiterId = recruiterId });
                if (job is null) return NotFound(new { message = "invalid Id" });
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only close your own job." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
