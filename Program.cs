// wrk2 -t10 -c100 -d20s -R2000 http://localhost:5247/samples/1

using Microsoft.EntityFrameworkCore;
// for OpenApiInfo
using Microsoft.OpenApi.Models;

// Configure Services =====================================================

var builder = WebApplication.CreateBuilder(args);

// https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings
// usually folks use to extract this from config: builder.Configuration.GetConnectionString("NameInConnectionStringsSection")
// disabling tracking to see what that does to performance
//builder.Services.AddDbContext<SamplesDbContext>(options => options.UseSqlite("Data Source=bleh.db;Cache=Shared"));

// You can globally disable tracking with:
// .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
builder.Services.AddDbContext<SamplesDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=dbwriter_dotnet;Username=dbwriter;Password=blehbleh"));

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

// instead of running dotnet ef database migrate you can do the following:
// (mashed up https://gist.github.com/davidfowl/ff1addd02d239d2d26f4648a06158727#gistcomment-3896575
//  and https://stackoverflow.com/a/46064116/532513 to come up with this -- keeping here
//  because might be useful later)
using (var serviceScope = app.Services.CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<SamplesDbContext>();
    dbContext.Database.Migrate();
}

// Routing ===========================================================

app.MapGet("/", () => "Hello World!");

// MapPost will magically bind body into record or class
app.MapPost("/samples", async (SamplesDbContext dbContext, PostedSample sample) => {
    // init EF model Sample from PostedSample record
    var dbSample = new Sample {Name = sample.Name, TimeStamp = sample.TimeStamp, v0 = sample.v0, v1 = sample.v1 };
    await dbContext.Samples.AddAsync(dbSample);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/{dbSample.Id}", dbSample);
});

// David Fowler recommends preferring await over returning Task directly:
// https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#prefer-asyncawait-over-directly-returning-task
app.MapGet("/samples", async (SamplesDbContext dbContext) => await dbContext.Samples.ToListAsync());

app.MapGet("/samples/{id}", async (SamplesDbContext dbContext, int id) => {
    var s = await dbContext.Samples.FindAsync(id);

    // Think very carefully if you want to do this in your code:
    // Here I want to ensure that the tracked Sample will not be re-used for subsequent reads
    // (AsNoTracking() is not available on FindAsync()).
    // also, seeing that we only want to pass the sample off to the client, tracking not relevant.
    dbContext.ChangeTracker.Clear();

    // you could use is-pattern-expression like this:
    //return s is Sample sample ? Results.Ok(sample) : Results.NotFound();
    // but we can reframe to re-use the Sample s we already have:
    return s is not null ? Results.Ok(s) : Results.NotFound();
});


app.UseSwaggerUI();

System.Console.WriteLine("Started up...");

//app.Run();
await app.RunAsync();


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
    // Id or SampleId will by convention be the PK
    public int Id { get; set; }
    // following guidance of https://github.com/dotnet/efcore/issues/15520
    // we use = default! here too
    public string Name { get; set; } = default!;
    public DateTime TimeStamp { get; set; }
    public float? v0 { get; set; }
    public float? v1 { get; set; }
}

// use light-weight record to specify shape of POSTed record
record PostedSample(string Name, DateTime TimeStamp, float? v0, float? v1);
