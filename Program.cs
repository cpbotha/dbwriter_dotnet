// wrk -t10 -c100 -d20s -R1000 http://localhost:5247/samples/1

using Microsoft.EntityFrameworkCore;
// for OpenApiInfo
using Microsoft.OpenApi.Models;

// Configure Services =====================================================

var builder = WebApplication.CreateBuilder(args);

// https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings
// usually folks use to extract this from config: builder.Configuration.GetConnectionString("NameInConnectionStringsSection")
// disabling tracking to see what that does to performance
//builder.Services.AddDbContext<SamplesDbContext>(options => options.UseSqlite("Data Source=bleh.db;Cache=Shared"));

builder.Services.AddDbContext<SamplesDbContext>(options => options.UseNpgsql("Host=localhost;Database=dbwriter_dotnet;Username=dbwriter;Password=blehbleh"));

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

// simplified version of https://gist.github.com/davidfowl/ff1addd02d239d2d26f4648a06158727#gistcomment-3896575
// here I just want to create or migrate the database if necessary so standalone server works
//var dbContext = app.Services.GetRequiredService<SamplesDbContext>();
//dbContext.Database.Migrate();

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

app.MapGet("/samples/{id}", async (SamplesDbContext dbContext, int id) => {
    var s = await dbContext.Samples.FindAsync(id);
    // try to make sure that the tracked Sample will not be re-used for subsequent reads
    dbContext.ChangeTracker.Clear();
    return s is Sample sample ? Results.Ok(sample) : Results.NotFound();
});


app.UseSwaggerUI();

System.Console.WriteLine("Started up...");

//app.Run();
await app.RunAsync();

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