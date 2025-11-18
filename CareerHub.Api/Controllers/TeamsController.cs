using CareerHub.Api.Data;
using CareerHub.Api.Dtos;
using CareerHub.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeamsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TeamResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeam([FromBody] TeamCreateDto dto)
    {
        var manager = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.ManagerId && u.Role == UserRole.Manager);
        if (manager == null)
        {
            return BadRequest(new { message = "Manager not found" });
        }

        var team = new Team
        {
            Name = dto.Name,
            ManagerId = dto.ManagerId
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        var response = MapToResponse(team);
        return CreatedAtAction(nameof(GetById), new { id = team.Id, version = "1.0" }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TeamResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeams([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? managerId = null)
    {
        var query = _context.Teams.AsQueryable();
        if (managerId.HasValue)
        {
            query = query.Where(t => t.ManagerId == managerId.Value);
        }

        var total = await query.CountAsync();
        var teams = await query
            .Include(t => t.Employees)
            .OrderBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PagedResponse<TeamResponseDto>
        {
            Items = teams.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Links = BuildPaginationLinks(page, pageSize, total, nameof(GetTeams))
        };

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TeamResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var team = await _context.Teams.Include(t => t.Employees).FirstOrDefaultAsync(t => t.Id == id);
        if (team == null)
        {
            return NotFound();
        }
        return Ok(MapToResponse(team));
    }

    private TeamResponseDto MapToResponse(Team team)
    {
        var dto = new TeamResponseDto
        {
            Id = team.Id,
            Name = team.Name,
            ManagerId = team.ManagerId,
            EmployeeIds = team.Employees.Select(e => e.Id).ToList(),
            Links = new List<LinkDto>
            {
                new("self", Url.ActionLink(nameof(GetById), values: new { id = team.Id, version = "1.0" }) ?? string.Empty, "GET"),
                new("employees", Url.ActionLink(action: nameof(EmployeesController.GetEmployees), controller: "Employees", values: new { version = "1.0", teamId = team.Id }) ?? string.Empty, "GET")
            }
        };
        return dto;
    }

    private List<LinkDto> BuildPaginationLinks(int page, int pageSize, int total, string actionName)
    {
        var links = new List<LinkDto>
        {
            new("self", Url.ActionLink(actionName, values: new { page, pageSize, version = "1.0" }) ?? string.Empty, "GET")
        };

        var totalPages = (int)Math.Ceiling((double)total / pageSize);
        if (page < totalPages)
        {
            links.Add(new LinkDto("next", Url.ActionLink(actionName, values: new { page = page + 1, pageSize, version = "1.0" }) ?? string.Empty, "GET"));
        }
        if (page > 1)
        {
            links.Add(new LinkDto("prev", Url.ActionLink(actionName, values: new { page = page - 1, pageSize, version = "1.0" }) ?? string.Empty, "GET"));
        }

        return links;
    }
}
