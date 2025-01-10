# JsonCraft.JsonPath

A lightweight .NET library for querying JSON documents using JSONPath expressions with System.Text.Json.

This implementation is inspired by and based on the JSONPath implementation in [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json), ported to work with System.Text.Json.

## Installation

The package is available on [NuGet](https://www.nuget.org/packages/JsonCraft.JsonPath).

Using the .NET CLI:
```sh
dotnet add package JsonCraft.JsonPath
```

## Usage

The library provides extension methods for `JsonElement` and `JsonDocument` to query JSON using JSONPath expressions:

```csharp
using System.Text.Json;
using JsonCraft.JsonPath;

// Parse JSON
string json = @"{
    ""store"": {
        ""book"": [
            {
                ""category"": ""reference"",
                ""author"": ""Nigel Rees"",
                ""title"": ""Sayings of the Century"",
                ""price"": 8.95
            },
            {
                ""category"": ""fiction"", 
                ""author"": ""Evelyn Waugh"",
                ""title"": ""Sword of Honour"",
                ""price"": 12.99
            }
        ]
    }
}";

JsonDocument document = JsonDocument.Parse(json);

// Select single token
JsonElement? title = document.SelectToken("$.store.book[0].title");
Console.WriteLine(title?.GetString()); // "Sayings of the Century"

// Select multiple tokens
IEnumerable<JsonElement> authors = document.SelectTokens("$.store.book[*].author");
foreach(var author in authors) {
    Console.WriteLine(author.GetString());
}

// Query with filter
var expensiveBooks = document.SelectTokens("$.store.book[?(@.price > 10)]");
```

## Supported JSONPath Syntax

| Operator | Description | Example | 
|----------|-------------|---------|
| `$` | Root object/element | `$` |
| `.` | Child operator | `$.store.book` |
| `..` | Recursive descent | `$..author` |
| `*` | Wildcard | `$.store.*` |
| `[n]` | Array index | `$.store.book[0]` |
| `[n,m]` | Multiple array indices | `$.store.book[0,1]` |
| `['n']` | Object property | `$.store['book']` |
| `['n', 'm']` | Multiple object properties | `$.store['book', 'bicycle']` |
| `[start:end:step]` | Array slice | `$.store.book[0:2]` |
| `[?(expression)]` | Filter expression | `$.store.book[?(@.price < 10)]` |
| `@` | Current object in filter | `[?(@.price > 10)]` |
| `==`,`!=` | Equality operators | `[?(@.category == 'fiction')]` |
| `>`,`>=`,`<`,`<=` | Comparison operators | `[?(@.price > 10)]` |
| `&&`, `||` | Logical AND/OR | `[?(@.price > 10 && @.category == 'fiction')]` |
| `=~` | Regular expression match | `[?(@.author =~ /.*Waugh/)]` |

## API Reference

| Method | Parameters | Return Type | Description |
|--------|------------|-------------|-------------|
| `SelectToken` | `string path` | `JsonElement?` | Selects a single token using a JSONPath expression. Returns null if no match is found. |
| `SelectToken` | `string path`<br>`JsonSelectSettings settings` | `JsonElement?` | Selects a single token using a JSONPath expression with custom settings. Throws `JsonPathException` if no match is found and `ErrorWhenNoMatch` is true. |
| `SelectTokens` | `string path` | `IEnumerable<JsonElement>` | Selects all tokens that match the JSONPath expression. Returns empty enumerable if no matches found. |
| `SelectTokens` | `string path`<br>`JsonSelectSettings settings` | `IEnumerable<JsonElement>` | Selects all tokens that match the JSONPath expression with custom settings. Throws `JsonPathException` if no matches found and `ErrorWhenNoMatch` is true. |

### JsonSelectSettings Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ErrorWhenNoMatch` | `bool` | `false` | When true, throws an exception if the path returns no matches |
| `RegexMatchTimeout` | `TimeSpan` | `null` | Timeout for regular expression operations in filter expressions |

## Error Handling

You can control error handling behavior using `JsonSelectSettings`:

```csharp
var settings = new JsonSelectSettings {
    ErrorWhenNoMatch = true, // Throw exception when path returns no matches
    RegexMatchTimeout = TimeSpan.FromSeconds(1) // Timeout for regex operations
};

var result = document.SelectToken("$.nonexistent", settings);
```

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.