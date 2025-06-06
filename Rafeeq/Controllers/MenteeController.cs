// Controllers/MenteeController.cs
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Mentee;
using Rafeeq.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/mentee")]
    [ApiController]
    public class MenteeController : ControllerBase
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly ILogger<MenteeController> _logger;

        public MenteeController(UnitOfWorkManager unitOfWork, ILogger<MenteeController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("{menteeId}/dashboard")]
        public async Task<ActionResult<MenteeDashboardDto>> GetDashboardData(int menteeId)
        {
            try
            {
                if (menteeId <= 0)
                {
                    _logger.LogWarning("Invalid mentee ID: {MenteeId}", menteeId);
                    return BadRequest("Invalid mentee ID");
                }

                var dashboardData = await _unitOfWork.Mentees.GetDashboardDataAsync(menteeId);

                if (dashboardData == null)
                {
                    _logger.LogWarning("Mentee not found with ID: {MenteeId}", menteeId);
                    return NotFound();
                }

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard data for mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}