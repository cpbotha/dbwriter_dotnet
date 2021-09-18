// https://docs.microsoft.com/en-us/ef/core/cli/dotnet

// dotnet tool install --global dotnet-ef --prerelease
// # ensure that you can now run just "dotnet ef"
// # create the database
// dotnet ef database update
// # create migrations files in Migrations
// dotnet ef migrations add initialDb
// # apply migrations
// dotnet ef database update

// to use the tool on a project, you also need on the project:
// dotnet add package Microsoft.EntityFrameworkCore.Design --prerelease
// you might also need
// Microsoft.EntityFrameworkCore.Tools --prerelease

// dotnet add package Microsoft.EntityFrameworkCore.sqlite --prerelease
using Microsoft.EntityFrameworkCore;
// for OpenApiInfo
using Microsoft.OpenApi.Models;

// Configure Services =====================================================

var builder = WebApplication.CreateBuilder(args);

//builder.Configuration.GetConnectionString("NameInConnectionStringsSection")
// https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings
builder.Services.AddDbContext<SamplesDbContext>(options => options.UseSqlite("Data Source=bleh.db;Cache=Shared"));

// setup openapi / swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "dotnet 6 minimal api dbwriter API",
    Description = "Simple description",
    Version = "v1"
}));


// Configure =====================================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();


// Routing ===========================================================

app.MapGet("/", () => "Hello World!");

// MapPost will magically bind body into record or class
app.MapPost("/samples", (SamplesDbContext dbContext, Sample sample) => {
    System.Console.WriteLine($"{sample.TimeStamp}");
    dbContext.Samples.Add(sample);
    dbContext.SaveChanges();
    return Results.Created($"/{sample.Id}", sample);
});

app.MapGet("/samples", (SamplesDbContext dbContext) => {
    return dbContext.Samples.ToList();

});

app.MapGet("/samples/{id}", (SamplesDbContext dbContext, int id) => {
    return dbContext.Samples.Find(id) is Sample sample ? Results.Ok(sample) : Results.NotFound();
});


app.UseSwaggerUI();


app.Run();
//await app.RunAsync();

record PostedSample(string Name, int Age);

public class SamplesDbContext : DbContext
{
    public SamplesDbContext(DbContextOptions options) : base(options)
    {
    }

    protected SamplesDbContext()
    {
    }
    // "= default!" means "init to default (null) but act like it's not null" (EF will init)
    // https://headspring.com/2019/12/19/c-sharp-8-entity-framework-patterns/
    public DbSet<Sample> Samples { get; set; } = default!;
}
public class Sample
{
    public int Id { get; set; }
    // following guidance of https://github.com/dotnet/efcore/issues/15520
    // we use = default! here too
    public string Name { get; set; } = default!;
    public DateTime TimeStamp { get; set; }
    public float v0 { get; set; }
    public float v1 { get; set; }
}