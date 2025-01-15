using Microsoft.EntityFrameworkCore;

namespace Waracle.Api.Data;

public class WaracleDbContext : DbContext
{
    public WaracleDbContext(DbContextOptions<WaracleDbContext> options) : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Booking> Bookings { get; set; }
}