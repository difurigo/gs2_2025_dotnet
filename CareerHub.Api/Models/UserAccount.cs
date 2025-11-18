using System.Text.Json.Serialization;

namespace CareerHub.Api.Models;

public class UserAccount
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? CareerGoal { get; set; }

    public int? TeamId { get; set; }
    [JsonIgnore]
    public Team? Team { get; set; }

    [JsonIgnore]
    public List<Team> ManagedTeams { get; set; } = new();
}
