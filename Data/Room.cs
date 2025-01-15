using System.Globalization;
using System.Text.Json.Serialization;

namespace Waracle.Api.Data;
public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonIgnore]
    public int HotelId { get; set; }
    [JsonIgnore]
    public Hotel? Hotel { get; set; }
    [JsonIgnore]
    public int RoomTypeId { get; set; }
    public RoomType? RoomType { get; set; }
    [JsonIgnore]
    public decimal Price { get; set; }
    [JsonPropertyName("price")]
    public string FormattedPrice => Price.ToString("C", new CultureInfo("en-GB"));
    [JsonIgnore]
    public List<Booking> Bookings { get; set; } = [];
}
