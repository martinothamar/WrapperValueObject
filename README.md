# WrapperValueObject

![Build](https://github.com/martinothamar/WrapperValueObject/workflows/Build/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/WrapperValueObject.Generator.svg)](https://www.nuget.org/packages/WrapperValueObject.Generator/)

> **Note**

> This library is not actively maintained at the moment, I recommend looking at [SteveDunn/Vogen](https://github.com/SteveDunn/Vogen)

A .NET source generator for creating
* Simple value objects wrapping other type(s), without the hassle of manual `Equals`/`GetHashCode`
* Value objects wrapping math primitives and other types
  * I.e. `[WrapperValueObject(typeof(int))] readonly partial struct MeterLength { }` - the type is implicitly castable to `int`
  * Math and comparison operator overloads are automatically generated
  * `ToString` is generated with formatting options similar to those on the primitive type, i.e. `ToString(string? format, IFormatProvider? provider)` for math types
* Strongly typed ID's
  * Similar to F# `type ProductId = ProductId of Guid`, here it becomes `[WrapperValueObject] readonly partial struct ProductId { }` with a `New()` function similar to `Guid.NewGuid()`

The generator targets .NET Standard 2.0 and has been tested with `netcoreapp3.1` and `net5.0` target frameworks.

Note that record type feature for structs is planned for C# 10, at which point this library might be obsolete.

## Installation

Add to your project file:

```xml
<PackageReference Include="WrapperValueObject.Generator" Version="0.0.1">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

Or install via CLI

```sh
dotnet add package WrapperValueObject.Generator --version 0.0.1
```

This package is a build time dependency only.

## Usage

1. Use the attribute to specify the underlying type.
2. Declare the struct or class with the `partial` keyword.

### Strongly typed ID


```csharp
[WrapperValueObject] readonly partial struct ProductId { }

var id = ProductId.New(); // Strongly typed Guid wrapper, i.e. {1658db8c-89a4-46ea-b97e-8cf966cfb3f1}

Assert.NotEqual(ProductId.New(), id);
Assert.False(ProductId.New() == id);
```

### Money type

```csharp
[WrapperValueObject(typeof(decimal))] readonly partial struct Money { }

Money money = 2m;

var result = money + 2m; // 4.0
var result2 = money + new Money(2m);

Assert.True(result == result2);
Assert.Equal(4m, (decimal)result);
```


### Metric types
```csharp
[WrapperValueObject(typeof(int))]
public readonly partial struct MeterLength 
{
    public static implicit operator CentimeterLength(MeterLength meter) => meter.Value * 100; // .Value is the inner type, in this case int
}

[WrapperValueObject(typeof(int))]
public readonly partial struct CentimeterLength
{
    public static implicit operator MeterLength(CentimeterLength centiMeter) => centiMeter.Value / 100;
}

MeterLength meters = 2;

CentimeterLength centiMeters = meters; // 200

Assert.Equal(200, (int)centiMeters);
```

### Complex types

```csharp
[WrapperValueObject] // Is Guid ID by default
readonly partial struct MatchId { }

[WrapperValueObject("HomeGoals", typeof(byte), "AwayGoals", typeof(byte))]
readonly partial struct MatchResult { }

partial struct Match
{
    public readonly MatchId MatchId { get; }

    public MatchResult Result { get; private set; }

    public void SetResult(MatchResult result) => Result = result;

    public Match(in MatchId matchId)
    {
        MatchId = matchId;
        Result = default;
    }
}

var match = new Match(MatchId.New());

match.SetResult((1, 2)); // Complex types use value tuples underneath, so can be implicitly converted
match.SetResult(new MatchResult(1, 2)); // Or the full constructor

var otherResult = new MatchResult(2, 1);

Debug.Assert(otherResult != match.Result);

match.SetResult((2, 1));
Debug.Assert(otherResult == match.Result);

Debug.Assert(match.MatchId != default);
Debug.Assert(match.Result != default);
Debug.Assert(match.Result.HomeGoals == 2);
Debug.Assert(match.Result.AwayGoals == 1);
```

### Validation 

To make sure only valid instances are created.
The validate function will be called in the generated constructors.

```csharp
[WrapperValueObject] // Is Guid ID by default
readonly partial struct MatchId
{ 
    static partial void Validate(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(id), $"{nameof(id)} must have value");
    }
}

[WrapperValueObject("HomeGoals", typeof(byte), "AwayGoals", typeof(byte))]
readonly partial struct MatchResult 
{ 
    static partial void Validate(byte homeGoals, byte awayGoals)
    {
        if (homeGoals < 0)
            throw new ArgumentOutOfRangeException(nameof(homeGoals), $"{nameof(homeGoals)} value cannot be less than 0");
        if (awayGoals < 0)
            throw new ArgumentOutOfRangeException(nameof(awayGoals), $"{nameof(awayGoals)} value cannot be less than 0");
    }
}
```

## Limitations

* Need .NET 5 SDK (I think) due to source generators
* Does not support nested types
* Limited configuration options in terms of what code is generated

## Related projects and inspiration

* [StronglyTypedId](https://github.com/andrewlock/StronglyTypedId) by @andrewlock

## TODO/under consideration

Further development on this PoC was prompted by this discussion: https://github.com/ironcev/awesome-roslyn/issues/17

* Replace one generic attribute (WrapperValueObject) with two (or more) that cleary identify the usecase. E.g. StronglyTypedIdAttribute, ImmutableStructAttribute, ...
* Support everything that StronglyTypedId supports (e.g. optional generation of JSON converters).
* Bring the documentation to the same level as in the StronglyTypedId project.
* Write tests.
* Create Nuget package.
