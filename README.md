dotnet 6 "minimal API" with EF Core for writing time-samples to a database

See the blog post [Developer experience setting up a minimal API in Go, C# and
Python at
vxlabs.com](https://vxlabs.com/2021/10/03/dx-minimal-api-go-csharp-python/) for
more information.

See you,
https://charlbotha.com/

## Build

Publish this whole thing as a single self-contained binary:

```shell
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained
```

See https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file

## Dev environment

You need at least .NET 6 (rc.1 at the time of writing) for this example.
### Get started with Entity Framework (EF)

When you start working on this thing:

First create the database in postgresql:

```shell
sudo su postgres
psql < drop_and_create_db.sql
exit
```

Then migrate this examples schema:

```shell
dotnet tool install --global dotnet-ef --prerelease
# ensure that you can now run just "dotnet ef"
# create / migrate the database
dotnet ef database update
```

Try it out with:

```shell
dotnet run
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
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --prerelease
```
but then with postgres (which is still at preview 7 not rc1) started seeing the following error:

```
System.TypeLoadException: Method 'AppendIdentityWhereCondition' in type 'Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal.NpgsqlUpdateSqlGenerator' from assembly 'Npgsql.EntityFrameworkCore.PostgreSQL, Version=6.0.0.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7' does not have an implementation.
```

It seems I am blocked until they release rc1 of the postgresql adapter: https://github.com/npgsql/efcore.pg/issues/1988

I did try rolling the dice with an unstable CI build:
https://www.myget.org/feed/npgsql-unstable/package/nuget/Npgsql.EntityFrameworkCore.PostgreSQL -- but this just resulted in the next error.

This started working with the `20210923T214126+sha.b93cf83d3` CI builds of Npgsql!

... and on 2021-09-25 the official 6.0.0.rc.1 builds were released.

## How I got started

I started from `dotnet new web` and took it from there.

For me as a newbie (to this stack), these two sites were super helpful:

- [David Fowler's Minimal APIs at a glance](https://gist.github.com/davidfowl/ff1addd02d239d2d26f4648a06158727)
- [Anuraj's Open API + Minimal API blog post and example](https://dotnetthoughts.net/openapi-support-for-aspnetcore-minimal-webapi/)
