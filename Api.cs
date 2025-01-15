using System.Globalization;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Waracle.Api.Models;
using Waracle.Api.Data;

namespace Waracle.Api;

public static class Api
{
    static readonly CultureInfo culture = new("en-GB");

    public static void MapApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/hotels", async (
            [FromQuery] string searchTerm,
            WaracleDbContext db) =>
        {
            var hotels = await db.Hotels
                .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
                .Where(h => h.Name.Contains(searchTerm))
                .ToListAsync();

            return Results.Ok(ApiResponse<object>.Create(hotels, hotels.Count));
        })
        .WithName("SearchHotels")
        .WithOpenApi(op =>
        {
            op.Summary = "Search for hotels by name";

            var fromDateParam = op.Parameters.First(p => p.Name == "searchTerm");
            fromDateParam.Description = "Hotel name";
            fromDateParam.Example = new OpenApiString("Seaside Resort");

            return op;
        });

        app.MapGet("/available-rooms", async (
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int numberOfGuests,
            WaracleDbContext db) =>
        {
            var availableRooms = await db.Hotels
                .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
                .Select(h => new
                {
                    h.Name,
                    CheckInTime = h.CheckInTimeString,
                    CheckOutTime = h.CheckOutTimeString,
                    Rooms = h.Rooms
                        .Where(r => r.RoomType!.Capacity >= numberOfGuests)
                        .Where(r => !r.Bookings.Any(b => b.FromDate <= toDate.Date
                            && b.ToDate >= fromDate.Date))
                })
                .Where(h => h.Rooms.Any())
                .ToListAsync();

            var totalAvailableRooms = availableRooms.Sum(h => h.Rooms.Count());

            return Results.Ok(ApiResponse<object>.Create(availableRooms, totalAvailableRooms));
        }).WithName("SearchAvailableRooms")
        .WithOpenApi(op =>
        {
            op.Summary = "Search for available rooms by from date, to date and number of guests";

            var fromDateParam = op.Parameters.First(p => p.Name == "fromDate");
            fromDateParam.Description = "Check-in date (format: YYYY-MM-DD)";
            fromDateParam.Example = new OpenApiString("2025-01-14");

            var toDateParam = op.Parameters.First(p => p.Name == "toDate");
            toDateParam.Description = "Check-out date (format: YYYY-MM-DD)";
            toDateParam.Example = new OpenApiString("2025-01-15");

            var guestsParam = op.Parameters.First(p => p.Name == "numberOfGuests");
            guestsParam.Description = "Number of guests staying";
            guestsParam.Example = new OpenApiInteger(2);

            return op;
        });

        _ = app.MapGet("/booking", async (
            [FromQuery] string bookingReference,
            WaracleDbContext db) =>
        {
            var booking = await db.Bookings
                .Where(b => b.Reference == bookingReference)
                .FirstOrDefaultAsync();

            return booking != null ?
                Results.Ok(ApiResponse<object>.Create(booking, 1)) :
                Results.NotFound(ApiResponse<object>.Create(new { message = "Booking not found" }, 0));
        }).WithName("GetBooking")
        .WithOpenApi(op =>
        {
            op.Summary = "Get a booking by reference";

            var referenceParam = op.Parameters.First(p => p.Name == "bookingReference");
            referenceParam.Description = "Booking reference (format: BK-YYYY-XXXX)";
            referenceParam.Example = new OpenApiString("BK-2025-0001");

            return op;
        });

        _ = app.MapPost("/booking", async (
            [FromBody] CreateBookingViewModel booking,
            WaracleDbContext db) =>
        {
            //Endpoint would benefit from fluent validation 
            if (!booking.IsValid())
                return Results.BadRequest(ApiResponse<object>.Create(new { message = "This booking is invalid" }, 0));

            var room = await db.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == booking.RoomId);

            if (room == null)
                return Results.NotFound(ApiResponse<object>.Create(new { message = "Room not found" }, 0));

            if (room.RoomType?.Capacity < booking.NumberOfGuests)
                return Results.BadRequest(ApiResponse<object>.Create(new { message = "Room capacity is less than the number of guests" }, 0));

            var booked = await db.Bookings
                .Where(b => b.RoomId == booking.RoomId)
                .Where(b => b.FromDate.Date < booking.ToDate && 
                            b.ToDate.Date > booking.FromDate)
                .AnyAsync();

            if (booked)
                return Results.BadRequest(ApiResponse<object>.Create(new { message = "Room is already booked" }, 0));

            var lastBooking = await db.Bookings
                .Where(b => b.CreatedAt.Year == DateTime.UtcNow.Year)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            var year = DateTime.UtcNow.Year;
            var lastSequence = int.Parse(lastBooking?.Reference.Split('-').Last() ?? "1") + 1;
            var reference = $"BK-{year}-{lastSequence:D4}";

            var fromDate = booking.FromDate.Date + room.Hotel!.CheckInTime.ToTimeSpan();
            var toDate = booking.ToDate.Date + room.Hotel!.CheckOutTime.ToTimeSpan();

            var newBooking = new Booking
            {
                Reference = reference,
                FromDate = fromDate,
                ToDate = toDate,
                RoomId = room.Id,
                Room = room,
                NumberOfGuests = booking.NumberOfGuests
            };

            await db.Bookings.AddAsync(newBooking);
            await db.SaveChangesAsync();

            return Results.Created($"/booking/{newBooking.Reference}", ApiResponse<object>.Create(newBooking, 1));
        }).WithName("CreateBooking")
        .WithOpenApi(op =>
        {
            op.Summary = "Create a booking";
            op.Description = "Create a booking";
            return op;
        });

        app.MapPost("/seed", async (WaracleDbContext db) =>
        {
            //Should come from a seed file. 
            var existingBookings = await db.Bookings.ToListAsync();
            db.Bookings.RemoveRange(existingBookings);

            var existingHotels = await db.Hotels.ToListAsync();
            db.Hotels.RemoveRange(existingHotels);

            var singleRoomType = new RoomType
            {
                Name = "Single",
                Capacity = 1
            };

            var doubleRoomType = new RoomType
            {
                Name = "Double",
                Capacity = 2
            };

            var deluxeRoomType = new RoomType
            {
                Name = "Deluxe",
                Capacity = 3
            };

            var hotels = new List<Hotel>
            {
                new()
                {
                    Name = "Grand Hotel",
                    CheckInTime = TimeOnly.FromTimeSpan(new TimeSpan(14, 0, 0)),
                    CheckOutTime = TimeOnly.FromTimeSpan(new TimeSpan(12, 0, 0)),
                    Rooms =
                    [
                        new() { Name = "Cozy Corner", RoomType = singleRoomType, Price = 100 },
                        new() { Name = "Peaceful Haven", RoomType = singleRoomType, Price = 100 },
                        new() { Name = "Sunset Suite", RoomType = doubleRoomType, Price = 200 },
                        new() { Name = "Garden View", RoomType = doubleRoomType, Price = 200 },
                        new() { Name = "Royal Penthouse", RoomType = deluxeRoomType, Price = 300 },
                        new() { Name = "Presidential Suite", RoomType = deluxeRoomType, Price = 300 },
                    ]
                },
                new()
                {
                    Name = "Seaside Resort",
                    CheckInTime = TimeOnly.FromTimeSpan(new TimeSpan(14, 0, 0)),
                    CheckOutTime = TimeOnly.FromTimeSpan(new TimeSpan(12, 0, 0)),
                    Rooms =
                    [
                        new() { Name = "Ocean View Single", RoomType = singleRoomType, Price = 120 },
                        new() { Name = "Beach Single", RoomType = singleRoomType, Price = 110 },
                        new() { Name = "Ocean View Double", RoomType = doubleRoomType, Price = 220 },
                        new() { Name = "Beach Double", RoomType = doubleRoomType, Price = 210 },
                        new() { Name = "Luxury Suite", RoomType = deluxeRoomType, Price = 350 },
                        new() { Name = "Beachfront Suite", RoomType = deluxeRoomType, Price = 400 },
                    ]
                }
            };

            await db.Hotels.AddRangeAsync(hotels);
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<object>.Create(hotels, hotels.Count));
        }).WithName("SeedHotelData")
        .WithOpenApi(op =>
        {
            op.Summary = "Seed hotel data";
            op.Description = "Add two hotels with rooms and room types. Hotels will have 6 rooms each. Each room will have one of three room types. Hotel names are Grand Hotel and Seaside Resort.";
            return op;
        });

        app.MapDelete("/seed", async (WaracleDbContext db) =>
        {
            var existingBookings = await db.Bookings.ToListAsync();
            db.Bookings.RemoveRange(existingBookings);

            var existingHotels = await db.Hotels.ToListAsync();
            db.Hotels.RemoveRange(existingHotels);

            await db.SaveChangesAsync();
            return Results.Ok();
        }).WithName("DeleteAllHotels")
        .WithOpenApi(op =>
        {
            op.Summary = "Delete all hotels";
            op.Description = "Delete all hotels and bookings from db";
            return op;
        });
    }
}