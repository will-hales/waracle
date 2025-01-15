using System.Text.Json.Serialization;

namespace Waracle.Api.Data;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Room> Rooms { get; set; } = [];
    [JsonIgnore]
    public TimeOnly CheckInTime { get; set; }
    [JsonIgnore]
    public TimeOnly CheckOutTime { get; set; }

    [JsonPropertyName("checkInTime")]
    public string CheckInTimeString => CheckInTime.ToString("HH:mm tt");
    [JsonPropertyName("checkOutTime")]
    public string CheckOutTimeString => CheckOutTime.ToString("HH:mm tt");
}
