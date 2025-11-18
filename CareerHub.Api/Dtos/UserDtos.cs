using System.ComponentModel.DataAnnotations;
using CareerHub.Api.Models;

namespace CareerHub.Api.Dtos;

public record ManagerCreateDto(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(3)] string Name,
    [property: Required, MinLength(6)] string Password);

public record TeamCreateDto(
    [property: Required] string Name,
    [property: Required] int ManagerId);

public record EmployeeCreateDto(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(3)] string Name,
    [property: Required, MinLength(6)] string Password,
    [property: Required] int TeamId,
    string? CareerGoal);

public record CareerGoalUpdateDto([property: Required] string CareerGoal);

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password);

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? CareerGoal { get; set; }
    public int? TeamId { get; set; }
    public List<LinkDto> Links { get; set; } = new();
}

public class TeamResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public List<int> EmployeeIds { get; set; } = new();
    public List<LinkDto> Links { get; set; } = new();
}

public record AuthResponse(string Token, DateTime ExpiresAt);
