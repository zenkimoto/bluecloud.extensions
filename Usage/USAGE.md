# BlueCloud.Extensions Usage Guide

### Table of Contents

* Data Extensions and Embedded Resources
* Lightweight Database Result to SQL Mapper
* Mapping Overrides (Advanced)
* Additional Method Extensions


## Data Extensions and Embedded Resources

### Method Naming

The extension methods have consistent naming.  Methods where you pass in a SQL String will end with _...QueryString()_.  Methods where you pass in an Embedded Resource name will end with _...EmbeddedResource()_.

### Data Extensions

_To be completed..._

### Embedded Resources

Embedded resources are files that get encapsulated as part of your assembly.  For more information, click [here](https://support.microsoft.com/en-us/help/816181/how-to-embed-and-to-access-resources-by-using-visual-c-net-or-visual-c).  Bluecloud Extensions makes extensive use of embedded resources to read your SQL files.

_To be completed..._

## Lightweight Database Result to Object Mapper

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


## Additional Method Extensions

_To be completed..._