# ![Bluecloud](https://cdn.rawgit.com/zenkimoto/bluecloud.extensions/3506f513649403c5d076db9e46e36ba5ca5f0e74/Logo.svg) 

# BlueCloud.Extensions C# Method Extension Library


>[![NuGet](https://img.shields.io/nuget/v/bluecloud.extensions.svg)](https://www.nuget.org/packages/BlueCloud.Extensions)

**BlueCloud.Extensions** is a helpful set of C# extensions used to map database results to object models.  It is a lightweight SQL Result to Object mapper.

## Installation

The bluecloud.extensions Library is available through [NuGet](https://www.nuget.org).  The Bluecloud Extensions NuGet page can be found [here](https://www.nuget.org/packages/BlueCloud.Extensions). 

To install it with package manager:

```
Install-Package bluecloud.extensions -Version 1.0.2
```

To install it with .NET CLI:

```
dotnet add package bluecloud.extensions --version 1.0.2
```

## Getting Started

The following is a basic _"Hello World"_ guide on using the BlueCloud.Extensions library.

Start by using the `BlueCloud.Extensions.Data` namespace.

```
using BlueCloud.Extensions.Data;
```

All data extension methods extends the `IDbConnection`, `IDbCommand` and `IDataReader` interfaces.  As such, this library is database agnostic.

### Decorating Your Objects

Add the `DbField` attribute to all properties that you want to map to a database result query on any plain old C# object. (POCO). Not all object properties need to be annotated with `DbField`, just the ones you want mapped.

```
public class Album
{
    [DbField("AlbumId")]
    public long AlbumId { get; set; }

    [DbField("Title")]
    public string Title { get; set; }

    [DbField("ArtistId")]
    public long ArtistId { get; set; }
}
```

### Mapping The Result Set

Call the GetObjectsFromQueryString() extension method from your connection and pass it in a query string.  It will map the result set to an IEnumerable of model objects with decorated `DbField` attributes.

```
var connection = new SqliteConnection(…);
connection.Open();

…

var albums = connection.GetObjectsFromQueryString<Album>("SELECT AlbumId, Title, ArtistId FROM albums").ToList();

…

connection.Close();
connection.Dispose();

```

Use `albums` like you would any list of objects!  

Replace `SqliteConnection` with a database connection of your choice.


## Documentation and Usage Guide

For more details on using this library, check out the [usage guide](Usage/USAGE.md).

You can find the generated API documentation here: [API Documentation](https://cdn.rawgit.com/zenkimoto/bluecloud.extensions/master/Documentation/html/annotated.html)

## Additional Information

### Frequently Asked Questions

1. **Why Bluecloud Extensions instead of an ORM like EntityFramework?**  _We feel you know your database model best and can write better and more optimized database queries than a framework can generate.  That's why Bluecloud Extensions is not really an ORM (in the traditional sense) but a SQL/database result to Object mapper._
2. **Do I need to know SQL?** _Yes, you need to know SQL in order to use Bluecloud Extensions._
3. **Is this library tested?** _Yes, there's a full battery of tests. Take a look [here](https://github.com/zenkimoto/bluecloud.extensions/tree/master/BlueCloud.Extensions.Tests)._
4. **What is the primary inspiration behind this project?** _This project was inspired by a Java SQL mapper project called [myBatis](http://blog.mybatis.org) (previously iBatis).  It is not a .NET port and only shares the primary concept of a SQL to Object mapper._

### Issues and Bug Reports

If you discover any bugs, feel free to create an issue on GitHub. We also welcome the open-source community to contribute to the project by forking it and issuing pull requests.

 *  [https://github.com/zenkimoto/bluecloud.extensions/issues](https://github.com/zenkimoto/bluecloud.extensions/issues)

### Contributions

If you'd like to contribute, please look [here](CONTRIBUTING.md).


