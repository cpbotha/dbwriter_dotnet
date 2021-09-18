## Build

Publish this whole thing as a single self-contained binary:

```shell
dotnet publish -r linux-x64 -p:PublishSingleFile=true --self-contained true
```

See https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file

## Dev environment

### Get started with Entity Framework (EF)

When you start working on this thing:

```shell
dotnet tool install --global dotnet-ef --prerelease
# ensure that you can now run just "dotnet ef"
# create / migrate the database
dotnet ef database update
```

When you've changed the database models and you want to update migrations, do:

```shell
# create migrations files in Migrations
dotnet ef migrations add initialDb
# apply migrations
dotnet ef database update
```
See the [EF Core CLI docs](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) for more details.

#### EF packages I needed to add

Note to self: When I started, this is how I added the EF deps:

```shell
dotnet add package Microsoft.EntityFrameworkCore.Design --prerelease
dotnet add package Microsoft.EntityFrameworkCore.Tools --prerelease
dotnet add package Microsoft.EntityFrameworkCore.sqlite --prerelease
```
