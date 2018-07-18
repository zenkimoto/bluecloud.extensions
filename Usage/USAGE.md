# BlueCloud.Extensions Usage Guide

### Table of Contents

* Data Extensions and Embedded Resources
* Lightweight Database Result to SQL Mapper
* Validate Parameters
* Mapping Overrides (Advanced)
* Custom Caching (Advanced)
* Additional Method Extensions


## Data Extensions and Embedded Resources

### Method Naming

The extension methods have consistent naming.  Methods where you pass in a SQL String will end with _...QueryString()_.  Methods where you pass in an Embedded Resource name will end with _...EmbeddedResource()_.

### DbFieldAttribute

The `DbField` attribute is used to annotate your model classes with the SQL result column names from `IDataReader`.  **Not all** properties in an object need to be annotated with `DbField` attribute, just the ones you want mapped.  In addition, you could create read only calculated properties based on `DbField` mapped properties.

> 💡 **Tip:** You can alias your columns in your SELECT queries to change the `IDataReader` column name result.

> 💡 **Tip:** Model objects do not have to have to be a 1-1 match with database tables.

### IDataReader Method Extensions

_To be completed..._

### IDbCommand Method Extensions

_To be completed..._

### Embedded Resources

Embedded resources are files that get encapsulated as part of your assembly.  For more information about Embedded Resources, click [here](https://support.microsoft.com/en-us/help/816181/how-to-embed-and-to-access-resources-by-using-visual-c-net-or-visual-c).  Bluecloud Extensions makes extensive use of embedded resources to read your SQL files to keep SQL out of your C# code base.  

To create an embedded resource, just simply add a SQL file in your project.  Select the SQL file, go to the properties pane and select _"Embedded Resource"_.  The SQL file will be embedded in the output assembly.  This allows you to write our SQL without worry about string concatenation or special string structures that muddy up the C# code base.

_To be completed..._

## Lightweight Database Result to Object Mapper

Methods related to the mapper will always have the word _"object"_ in the method name to indicate that it is either _"Get"_ retrieving objects or _"ExecuteNonQuery"_ performing some operation on the database.

All extension methods related to the object mapper have a _"QueryString"_ and _"EmbeddedResource"_ versions.  

#### Get Object(s) Extension Methods

`T GetSingleObjectFromQueryString<T>(...)` _Returns a single mapped object from a SQL Query_

`T GetSingleObjectFromEmbeddedResource<T>(...)` _Returns a single mapped object from a SQL Query stored as an embedded resource_

`IEnumerable<T> GetObjectsFromQueryString<T>(...)` _Returns an IEnumerable of mapped objects from a SQL Query_

`IEnumerable<T> GetObjectsFromEmbeddedResource()` _Returns an IEnumerable of mapped objects from a SQL Query stored as an embedded resource_

#### ExecuteNonQuery Extension Methods

`int ExecuteNonQueryStringForObject<T>(...)` _Executes a non query SQL binding parameters from a mapped object_

`int ExecuteNonQueryEmbeddedResourceForObject<T>(...)` _Executes a non query SQL from an embedded resource binding parameters from a mapped object_

`void ExecuteNonQueryStringForObjects<T>(...)` _Executes a non SQL binding parameters from an IEnumerable of mapped objects_

`void ExecuteNonQueryEmbeddedResourceForObjects<T>(...)` _Executes a non SQL from an embedded resource binding parameters from an IEnumerable of mapped objects_

#### Example

_To be completed..._

## Validate Parameters

On all extension methods, there is a validateParameters parameter that validates the mapping between SQL and parameters in the `IDbCommand` object.

By default, validateParameters is set to `false` assuming that code executed is in release mode.

_To be completed..._

## Mapping Overrides (Advanced)

When persisting and hydrating objects from the database, sometimes you need to customize how properties get mapped to the database.  

Some examples include:

 * **Supporting booleans** - _true and false could be mapped to a 1 and 0 respectively as a `NUMBER` type or a "TRUE" or "FALSE" `VARCHAR` string..._
 * **Supporting enumerated types** - _convert enums to strings and back_
 * **Persisting dates in UTC and hydrating them in local time.**

### IDbHydrationOverridable and IDbSerializationOverridable Interfaces

In your model objects, you would need to implement the `IDbHydrationOverridable` and `IDbSerializationOverridable` interfaces.

 * `IDbHydrationOverridable` - Override how model properties get populated from database values.  Example: Converting a 1 or 0 to a boolean model property.
 * `IDbSerializationOverridable` - Override how database fields get populated from model properties.  Example: Converting a boolean true/false value to a NUMBER database type.

 **The `IDbHydrationOverridable` interface has two methods:**

 * `bool ShouldOverridePropertyHydration(string propertyName);` The name of each property the mapper is trying to hydrate will be passed to this method first.  Return `true` if you want to override how a property is being hydrated, otherwise you should always return `false`.
 * `object OverridePropertyHydration(string propertyName, object value);` If a property is marked as overriden from the previous method, the mapper will call this method and allow you to customize hydration.  This is where you would change types, modify values, etc.

 **The `IDbSerializationOverridable` interface also has two methods:**
 
 * `bool ShouldOverridePropertySerialization(string propertyName);` The name of each property the mapper is trying to serialized will be passed to this method first.  Return `true` if you want to override how a property is being serialized, otherwise you should always return `false`.
 * `object OverridePropertySerialization(string propertyName, object value);` If a property is marked as overriden from the previous method, the mapper will call this method and allow you to customize serialization.  This is where you would change types, modify values, etc. 

#### Example

The following is an example of how to override a property mapping.  In the example, we want to override how booleans get stored in the database.  In this case, a NUMBER where 0 represents `false` and a 1 represents `true`.

This test assumes a database table with the following structure:

```
CREATE TABLE boolean_test 
(
	boolean_value NUMBER, 
	long_value NUMBER
)
```

The property mapping overriding code lives in the model object code:

```
public class BooleanTest : IDbHydrationOverridable, IDbSerializationOverridable
{
    [DbField("boolean_value")]
    public bool BooleanValue { get; set; }

    [DbField("long_value")]
    public long LongValue { get; set; }

    public BooleanTest() 
    {
        BooleanValue = true;
        LongValue = 123;
    }

    #region IDbHydrationOverridable

    public bool ShouldOverridePropertyHydration(string propertyName)
    {
        return propertyName == "BooleanValue" ? true : false;
    }

    public object OverridePropertyHydration(string propertyName, object value)
    {
        // This method only gets called when the propertyName is BooleanValue.

        return (long)value == 1 ? true : false;  // Hydrate property value to C# Boolean Type
    }

    #endregion

    #region IDbSerializationOverridable

    public bool ShouldOverridePropertySerialization(string propertyName)
    {
        return propertyName == "BooleanValue" ? true : false;
    }

    public object OverridePropertySerialization(string propertyName, object value)
    {
        // This method only gets called when the propertyName is BooleanValue.

        return (bool)value == true ? 1 : 0; // Persist to the database as a database NUMBER type
    }

    #endregion
}
```

## Custom Caching (Advanced)

BlueCloud.Extensions utilizes [MemoryCache](https://msdn.microsoft.com/en-us/library/system.runtime.caching.memorycache(v=vs.110).aspx) under the covers with a default 2 hour sliding window.  

If you want to change the Timespan of the sliding window to 30 minutes, you can assign a new DefaultCache like so:

```
IDataReaderExtensions.cache = new DefaultCache<List<DbMapping>>(new TimeSpan(0, 30, 0));
```

If you want to create your own cache, create a new class that implements the [`BlueCloud.Extensions.Data.ICacheable`](https://cdn.rawgit.com/zenkimoto/bluecloud.extensions/master/Documentation/html/interface_blue_cloud_1_1_extensions_1_1_cache_1_1_i_cacheable.html) Interface.

_To be completed..._

## Additional Method Extensions

_To be completed..._