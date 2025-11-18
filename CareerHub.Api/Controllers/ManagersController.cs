using CareerHub.Api.Data;
using CareerHub.Api.Dtos;
using CareerHub.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ManagersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public ManagersController(AppDbContext context, IPasswordHasher<UserAccount> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateManager([FromBody] ManagerCreateDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return Conflict(new { message = "Email already registered" });
        }

        var manager = new UserAccount
        {
            Email = dto.Email.ToLowerInvariant(),
            Name = dto.Name,
            Role = UserRole.Manager,
        };
        manager.PasswordHash = _passwordHasher.HashPassword(manager, dto.Password);

        _context.Users.Add(manager);
        await _context.SaveChangesAsync();

        var response = MapToResponse(manager);
        return CreatedAtAction(nameof(GetById), new { id = manager.Id, version = "1.0" }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetManagers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var total = await _context.Users.CountAsync(u => u.Role == UserRole.Manager);
        var items = await _context.Users
            .Where(u => u.Role == UserRole.Manager)
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = items.Select(MapToResponse).ToList();
        var response = new PagedResponse<UserResponseDto>
        {
            Items = dtoList,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Links = BuildPaginationLinks(page, pageSize, total, nameof(GetManagers))
        };
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var manager = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Manager);
        if (manager == null)
        {
            return NotFound();
        }
        return Ok(MapToResponse(manager));
    }

    private UserResponseDto MapToResponse(UserAccount user)
    {
        var dto = new UserResponseDto
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
                new("teams", Url.ActionLink(action: nameof(TeamsController.GetTeams), controller: "Teams", values: new { version = "1.0", managerId = user.Id }) ?? string.Empty, "GET")
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
