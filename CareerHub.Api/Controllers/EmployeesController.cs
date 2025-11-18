using CareerHub.Api.Data;
using CareerHub.Api.Dtos;
using CareerHub.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public EmployeesController(AppDbContext context, IPasswordHasher<UserAccount> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto dto)
    {
        var team = await _context.Teams.Include(t => t.Manager).FirstOrDefaultAsync(t => t.Id == dto.TeamId);
        if (team == null)
        {
            return BadRequest(new { message = "Team not found" });
        }

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return Conflict(new { message = "Email already registered" });
        }

        var employee = new UserAccount
        {
            Email = dto.Email.ToLowerInvariant(),
            Name = dto.Name,
            Role = UserRole.Employee,
            CareerGoal = dto.CareerGoal,
            TeamId = dto.TeamId
        };
        employee.PasswordHash = _passwordHasher.HashPassword(employee, dto.Password);

        _context.Users.Add(employee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id, version = "1.0" }, MapToResponse(employee));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? teamId = null)
    {
        var query = _context.Users.Where(u => u.Role == UserRole.Employee);
        if (teamId.HasValue)
        {
            query = query.Where(e => e.TeamId == teamId.Value);
        }

        var total = await query.CountAsync();
        var employees = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PagedResponse<UserResponseDto>
        {
            Items = employees.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Links = BuildPaginationLinks(page, pageSize, total, nameof(GetEmployees))
        };
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Employee);
        if (employee == null)
        {
            return NotFound();
        }
        return Ok(MapToResponse(employee));
    }

    [HttpPut("{id:int}/goal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCareerGoal(int id, [FromBody] CareerGoalUpdateDto dto)
    {
        var employee = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Employee);
        if (employee == null)
        {
            return NotFound();
        }

        employee.CareerGoal = dto.CareerGoal;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private UserResponseDto MapToResponse(UserAccount user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            CareerGoal = user.CareerGoal,
            TeamId = user.TeamId,
            Links = new List<LinkDto>
            {
                new("self", Url.ActionLink(nameof(GetById), values: new { id = user.Id, version = "1.0" }) ?? string.Empty, "GET"),
                new("team", Url.ActionLink(action: nameof(TeamsController.GetById), controller: "Teams", values: new { id = user.TeamId, version = "1.0" }) ?? string.Empty, "GET")
            }
        };
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
