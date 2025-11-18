using System.Text.Json.Serialization;

namespace CareerHub.Api.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    [JsonIgnore]
    public UserAccount? Manager { get; set; }

    public List<UserAccount> Employees { get; set; } = new();
}
