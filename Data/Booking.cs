using System.Text.Json.Serialization;

namespace Waracle.Api.Data;

public class Booking
{
    public int Id { get; set; }
    public required string Reference { get; set; } 
    [JsonIgnore]
    public int RoomId { get; set; }
    [JsonIgnore]
    public Room? Room { get; set; }
    [JsonIgnore]
    public DateTime FromDate { get; set; }
    [JsonIgnore]
    public DateTime ToDate { get; set; }
    public int NumberOfGuests { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("checkIn")]
    public string CheckInString => FromDate.ToString("dd-MM-yyyy HH:mm tt");
    [JsonPropertyName("checkOut")]
    public string CheckOutString => ToDate.ToString("dd-MM-yyyy HH:mm tt");

}