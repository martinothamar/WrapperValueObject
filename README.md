# WrapperValueObject

![Build](https://github.com/martinothamar/WrapperValueObject/workflows/Build/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/WrapperValueObject.Generator.svg)](https://www.nuget.org/packages/WrapperValueObject.Generator/)

A .NET source generator for creating
* Simple value objects wrapping other type(s), without the hassle of manual `Equals`/`GetHashCode`
* Value objects wrapping math primitives
  * I.e. `[WrapperValueObject(typeof(int))] partial readonly struct MeterLength` - the type is implicitly castable to `int`, and you can create your own math operations
* Strongly typed ID's
  * Similar to F# `type ProductId = ProductId of Guid`, here it becomes `[WrapperValueObject] partial readonly struct ProductId`

Note that record type feature for structs is planned for C# 10, in which cases some of the
use cases this library supports will be easier to achieve without this libray.

## Installation

Add to your project file:

```xml
<PackageReference Include="WrapperValueObject.Generator" Version="0.0.1-alpha04">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

Or install via CLI

```sh
dotnet add package WrapperValueObject.Generator --version 0.0.1-alpha04
```

## Usage

1. Use the attribute to specify the underlying type.
2. Declare the struct or class with the `partial` keyword. (and does not support nested types)

### Simple example

```csharp
[WrapperValueObject(typeof(int))]
public readonly partial struct MeterLength 
{
    public static implicit operator CentimeterLength(MeterLength meter) => meter.Value * 100;
}

[WrapperValueObject(typeof(int))]
public readonly partial struct CentimeterLength
{
    public static implicit operator MeterLength(CentimeterLength centiMeter) => centiMeter / 100;
}

MeterLength meters = 2;

CentimeterLength centiMeters = meters;

Assert.Equal(200, (int)centiMeters);
```

### Full example

```csharp
using System;
using System.Diagnostics;

namespace WrapperValueObject.TestConsole
{
    [WrapperValueObject] // Is Guid by default
    public readonly partial struct MatchId
    {
        public static MatchId New() => Guid.NewGuid();
    }

    [WrapperValueObject("HomeGoals", typeof(byte), "AwayGoals", typeof(byte))]
    public readonly partial struct MatchResult
    {
    }

    public partial struct Match
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

    public static class Program
    {
        static void Main()
        {
            var match = new Match(MatchId.New());

            match.SetResult((1, 2));
            match.SetResult(new MatchResult(1, 2));

            var otherResult = new MatchResult(2, 1);

            Debug.Assert(otherResult != match.Result);

            match.SetResult((2, 1));
            Debug.Assert(otherResult == match.Result);

            Debug.Assert(match.MatchId != default);
            Debug.Assert(match.Result != default);
            Debug.Assert(match.Result.HomeGoals == 2);
            Debug.Assert(match.Result.AwayGoals == 1);
        }
    }
}
```

## TODO/under consideration

Further development on this PoC was prompted by this discussion: https://github.com/ironcev/awesome-roslyn/issues/17

* Replace one generic attribute (WrapperValueObject) with two (or more) that cleary identify the usecase. E.g. StronglyTypedIdAttribute, ImmutableStructAttribute, ...
* Support everything that StronglyTypedId supports (e.g. optional generation of JSON converters).
* Bring the documentation to the same level as in the StronglyTypedId project.
* Write tests.
* Create Nuget package.
