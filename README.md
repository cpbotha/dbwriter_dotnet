dotnet 6 "minimal API" with EF Core for writing time-samples to a database

See the blog post at https://vxlabs.com/ "Developer experience setting up a
minimal API in Go, C# and Python" (should be published before the end of
October)

See you,
https://charlbotha.com/

## Build

Publish this whole thing as a single self-contained binary:

```shell
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained
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
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --prerelease
```
but then with postgres (which is still at preview 7 not rc1) started seeing the following error:

```
System.TypeLoadException: Method 'AppendIdentityWhereCondition' in type 'Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal.NpgsqlUpdateSqlGenerator' from assembly 'Npgsql.EntityFrameworkCore.PostgreSQL, Version=6.0.0.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7' does not have an implementation.
```

It seems I am blocked until they release rc1 of the postgresql adapter: https://github.com/npgsql/efcore.pg/issues/1988

I did try rolling the dice with an unstable CI build:
https://www.myget.org/feed/npgsql-unstable/package/nuget/Npgsql.EntityFrameworkCore.PostgreSQL -- but this just resulted in the next error.

## How I got started

I started from `dotnet new web` and took it from there.

For me as a newbie (to this stack), these two sites were super helpful:

- [David Fowler's Minimal APIs at a glance](https://gist.github.com/davidfowl/ff1addd02d239d2d26f4648a06158727)
- [Anuraj's Open API + Minimal API blog post and example](https://dotnetthoughts.net/openapi-support-for-aspnetcore-minimal-webapi/)
