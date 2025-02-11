using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using GameLibraryAPI.Data;
using GameLibraryAPI.Dtos;
using GameLibraryAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Database config
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register FluentValidation for DTO validation
builder.Services.AddValidatorsFromAssemblyContaining<CreateGameDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateGameDtoValidator>();

// Enable CORS for frontend access
builder.Services.AddCors();

var app = builder.Build();

// Ensure database is created and apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    db.Database.Migrate();
}

app.UseCors(options =>
    options.Al wAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Website")),
    RequestPath = ""
});

#region API Endpoints

// GET /games - Fetch all games
app.MapGet("games", async (GameDbContext db) => await db.Games.ToListAsync());

// GET /games/{id} - Fetch a specific game
app.MapGet("games/{id}", async (int id, GameDbContext db) =>
{
    var game = await db.Games.FindAsync(id);
    return game is null ? Results.NotFound() : Results.Ok(game);
});

// POST /games - Add a new game
app.MapPost("games", async (CreateGameDto newGame, GameDbContext db, IValidator<CreateGameDto> validator) =>
{
    var validationResult = await validator.ValidateAsync(newGame);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new { message = validationResult.Errors.Select(e => e.ErrorMessage) });
    }

    bool gameExists = await db.Games.AnyAsync(g => g.Name == newGame.Name && g.ReleaseDate.Year == newGame.ReleaseDate.Year);
    if (gameExists)
    {
        return Results.BadRequest(new { message = "Game already exists" });
    }

    var game = new Game
    {
        Name = newGame.Name,
        Genre = newGame.Genre,
        Price = newGame.Price,
        ReleaseDate = newGame.ReleaseDate
    };

    db.Games.Add(game);
    await db.SaveChangesAsync();

    return Results.Created($"/games/{game.Id}", game);
});

// PUT /games/{id} - Update existing game
app.MapPut("games/{id}", async (int id, UpdateGameDto updatedGame, GameDbContext db, IValidator<UpdateGameDto> validator) =>
{
    var validationResult = await validator.ValidateAsync(updatedGame);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new { message = validationResult.Errors.Select(e => e.ErrorMessage) });
    }

    var game = await db.Games.FindAsync(id);
    if (game is null) return Results.NotFound();

    game.Name = updatedGame.Name;
    game.Genre = updatedGame.Genre;
    game.Price = updatedGame.Price;
    game.ReleaseDate = updatedGame.ReleaseDate;

    await db.SaveChangesAsync();
    return Results.Ok(game);
});

// DELETE /games/{id} - Remove game from database
app.MapDelete("games/{id}", async (int id, GameDbContext db) =>
{
    var game = await db.Games.FindAsync(id);
    if (game is null) return Results.NotFound(new { message = "Game not found" });

    db.Games.Remove(game);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Game deleted successfully", deletedGame = game });
});

#endregion

app.Run();
