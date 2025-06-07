using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public TestController(LoyaltyContext context)
        {
            _context = context;
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _context.MembershipPlans
                .Where(p => p.Activo)
                .Select(p => new MembershipPlanDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    PrecioMensual = p.PrecioMensual,
                    EntradasMensuales = p.EntradasMensuales,
                    Nivel = p.Nivel
                })
                .ToListAsync();
            return Ok(plans);
        }

        [HttpGet("memberships/{codCliente}")]
        public async Task<IActionResult> GetMembershipsByCliente(decimal codCliente)
        {
            var memberships = await _context.CustomerMemberships
                .Include(cm => cm.Cliente)
                .Where(cm => cm.CodCliente == codCliente)
                .Select(cm => new CustomerMembershipDto
                {
                    Id = cm.Id,
                    CodCliente = cm.CodCliente,
                    PlanId = cm.PlanId,
                    FechaInicio = cm.FechaInicio,
                    Estado = cm.Estado
                })
                .ToListAsync();
            return Ok(memberships);
        }
    }
}
