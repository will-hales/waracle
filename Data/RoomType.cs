using System.Text.Json.Serialization;

namespace Waracle.Api.Data;

public class RoomType
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
 