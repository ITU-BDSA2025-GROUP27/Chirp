using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string dbPath = "../../data/chirp_cli_db.csv";
var db = CSVDatabase<Cheep>.Instance(dbPath);

app.MapGet("/", () => "Hello World!");

app.MapGet("/cheeps", () =>
{
    var cheeps = db.Read();
    return Results.Ok(cheeps);
});

app.MapPost("/cheep", (Cheep cheep) =>
{
    db.Store(cheep);
    return Results.Ok();
});

app.Run();

public record Cheep(string Author, string Message, long Timestamp);
