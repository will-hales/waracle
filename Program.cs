using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Waracle.Api;
using Waracle.Api.Data;
using Waracle.Api.Models;   

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Waracle Tech Test - Hotel Booking API",
        Version = "v1",
        Description = """
            API for hotel room bookings. Features include:
            - Search for hotels 
            - Search for available rooms based on date range and number of guests
            - Make room bookings
            - Retrieve booking information
            
            Date format for all endpoints: YYYY-MM-DD

            FYI - Data is already seeded. Run delete and seed in the database to reset.
            """,
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Will Hales",
            Email = "willphales09@icloiud.com"
        }
    });
});

builder.Services.AddDbContext<WaracleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Waracle Hotel Booking API v1");
    options.DocumentTitle = "Waracle Hotel Booking API";
    options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
});

app.UseHttpsRedirection();

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();

// Minimal API
// Didn't want to assume architecture
Api.MapApi(app);

// Add global error handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Create(
                new { message = "An unexpected error occurred" }, 
                0
            )
        );
    });
});

app.Run();

