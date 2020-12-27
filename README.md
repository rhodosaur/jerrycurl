[![NuGet](https://img.shields.io/nuget/v/Jerrycurl)](https://nuget.org/packages/Jerrycurl)
[![Build status](https://ci.appveyor.com/api/projects/status/aihogw33ef50go9r?svg=true)](https://ci.appveyor.com/project/rhodosaur/jerrycurl/branch/master)
[![Test status](https://img.shields.io/appveyor/tests/rhodosaur/jerrycurl/dev)](https://ci.appveyor.com/project/rhodosaur/jerrycurl/branch/master/tests)
[![Gitter chat](https://badges.gitter.im/gitterHQ/gitter.png)](https://gitter.im/jerrycurl-mvc/community)

---

![Jerrycurl](gfx/icon.png)

## Jerrycurl - High-performance ORM and MVC framework for .NET
**Jerrycurl** is an object-relational mapper, MVC framework and Razor SQL implementation that allows developers to build data access for .NET using tools and features inspired by those of ASP.NET.

### Installation
Jerrycurl is available on NuGet and can be installed into any [SDK-style](https://docs.microsoft.com/en-us/nuget/resources/check-project-format) C# project. Its main package contains support for compiling `.cssql` files into your project and executing them via the built-in MVC engine. Additionally you can install support for [one of our supported databases](https://nuget.org/packages?q=Jerrycurl.Vendors) from NuGet as well.

```shell
> dotnet add package Jerrycurl
> dotnet add package Jerrycurl.Vendors.SqlServer
```

#### Tooling
You can generate a basic object model from your database schema by installing and invoking our [CLI](https://www.nuget.org/packages/dotnet-jerry/).
```shell
> dotnet tool install --global dotnet-jerry
> jerry scaffold -v sqlserver -c "SERVER=.;DATABASE=blogdb;TRUSTED_CONNECTION=true" -ns BlogDb.Database
Connecting to database 'blogdb'...
Generating...
Generated 7 tables and 21 columns in Database.cs.
```
To learn more about the CLI, type in `jerry help`.

### MVC design
Jerrycurl features a variant of the model-view-controller pattern made specifically for the relational world and its most prized asset: the SQL language. Each project consists of a selection of models, accessors and queries/commands, as per the CQS pattern.

#### Model layer
The model is a collection of simple data records and provides the structure for interacting with data and metadata on either the object side or the database side. It usually combines a representation of the database in a familiar class-per-table fashion along with whatever customized data views you might need.

You can create views through composition with support for both unary properties (one-to-one mapping) or n-ary lists (one-to-many mapping).

```csharp
// Database.cs
[Table("dbo", "Blog")]
class Blog
{
    [Id, Key("PK_Blog")]
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedOn { get; set; }
}
//...
```
```csharp
// Views/Blogs/BlogView.cs
class BlogView : Blog
{
    public BlogAuthor Author { get; set; }
    public IList<BlogPost> Posts { get; set; }
}
```

#### Command/query layer
Commands and queries are written with our customized Razor SQL syntax and uses `.cssql` as extension, which ensures their compilation into the project in the usual build process. They are placed in either the `Queries` or `Commands` folders based on whether they *read* or *write* data in the underlying database.
```
-- Queries/Blogs/GetAll.cssql
@result BlogView
@model BlogFilter
@{
    var p = R.Open(m => m.Posts);
}

SELECT      @R.Star(),
            @R.Star(m => m.Author)
INNER JOIN  @R.Tbl(m => m.Author) ON @R.Col(m => m.Author.BlogId) = @R.Col(m => m.Id)
FROM        @R.Tbl()
WHERE       @R.Col(m => m.CreatedOn) >= @M.Par(m => m.FromDate)

SELECT      @p.Star()
FROM        @p.Tbl()
INNER JOIN  @R.Tbl() ON @R.Col(m => m.Id) = @p.Col(m => m.BlogId)
WHERE       @R.Col(m => m.CreatedOn) >= @M.Par(m => m.FromDate)
```
```
-- Commands/Blogs/AddBlogs.cssql
@model Blog

@foreach (var v in this.M.Vals())
{
    INSERT INTO @v.TblName() ( @v.In().ColNames() )
    OUTPUT      @v.Out().Cols("INSERTED").As().Props()
    VALUES                   ( @v.In().Pars() )
}
```

#### Accessor (controller) layer
Accessors provide the bridge between the model and command/query layer by passing *input data* to an underlying `.cssql` file and returning the *output data* as either new objects (queries) or modifications to existing objects (commands). 
```csharp
// Accessors/BlogsAccessor.cs
public class BlogsAccessor : Accessor
{
    public IList<BlogView> GetAll(DateTime fromDate) // -> Queries/Blogs/GetAll.cssql
        => this.Query<BlogView>(model: new BlogFilter { FromDate = fromDate });
    
    public void AddBlogs(IList<Blog> newBlogs) // -> Commands/Blogs/AddBlogs.cssql
        => this.Execute(model: newMovies);
}
```

#### Domain (application) layer
A domain should be created in a parent namespace and presents a shared place for adding the requires configuration for all associated commands or queries executions.
```csharp
// BlogsDomain.cs
class BlogsDomain : IDomain
{
    public void Configure(DomainOptions options)
    {
        options.UseSqlServer("SERVER=.;DATABASE=blogdb;TRUSTED_CONNECTION=true");
    }
}
```

#### Usage
To use your newly minted accessor, simply fire up an instance and call one of its methods.

```csharp
var accessor = new BlogsAccessor();
var blogs = accessor.GetAll();
```

To learn more about Jerrycurl and how to get started, visit [our official site](https://jerrycurl.net) or check our [samples repo](https://github.com/rwredding/jerrycurl-samples).

## Building from source
Jerrycurl can be built on [any OS supported by .NET Core](https://docs.microsoft.com/en-us/dotnet/core/install/dependencies) and included in this repository is a [PowerShell script](build.ps1) that performs all build-related tasks.

### Prerequisites
* .NET Core SDK 5.0
* .NET Core Runtime 2.1 / 3.1 (to run tests)
* PowerShell 5.0+ (PowerShell Core on Linux/macOS) 
* Visual Studio 2019 (latest) (optional)
* Docker (optional - for running live database tests)

### Clone, Build and Test
Clone the repository and run our build script from PowerShell.
```powershell
PS> git clone https://github.com/rhodosaur/jerrycurl
PS> cd jerrycurl
PS> .\build.ps1 [-NoTest] [-NoPack]
```

This runs the `Restore`, `Clean`, `Build`, `[Test]` and `[Pack]` targets on `jerrycurl.sln` and places NuGet packages in `/artifacts/packages`. Each target can also be run manually, and in Visual Studio if preferred.

By default, the `Test` target skips any unit test that requires live running database server. To help you to include these, you can run our [`docker compose` script](test/tools/boot-dbs.ps1) to boot up instances of our supported databases.

```powershell
PS> .\test\tools\boot-dbs.ps1 up sqlserver,mysql,postgres,oracle
```

Please allow ~60 seconds for the databases to be ready after which you can re-run `build.ps1`; it will then automatically target the included databases instances. When done, you can tear everything down again.

```powershell
PS> .\test\tools\boot-dbs.ps1 down sqlserver,mysql,postgres,oracle
```

> If you already have an empty database running that can be used for testing, you can manually specify its connection string in the environment variable `JERRY_SQLSERVER_CONN`, `JERRY_MYSQL_CONN`, `JERRY_POSTGRES_CONN` or `JERRY_ORACLE_CONN`.

> Pulling the Oracle Database image requires that you are logged into Docker and have accepted their [terms of service](https://hub.docker.com/_/oracle-database-enterprise-edition).
