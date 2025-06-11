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

        private int GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
            }
            return userId;
        }

        [HttpGet("{menteeId}/dashboard")]
        public async Task<ActionResult<MenteeDashboardDto>> GetDashboardData()
        {
            try
            {
                var menteeId = GetCurrentUserId();
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
                _logger.LogError(ex, "Error fetching dashboard data for mentee");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}