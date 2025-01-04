
using JsonCraft.JsonPath;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JsonCraft.Tests.JsonPath
{
    [TestClass]
    public class JPathExecuteTests
    {
        [TestMethod]
        public void GreaterThanIssue1518()
        {
            string statusJson = @"{""usingmem"": ""214376""}";//214,376
            var jObj = JsonDocument.Parse(statusJson);

            var aa = jObj.SelectToken("$..[?(@.usingmem>10)]");//found,10
            Assert.IsTrue(jObj, aa);

            var bb = jObj.SelectToken("$..[?(@.usingmem>27000)]");//null, 27,000
            Assert.AreEqual(jObj, bb);

            var cc = jObj.SelectToken("$..[?(@.usingmem>21437)]");//found, 21,437
            Assert.AreEqual(jObj, cc);

            var dd = jObj.SelectToken("$..[?(@.usingmem>21438)]");//null,21,438
            Assert.AreEqual(jObj, dd);
        }

        /*[TestMethod]
        public void BacktrackingRegex_SingleMatch_TimeoutRespected()
        {
            const string RegexBacktrackingPattern = "(?<a>(.*?))[|].*(?<b>(.*?))[|].*(?<c>(.*?))[|].*(?<d>[1-3])[|].*(?<e>(.*?))[|].*[|].*[|].*(?<f>(.*?))[|].*[|].*(?<g>(.*?))[|].*(?<h>(.*))";

            var regexBacktrackingData = new JArray();
            regexBacktrackingData.Add(new JObject(new JProperty("b", @"15/04/2020 8:18:03 PM|1|System.String[]|3|Libero eligendi magnam ut inventore.. Quaerat et sit voluptatibus repellendus blanditiis aliquam ut.. Quidem qui ut sint in ex et tempore.|||.\iste.cpp||46018|-1")));

            Assert.ThrowsException<RegexMatchTimeoutException>(() =>
            {
                regexBacktrackingData.SelectTokens(
                    $"[?(@.b =~ /{RegexBacktrackingPattern}/)]",
                    new JsonSelectSettings
                    {
                        RegexMatchTimeout = TimeSpan.FromSeconds(0.01)
                    }).ToArray();
            });
        }

        [TestMethod]
        public void GreaterThanWithIntegerParameterAndStringValue()
        {
            string json = @"{
  ""persons"": [
    {
      ""name""  : ""John"",
      ""age"": ""26""
    },
    {
      ""name""  : ""Jane"",
      ""age"": ""2""
    }
  ]
}";

            JObject models = JsonDocument.Parse(json);

            var results = models.SelectTokens("$.persons[?(@.age > 3)]").ToList();

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void GreaterThanWithStringParameterAndIntegerValue()
        {
            string json = @"{
              ""persons"": [
                {
                  ""name""  : ""John"",
                  ""age"": 26
                },
                {
                  ""name""  : ""Jane"",
                  ""age"": 2
                }
              ]
            }";

            JObject models = JsonDocument.Parse(json);

            var results = models.SelectTokens("$.persons[?(@.age > '3')]").ToList();

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void RecursiveWildcard()
        {
            string json = @"{
    ""a"": [
        {
            ""id"": 1
        }
    ],
    ""b"": [
        {
            ""id"": 2
        },
        {
            ""id"": 3,
            ""c"": {
                ""id"": 4
            }
        }
    ],
    ""d"": [
        {
            ""id"": 5
        }
    ]
}";

            JObject models = JsonDocument.Parse(json);

            var results = models.SelectTokens("$.b..*.id").ToList();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(2, (int)results[0]);
            Assert.AreEqual(3, (int)results[1]);
            Assert.AreEqual(4, (int)results[2]);
        }

        [TestMethod]
        public void ScanFilter()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            JObject models = JsonDocument.Parse(json);

            var results = models.SelectTokens("$.elements..[?(@.id=='AAA')]").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(models["elements"][0]["children"][0]["children"][0], results[0]);
        }

        [TestMethod]
        public void FilterTrue()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            JObject models = JsonDocument.Parse(json);

            var results = models.SelectTokens("$.elements[?(true)]").ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(results[0], models["elements"][0]);
            Assert.AreEqual(results[1], models["elements"][1]);
        }

        [TestMethod]
        public void ScanFilterTrue()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            JObject models = JsonDocument.Parse(json);

            var results = models.SelectTokens("$.elements..[?(true)]").ToList();

            Assert.AreEqual(25, results.Count);
        }

        [TestMethod]
        public void ScanQuoted()
        {
            string json = @"{
    ""Node1"": {
        ""Child1"": {
            ""Name"": ""IsMe"",
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        },
        ""My.Child.Node"": {
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        }
    },
    ""Node2"": {
        ""TargetNode"": {
            ""Prop1"": ""Val1"",
            ""Prop2"": ""Val2""
        }
    }
}";

            JObject models = JsonDocument.Parse(json);

            int result = models.SelectTokens("$..['My.Child.Node']").Count();
            Assert.AreEqual(1, result);

            result = models.SelectTokens("..['My.Child.Node']").Count();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ScanMultipleQuoted()
        {
            string json = @"{
    ""Node1"": {
        ""Child1"": {
            ""Name"": ""IsMe"",
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        },
        ""My.Child.Node"": {
            ""TargetNode"": {
                ""Prop1"": ""Val3"",
                ""Prop2"": ""Val4""
            }
        }
    },
    ""Node2"": {
        ""TargetNode"": {
            ""Prop1"": ""Val5"",
            ""Prop2"": ""Val6""
        }
    }
}";

            JObject models = JsonDocument.Parse(json);

            var results = models.SelectTokens("$..['My.Child.Node','Prop1','Prop2']").ToList();
            Assert.AreEqual("Val1", (string)results[0]);
            Assert.AreEqual("Val2", (string)results[1]);
            Assert.AreEqual(JTokenType.Object, results[2].Type);
            Assert.AreEqual("Val3", (string)results[3]);
            Assert.AreEqual("Val4", (string)results[4]);
            Assert.AreEqual("Val5", (string)results[5]);
            Assert.AreEqual("Val6", (string)results[6]);
        }

        [TestMethod]
        public void ParseWithEmptyArrayContent()
        {
            var json = @"{
    'controls': [
        {
            'messages': {
                'addSuggestion': {
                    'en-US': 'Add'
                }
            }
        },
        {
            'header': {
                'controls': []
            },
            'controls': [
                {
                    'controls': [
                        {
                            'defaultCaption': {
                                'en-US': 'Sort by'
                            },
                            'sortOptions': [
                                {
                                    'label': {
                                        'en-US': 'Name'
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    ]
}";
            JObject jToken = JsonDocument.Parse(json);
            IList<JToken> tokens = jToken.SelectTokens("$..en-US").ToList();

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual("Add", (string)tokens[0]);
            Assert.AreEqual("Sort by", (string)tokens[1]);
            Assert.AreEqual("Name", (string)tokens[2]);
        }

        [TestMethod]
        public void SelectTokenAfterEmptyContainer()
        {
            string json = @"{
    'cont': [],
    'test': 'no one will find me'
}";

            JObject o = JsonDocument.Parse(json);

            IList<JToken> results = o.SelectTokens("$..test").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("no one will find me", (string)results[0]);
        }

        [TestMethod]
        public void EvaluatePropertyWithRequired()
        {
            string json = "{\"bookId\":\"1000\"}";
            JObject o = JsonDocument.Parse(json);

            string bookId = (string)o.SelectToken("bookId", true);

            Assert.AreEqual("1000", bookId);
        }

        [TestMethod]
        public void EvaluateEmptyPropertyIndexer()
        {
            JObject o = new JObject(
                new JProperty("", 1));

            JToken t = o.SelectToken("['']");
            Assert.AreEqual(1, (int)t);
        }

        [TestMethod]
        public void EvaluateEmptyString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("");
            Assert.AreEqual(o, t);

            t = o.SelectToken("['']");
            Assert.AreEqual(null, t);
        }

        [TestMethod]
        public void EvaluateEmptyStringWithMatchingEmptyProperty()
        {
            JObject o = new JObject(
                new JProperty(" ", 1));

            JToken t = o.SelectToken("[' ']");
            Assert.AreEqual(1, (int)t);
        }

        [TestMethod]
        public void EvaluateWhitespaceString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken(" ");
            Assert.AreEqual(o, t);
        }

        [TestMethod]
        public void EvaluateDollarString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("$");
            Assert.AreEqual(o, t);
        }

        [TestMethod]
        public void EvaluateDollarTypeString()
        {
            JObject o = new JObject(
                new JProperty("$values", new JArray(1, 2, 3)));

            JToken t = o.SelectToken("$values[1]");
            Assert.AreEqual(2, (int)t);
        }

        [TestMethod]
        public void EvaluateSingleProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("Blah");
            Assert.IsNotNull(t);
            Assert.AreEqual(JTokenType.Integer, t.Type);
            Assert.AreEqual(1, (int)t);
        }

        [TestMethod]
        public void EvaluateWildcardProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1),
                new JProperty("Blah2", 2));

            IList<JToken> t = o.SelectTokens("$.*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
        }

        [TestMethod]
        public void QuoteName()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("['Blah']");
            Assert.IsNotNull(t);
            Assert.AreEqual(JTokenType.Integer, t.Type);
            Assert.AreEqual(1, (int)t);
        }

        [TestMethod]
        public void EvaluateMissingProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("Missing[1]");
            Assert.IsNull(t);
        }

        [TestMethod]
        public void EvaluateIndexerOnObject()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("[1]");
            Assert.IsNull(t);
        }

        [TestMethod]
        public void EvaluateIndexerOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("[1]", true); }, @"Index 1 not valid on JObject.");
        }

        [TestMethod]
        public void EvaluateWildcardIndexOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("[*]", true); }, @"Index * not valid on JObject.");
        }

        [TestMethod]
        public void EvaluateSliceOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("[:]", true); }, @"Array slice is not valid on JObject.");
        }

        [TestMethod]
        public void EvaluatePropertyOnArray()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            JToken t = a.SelectToken("BlahBlah");
            Assert.IsNull(t);
        }

        [TestMethod]
        public void EvaluateMultipleResultsError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[0, 1]"); }, @"Path returned multiple tokens.");
        }

        [TestMethod]
        public void EvaluatePropertyOnArrayWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("BlahBlah", true); }, @"Property 'BlahBlah' not valid on JArray.");
        }

        [TestMethod]
        public void EvaluateNoResultsWithMultipleArrayIndexes()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[9,10]", true); }, @"Index 9 outside the bounds of JArray.");
        }

        [TestMethod]
        public void EvaluateConstructorOutOfBoundsIndxerWithError()
        {
            JConstructor c = new JConstructor("Blah");

            ExceptionAssert.Throws<JsonException>(() => { c.SelectToken("[1]", true); }, @"Index 1 outside the bounds of JConstructor.");
        }

        [TestMethod]
        public void EvaluateConstructorOutOfBoundsIndxer()
        {
            JConstructor c = new JConstructor("Blah");

            Assert.IsNull(c.SelectToken("[1]"));
        }

        [TestMethod]
        public void EvaluateMissingPropertyWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("Missing", true); }, "Property 'Missing' does not exist on JObject.");
        }

        [TestMethod]
        public void EvaluatePropertyWithoutError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JValue v = (JValue)o.SelectToken("Blah", true);
            Assert.AreEqual(1, v.Value);
        }

        [TestMethod]
        public void EvaluateMissingPropertyIndexWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("['Missing','Missing2']", true); }, "Property 'Missing' does not exist on JObject.");
        }

        [TestMethod]
        public void EvaluateMultiPropertyIndexOnArrayWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("['Missing','Missing2']", true); }, "Properties 'Missing', 'Missing2' not valid on JArray.");
        }

        [TestMethod]
        public void EvaluateArraySliceWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[99:]", true); }, "Array slice of 99 to * returned no results.");

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[1:-19]", true); }, "Array slice of 1 to -19 returned no results.");

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[:-19]", true); }, "Array slice of * to -19 returned no results.");

            a = new JArray();

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[:]", true); }, "Array slice of * to * returned no results.");
        }

        [TestMethod]
        public void EvaluateOutOfBoundsIndxer()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            JToken t = a.SelectToken("[1000].Ha");
            Assert.IsNull(t);
        }

        [TestMethod]
        public void EvaluateArrayOutOfBoundsIndxerWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[1000].Ha", true); }, "Index 1000 outside the bounds of JArray.");
        }

        [TestMethod]
        public void EvaluateArray()
        {
            JArray a = new JArray(1, 2, 3, 4);

            JToken t = a.SelectToken("[1]");
            Assert.IsNotNull(t);
            Assert.AreEqual(JTokenType.Integer, t.Type);
            Assert.AreEqual(2, (int)t);
        }

        [TestMethod]
        public void EvaluateArraySlice()
        {
            JArray a = new JArray(1, 2, 3, 4, 5, 6, 7, 8, 9);
            IList<JToken> t = null;

            t = a.SelectTokens("[-3:]").ToList();
            Assert.AreEqual(3, t.Count);
            Assert.AreEqual(7, (int)t[0]);
            Assert.AreEqual(8, (int)t[1]);
            Assert.AreEqual(9, (int)t[2]);

            t = a.SelectTokens("[-1:-2:-1]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(9, (int)t[0]);

            t = a.SelectTokens("[-2:-1]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(8, (int)t[0]);

            t = a.SelectTokens("[1:1]").ToList();
            Assert.AreEqual(0, t.Count);

            t = a.SelectTokens("[1:2]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(2, (int)t[0]);

            t = a.SelectTokens("[::-1]").ToList();
            Assert.AreEqual(9, t.Count);
            Assert.AreEqual(9, (int)t[0]);
            Assert.AreEqual(8, (int)t[1]);
            Assert.AreEqual(7, (int)t[2]);
            Assert.AreEqual(6, (int)t[3]);
            Assert.AreEqual(5, (int)t[4]);
            Assert.AreEqual(4, (int)t[5]);
            Assert.AreEqual(3, (int)t[6]);
            Assert.AreEqual(2, (int)t[7]);
            Assert.AreEqual(1, (int)t[8]);

            t = a.SelectTokens("[::-2]").ToList();
            Assert.AreEqual(5, t.Count);
            Assert.AreEqual(9, (int)t[0]);
            Assert.AreEqual(7, (int)t[1]);
            Assert.AreEqual(5, (int)t[2]);
            Assert.AreEqual(3, (int)t[3]);
            Assert.AreEqual(1, (int)t[4]);
        }

        [TestMethod]
        public void EvaluateWildcardArray()
        {
            JArray a = new JArray(1, 2, 3, 4);

            List<JToken> t = a.SelectTokens("[*]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
            Assert.AreEqual(3, (int)t[2]);
            Assert.AreEqual(4, (int)t[3]);
        }

        [TestMethod]
        public void EvaluateArrayMultipleIndexes()
        {
            JArray a = new JArray(1, 2, 3, 4);

            IEnumerable<JToken> t = a.SelectTokens("[1,2,0]");
            Assert.IsNotNull(t);
            Assert.AreEqual(3, t.Count());
            Assert.AreEqual(2, (int)t.ElementAt(0));
            Assert.AreEqual(3, (int)t.ElementAt(1));
            Assert.AreEqual(1, (int)t.ElementAt(2));
        }

        [TestMethod]
        public void EvaluateScan()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JArray a = new JArray(o1, o2);

            IList<JToken> t = a.SelectTokens("$..Name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
        }

        [TestMethod]
        public void EvaluateWildcardScan()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JArray a = new JArray(o1, o2);

            IList<JToken> t = a.SelectTokens("$..*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(5, t.Count);
            Assert.IsTrue(JToken.DeepEquals(a, t[0]));
            Assert.IsTrue(JToken.DeepEquals(o1, t[1]));
            Assert.AreEqual(1, (int)t[2]);
            Assert.IsTrue(JToken.DeepEquals(o2, t[3]));
            Assert.AreEqual(2, (int)t[4]);
        }

        [TestMethod]
        public void EvaluateScanNestResults()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JObject o3 = new JObject { { "Name", new JObject { { "Name", new JArray(3) } } } };
            JArray a = new JArray(o1, o2, o3);

            IList<JToken> t = a.SelectTokens("$..Name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
            Assert.IsTrue(JToken.DeepEquals(new JObject { { "Name", new JArray(3) } }, t[2]));
            Assert.IsTrue(JToken.DeepEquals(new JArray(3), t[3]));
        }

        [TestMethod]
        public void EvaluateWildcardScanNestResults()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JObject o3 = new JObject { { "Name", new JObject { { "Name", new JArray(3) } } } };
            JArray a = new JArray(o1, o2, o3);

            IList<JToken> t = a.SelectTokens("$..*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(9, t.Count);

            Assert.IsTrue(JToken.DeepEquals(a, t[0]));
            Assert.IsTrue(JToken.DeepEquals(o1, t[1]));
            Assert.AreEqual(1, (int)t[2]);
            Assert.IsTrue(JToken.DeepEquals(o2, t[3]));
            Assert.AreEqual(2, (int)t[4]);
            Assert.IsTrue(JToken.DeepEquals(o3, t[5]));
            Assert.IsTrue(JToken.DeepEquals(new JObject { { "Name", new JArray(3) } }, t[6]));
            Assert.IsTrue(JToken.DeepEquals(new JArray(3), t[7]));
            Assert.AreEqual(3, (int)t[8]);
        }

        [TestMethod]
        public void EvaluateSinglePropertyReturningArray()
        {
            JObject o = new JObject(
                new JProperty("Blah", new[] { 1, 2, 3 }));

            JToken t = o.SelectToken("Blah");
            Assert.IsNotNull(t);
            Assert.AreEqual(JTokenType.Array, t.Type);

            t = o.SelectToken("Blah[2]");
            Assert.AreEqual(JTokenType.Integer, t.Type);
            Assert.AreEqual(3, (int)t);
        }

        [TestMethod]
        public void EvaluateLastSingleCharacterProperty()
        {
            JObject o2 = JsonDocument.Parse("{'People':[{'N':'Jeff'}]}");
            string a2 = (string)o2.SelectToken("People[0].N");

            Assert.AreEqual("Jeff", a2);
        }

        [TestMethod]
        public void ExistsQuery()
        {
            JArray a = new JArray(new JObject(new JProperty("hi", "ho")), new JObject(new JProperty("hi2", "ha")));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", "ho")), t[0]));
        }

        [TestMethod]
        public void EqualsQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", "ho")),
                new JObject(new JProperty("hi", "ha")));

            IList<JToken> t = a.SelectTokens("[ ?( @.['hi'] == 'ha' ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", "ha")), t[0]));
        }

        [TestMethod]
        public void NotEqualsQuery()
        {
            JArray a = new JArray(
                new JArray(new JObject(new JProperty("hi", "ho"))),
                new JArray(new JObject(new JProperty("hi", "ha"))));

            IList<JToken> t = a.SelectTokens("[ ?( @..hi <> 'ha' ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JArray(new JObject(new JProperty("hi", "ho"))), t[0]));
        }

        [TestMethod]
        public void NoPathQuery()
        {
            JArray a = new JArray(1, 2, 3);

            IList<JToken> t = a.SelectTokens("[ ?( @ > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(2, (int)t[0]);
            Assert.AreEqual(3, (int)t[1]);
        }

        [TestMethod]
        public void MultipleQueries()
        {
            JArray a = new JArray(1, 2, 3, 4, 5, 6, 7, 8, 9);

            // json path does item based evaluation - http://www.sitepen.com/blog/2008/03/17/jsonpath-support/
            // first query resolves array to ints
            // int has no children to query
            IList<JToken> t = a.SelectTokens("[?(@ <> 1)][?(@ <> 4)][?(@ < 7)]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(0, t.Count);
        }

        [TestMethod]
        public void GreaterQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", 1)),
                new JObject(new JProperty("hi", 2)),
                new JObject(new JProperty("hi", 3)));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[0]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[1]));
        }

        [TestMethod]
        public void LesserQuery_ValueFirst()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", 1)),
                new JObject(new JProperty("hi", 2)),
                new JObject(new JProperty("hi", 3)));

            IList<JToken> t = a.SelectTokens("[ ?( 1 < @.hi ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[0]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[1]));
        }

#if !(PORTABLE || DNXCORE50 || PORTABLE40 || NET35 || NET20) || NETSTANDARD1_3 || NETSTANDARD2_0 || NET6_0_OR_GREATER
        [TestMethod]
        public void GreaterQueryBigInteger()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", new BigInteger(1))),
                new JObject(new JProperty("hi", new BigInteger(2))),
                new JObject(new JProperty("hi", new BigInteger(3))));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[0]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[1]));
        }
#endif

        [TestMethod]
        public void GreaterOrEqualQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", 1)),
                new JObject(new JProperty("hi", 2)),
                new JObject(new JProperty("hi", 2.0)),
                new JObject(new JProperty("hi", 3)));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi >= 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 1)), t[0]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[1]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2.0)), t[2]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[3]));
        }

        [TestMethod]
        public void NestedQuery()
        {
            JArray a = new JArray(
                new JObject(
                    new JProperty("name", "Bad Boys"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Will Smith"))))),
                new JObject(
                    new JProperty("name", "Independence Day"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Will Smith"))))),
                new JObject(
                    new JProperty("name", "The Rock"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Nick Cage")))))
                );

            IList<JToken> t = a.SelectTokens("[?(@.cast[?(@.name=='Will Smith')])].name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual("Bad Boys", (string)t[0]);
            Assert.AreEqual("Independence Day", (string)t[1]);
        }

        [TestMethod]
        public void PathWithConstructor()
        {
            JArray a = JArray.Parse(@"[
  {
    ""Property1"": [
      1,
      [
        [
          []
        ]
      ]
    ]
  },
  {
    ""Property2"": new Constructor1(
      null,
      [
        1
      ]
    )
  }
]");

            JValue v = (JValue)a.SelectToken("[1].Property2[1][0]");
            Assert.AreEqual(1L, v.Value);
        }

        [TestMethod]
        public void MultiplePaths()
        {
            JArray a = JArray.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(@.price > @.max_price)]").ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(a[2], results[0]);
        }

        [TestMethod]
        public void Exists_True()
        {
            JArray a = JArray.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(true)]").ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(a[0], results[0]);
            Assert.AreEqual(a[1], results[1]);
            Assert.AreEqual(a[2], results[2]);
        }

        [TestMethod]
        public void Exists_Null()
        {
            JArray a = JArray.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(true)]").ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(a[0], results[0]);
            Assert.AreEqual(a[1], results[1]);
            Assert.AreEqual(a[2], results[2]);
        }

        [TestMethod]
        public void WildcardWithProperty()
        {
            JObject o = JsonDocument.Parse(@"{
    ""station"": 92000041000001, 
    ""containers"": [
        {
            ""id"": 1,
            ""text"": ""Sort system"",
            ""containers"": [
                {
                    ""id"": ""2"",
                    ""text"": ""Yard 11""
                },
                {
                    ""id"": ""92000020100006"",
                    ""text"": ""Sort yard 12""
                },
                {
                    ""id"": ""92000020100005"",
                    ""text"": ""Yard 13""
                } 
            ]
        }, 
        {
            ""id"": ""92000020100011"",
            ""text"": ""TSP-1""
        }, 
        {
            ""id"":""92000020100007"",
            ""text"": ""Passenger 15""
        }
    ]
}");

            IList<JToken> tokens = o.SelectTokens("$..*[?(@.text)]").ToList();
            int i = 0;
            Assert.AreEqual("Sort system", (string)tokens[i++]["text"]);
            Assert.AreEqual("TSP-1", (string)tokens[i++]["text"]);
            Assert.AreEqual("Passenger 15", (string)tokens[i++]["text"]);
            Assert.AreEqual("Yard 11", (string)tokens[i++]["text"]);
            Assert.AreEqual("Sort yard 12", (string)tokens[i++]["text"]);
            Assert.AreEqual("Yard 13", (string)tokens[i++]["text"]);
            Assert.AreEqual(6, tokens.Count);
        }

        [TestMethod]
        public void QueryAgainstNonStringValues()
        {
            IList<object> values = new List<object>
            {
                "ff2dc672-6e15-4aa2-afb0-18f4f69596ad",
                new Guid("ff2dc672-6e15-4aa2-afb0-18f4f69596ad"),
                "http://localhost",
                new Uri("http://localhost"),
                "2000-12-05T05:07:59Z",
                new DateTime(2000, 12, 5, 5, 7, 59, DateTimeKind.Utc),
#if !NET20
                "2000-12-05T05:07:59-10:00",
                new DateTimeOffset(2000, 12, 5, 5, 7, 59, -TimeSpan.FromHours(10)),
#endif
                "SGVsbG8gd29ybGQ=",
                Encoding.UTF8.GetBytes("Hello world"),
                "365.23:59:59",
                new TimeSpan(365, 23, 59, 59)
            };

            JObject o = new JObject(
                new JProperty("prop",
                    new JArray(
                        values.Select(v => new JObject(new JProperty("childProp", v)))
                        )
                    )
                );

            IList<JToken> t = o.SelectTokens("$.prop[?(@.childProp =='ff2dc672-6e15-4aa2-afb0-18f4f69596ad')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='http://localhost')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='2000-12-05T05:07:59Z')]").ToList();
            Assert.AreEqual(2, t.Count);

#if !NET20
            t = o.SelectTokens("$.prop[?(@.childProp =='2000-12-05T05:07:59-10:00')]").ToList();
            Assert.AreEqual(2, t.Count);
#endif

            t = o.SelectTokens("$.prop[?(@.childProp =='SGVsbG8gd29ybGQ=')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='365.23:59:59')]").ToList();
            Assert.AreEqual(2, t.Count);
        }

        [TestMethod]
        public void Example()
        {
            JObject o = JsonDocument.Parse(@"{
        ""Stores"": [
          ""Lambton Quay"",
          ""Willis Street""
        ],
        ""Manufacturers"": [
          {
            ""Name"": ""Acme Co"",
            ""Products"": [
              {
                ""Name"": ""Anvil"",
                ""Price"": 50
              }
            ]
          },
          {
            ""Name"": ""Contoso"",
            ""Products"": [
              {
                ""Name"": ""Elbow Grease"",
                ""Price"": 99.95
              },
              {
                ""Name"": ""Headlight Fluid"",
                ""Price"": 4
              }
            ]
          }
        ]
      }");

            string name = (string)o.SelectToken("Manufacturers[0].Name");
            // Acme Co

            decimal productPrice = (decimal)o.SelectToken("Manufacturers[0].Products[0].Price");
            // 50

            string productName = (string)o.SelectToken("Manufacturers[1].Products[0].Name");
            // Elbow Grease

            Assert.AreEqual("Acme Co", name);
            Assert.AreEqual(50m, productPrice);
            Assert.AreEqual("Elbow Grease", productName);

            IList<string> storeNames = o.SelectToken("Stores").Select(s => (string)s).ToList();
            // Lambton Quay
            // Willis Street

            IList<string> firstProductNames = o["Manufacturers"].Select(m => (string)m.SelectToken("Products[1].Name")).ToList();
            // null
            // Headlight Fluid

            decimal totalPrice = o["Manufacturers"].Sum(m => (decimal)m.SelectToken("Products[0].Price"));
            // 149.95

            Assert.AreEqual(2, storeNames.Count);
            Assert.AreEqual("Lambton Quay", storeNames[0]);
            Assert.AreEqual("Willis Street", storeNames[1]);
            Assert.AreEqual(2, firstProductNames.Count);
            Assert.AreEqual(null, firstProductNames[0]);
            Assert.AreEqual("Headlight Fluid", firstProductNames[1]);
            Assert.AreEqual(149.95m, totalPrice);
        }

        [TestMethod]
        public void NotEqualsAndNonPrimativeValues()
        {
            string json = @"[
  {
    ""name"": ""string"",
    ""value"": ""aString""
  },
  {
    ""name"": ""number"",
    ""value"": 123
  },
  {
    ""name"": ""array"",
    ""value"": [
      1,
      2,
      3,
      4
    ]
  },
  {
    ""name"": ""object"",
    ""value"": {
      ""1"": 1
    }
  }
]";

            JArray a = JArray.Parse(json);

            List<JToken> result = a.SelectTokens("$.[?(@.value!=1)]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!='2000-12-05T05:07:59-10:00')]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!=null)]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!=123)]").ToList();
            Assert.AreEqual(3, result.Count);

            result = a.SelectTokens("$.[?(@.value)]").ToList();
            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public void RootInFilter()
        {
            string json = @"[
   {
      ""store"" : {
         ""book"" : [
            {
               ""category"" : ""reference"",
               ""author"" : ""Nigel Rees"",
               ""title"" : ""Sayings of the Century"",
               ""price"" : 8.95
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""Evelyn Waugh"",
               ""title"" : ""Sword of Honour"",
               ""price"" : 12.99
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""Herman Melville"",
               ""title"" : ""Moby Dick"",
               ""isbn"" : ""0-553-21311-3"",
               ""price"" : 8.99
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""J. R. R. Tolkien"",
               ""title"" : ""The Lord of the Rings"",
               ""isbn"" : ""0-395-19395-8"",
               ""price"" : 22.99
            }
         ],
         ""bicycle"" : {
            ""color"" : ""red"",
            ""price"" : 19.95
         }
      },
      ""expensive"" : 10
   }
]";

            JArray a = JArray.Parse(json);

            List<JToken> result = a.SelectTokens("$.[?($.[0].store.bicycle.price < 20)]").ToList();
            Assert.AreEqual(1, result.Count);

            result = a.SelectTokens("$.[?($.[0].store.bicycle.price < 10)]").ToList();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void RootInFilterWithRootObject()
        {
            string json = @"{
                ""store"" : {
                    ""book"" : [
                        {
                            ""category"" : ""reference"",
                            ""author"" : ""Nigel Rees"",
                            ""title"" : ""Sayings of the Century"",
                            ""price"" : 8.95
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""Evelyn Waugh"",
                            ""title"" : ""Sword of Honour"",
                            ""price"" : 12.99
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""Herman Melville"",
                            ""title"" : ""Moby Dick"",
                            ""isbn"" : ""0-553-21311-3"",
                            ""price"" : 8.99
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""J. R. R. Tolkien"",
                            ""title"" : ""The Lord of the Rings"",
                            ""isbn"" : ""0-395-19395-8"",
                            ""price"" : 22.99
                        }
                    ],
                    ""bicycle"" : [
                        {
                            ""color"" : ""red"",
                            ""price"" : 19.95
                        }
                    ]
                },
                ""expensive"" : 10
            }";

            JObject a = JsonDocument.Parse(json);

            List<JToken> result = a.SelectTokens("$..book[?(@.price <= $['expensive'])]").ToList();
            Assert.AreEqual(2, result.Count);

            result = a.SelectTokens("$.store..[?(@.price > $.expensive)]").ToList();
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void RootInFilterWithInitializers()
        {
            JObject rootObject = new JObject
            {
                { "referenceDate", new JValue(DateTime.MinValue) },
                {
                    "dateObjectsArray",
                    new JArray()
                    {
                        new JObject { { "date", new JValue(DateTime.MinValue) } },
                        new JObject { { "date", new JValue(DateTime.MaxValue) } },
                        new JObject { { "date", new JValue(DateTime.Now) } },
                        new JObject { { "date", new JValue(DateTime.MinValue) } },
                    }
                }
            };

            List<JToken> result = rootObject.SelectTokens("$.dateObjectsArray[?(@.date == $.referenceDate)]").ToList();
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void IdentityOperator()
        {
            JObject o = JsonDocument.Parse(@"{
	            'Values': [{

                    'Coercible': 1,
                    'Name': 'Number'

                }, {
		            'Coercible': '1',
		            'Name': 'String'
	            }]
            }");

            // just to verify expected behavior hasn't changed
            IEnumerable<string> sanity1 = o.SelectTokens("Values[?(@.Coercible == '1')].Name").Select(x => (string)x);
            IEnumerable<string> sanity2 = o.SelectTokens("Values[?(@.Coercible != '1')].Name").Select(x => (string)x);
            // new behavior
            IEnumerable<string> mustBeNumber1 = o.SelectTokens("Values[?(@.Coercible === 1)].Name").Select(x => (string)x);
            IEnumerable<string> mustBeString1 = o.SelectTokens("Values[?(@.Coercible !== 1)].Name").Select(x => (string)x);
            IEnumerable<string> mustBeString2 = o.SelectTokens("Values[?(@.Coercible === '1')].Name").Select(x => (string)x);
            IEnumerable<string> mustBeNumber2 = o.SelectTokens("Values[?(@.Coercible !== '1')].Name").Select(x => (string)x);

            // FAILS-- JPath returns { "String" }
            //CollectionAssert.AreEquivalent(new[] { "Number", "String" }, sanity1);
            // FAILS-- JPath returns { "Number" }
            //Assert.IsTrue(!sanity2.Any());
            Assert.AreEqual("Number", mustBeNumber1.Single());
            Assert.AreEqual("String", mustBeString1.Single());
            Assert.AreEqual("Number", mustBeNumber2.Single());
            Assert.AreEqual("String", mustBeString2.Single());
        }

        [TestMethod]
        public void QueryWithEscapedPath()
        {
            JToken t = JToken.Parse(@"{
""Property"": [
          {
            ""@Name"": ""x"",
            ""@Value"": ""y"",
            ""@Type"": ""FindMe""
          }
   ]
}");

            var tokens = t.SelectTokens("$..[?(@.['@Type'] == 'FindMe')]").ToList();
            Assert.AreEqual(1, tokens.Count);
        }

        [TestMethod]
        public void Equals_FloatWithInt()
        {
            JToken t = JToken.Parse(@"{
  ""Values"": [
    {
      ""Property"": 1
    }
  ]
}");

            Assert.IsNotNull(t.SelectToken(@"Values[?(@.Property == 1.0)]"));
        }

#if DNXCORE50
        [Theory]
#endif
        [TestCaseSource(nameof(StrictMatchWithInverseTestData))]
        public static void EqualsStrict(string value1, string value2, bool matchStrict)
        {
            string completeJson = @"{
  ""Values"": [
    {
      ""Property"": " + value1 + @"
    }
  ]
}";
            string completeEqualsStrictPath = "$.Values[?(@.Property === " + value2 + ")]";
            string completeNotEqualsStrictPath = "$.Values[?(@.Property !== " + value2 + ")]";

            JToken t = JToken.Parse(completeJson);

            bool hasEqualsStrict = t.SelectTokens(completeEqualsStrictPath).Any();
            Assert.AreEqual(
                matchStrict,
                hasEqualsStrict,
                $"Expected {value1} and {value2} to match: {matchStrict}"
                + Environment.NewLine + completeJson + Environment.NewLine + completeEqualsStrictPath);

            bool hasNotEqualsStrict = t.SelectTokens(completeNotEqualsStrictPath).Any();
            Assert.AreNotEqual(
                matchStrict,
                hasNotEqualsStrict,
                $"Expected {value1} and {value2} to match: {!matchStrict}"
                + Environment.NewLine + completeJson + Environment.NewLine + completeEqualsStrictPath);
        }

        public static IEnumerable<object[]> StrictMatchWithInverseTestData()
        {
            foreach (var item in StrictMatchTestData())
            {
                yield return new object[] { item[0], item[1], item[2] };

                if (!item[0].Equals(item[1]))
                {
                    // Test the inverse
                    yield return new object[] { item[1], item[0], item[2] };
                }
            }
        }*/

        private static IEnumerable<object[]> StrictMatchTestData()
        {
            yield return new object[] { "1", "1", true };
            yield return new object[] { "1", "1.0", true };
            yield return new object[] { "1", "true", false };
            yield return new object[] { "1", "'1'", false };
            yield return new object[] { "'1'", "'1'", true };
            yield return new object[] { "false", "false", true };
            yield return new object[] { "true", "false", false };
            yield return new object[] { "1", "1.1", false };
            yield return new object[] { "1", "null", false };
            yield return new object[] { "null", "null", true };
            yield return new object[] { "null", "'null'", false };
        }
    }
}