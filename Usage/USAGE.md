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

_To be completed..._

## Additional Method Extensions

_To be completed..._