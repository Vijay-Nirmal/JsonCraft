using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using JsonCraft.Benchmark;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.Json;

var config = ManualConfig.Create(DefaultConfig.Instance)
                        .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                        .WithArtifactsPath("../BenchmarkDotNet.Artifacts")
                        .HideColumns("StdDev", "RatioSD")
                        .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(100)
                                                                .WithRatioStyle(RatioStyle.Trend));


var report = BenchmarkRunner.Run<BenchmarkJsonPath>(config);


var jsonString = """
        {
            "store": {
                "book": [
                    {
                        "category": "reference",
                        "author": "Nigel Rees",
                        "title": "Sayings of the Century",
                        "price": 8.95
                    },
                    {
                        "category": "fiction",
                        "author": "Evelyn Waugh",
                        "title": "Sword of Honour",
                        "price": 12.99
                    }
                ],
                "bicycle": {
                    "color": "red",
                    "price": 19.95
                }
            }
        }
        """;

var jsonDocument = JsonDocument.Parse(jsonString);

var newtonsoftJson = JToken.Parse(jsonString);

//var start = Stopwatch.GetTimestamp();
//for (int i = 0; i < 10_000_000; i++)
//{
//    int count = 0;
//    foreach (var item in JsonCraft.JsonPath.JsonExtensions.SelectTokens(jsonDocument.RootElement, "$.store.book[?(@.price > 10 && @.price < 20)]"))
//    {
//        count++;
//    }
//    //foreach (var item in newtonsoftJson.SelectTokens("$..*"))
//    //{
//    //    count++;
//    //}
//    //foreach (var item in BlushingPenguin.JsonPath.JsonExtensions.SelectTokens(jsonDocument.RootElement, "$..*"))
//    //{
//    //    count++;
//    //}
//}
//Console.WriteLine();
//Console.WriteLine(Stopwatch.GetElapsedTime(start));
//Console.WriteLine("===================================================");
//Console.WriteLine(string.Join("\n", newtonsoftJson.SelectTokens("$.store.bicycle.color").Select(x => x.ToString())));
//Console.WriteLine(string.Join("\n", JsonCraft.JsonPath.JsonExtensions.SelectTokens(jsonDocument.RootElement, "$..['bicycle','price']").Select(x => JsonSerializer.Serialize(x))));
//Console.WriteLine(string.Join("\n", JsonCraft.JsonPath.JsonExtensions.SelectTokens(jsonDocument.RootElement, "$.store.book[?(@.category == 'fiction')]").ToList().Select(x => JsonSerializer.Serialize(x))));
//Console.WriteLine("===================================================");
//Console.WriteLine(string.Join("\n", BlushingPenguin.JsonPath.JsonExtensions.SelectTokens(jsonDocument.RootElement, "$.store.book[?(@.author && @.title)]").Select(x => JsonSerializer.Serialize(x))));
