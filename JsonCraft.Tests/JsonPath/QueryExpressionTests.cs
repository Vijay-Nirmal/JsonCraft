using JsonCraft.JsonPath;
using System.Text.Json;

namespace JsonCraft.Tests.JsonPath
{
    [TestClass]
    public class QueryExpressionTests
    {
        [TestMethod]
        public void AndExpressionTest()
        {
            CompositeExpression compositeExpression = new CompositeExpression(QueryOperator.And)
            {
                Expressions = new List<QueryExpression>
                {
                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("FirstName") }, null),
                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("LastName") }, null)
                }
            };

            JsonElement o1 = JsonDocument.Parse("{\"Title\":\"Title!\",\"FirstName\":\"FirstName!\",\"LastName\":\"LastName!\"}").RootElement;

            Assert.IsTrue(compositeExpression.IsMatch(o1, o1));

            JsonElement o2 = JsonDocument.Parse("{\"Title\":\"Title!\",\"FirstName\":\"FirstName!\"}").RootElement;

            Assert.IsFalse(compositeExpression.IsMatch(o2, o2));

            JsonElement o3 = JsonDocument.Parse("{\"Title\":\"Title!\"}").RootElement;

            Assert.IsFalse(compositeExpression.IsMatch(o3, o3));
        }

        [TestMethod]
        public void OrExpressionTest()
        {
            CompositeExpression compositeExpression = new CompositeExpression(QueryOperator.Or)
            {
                Expressions = new List<QueryExpression>
                {
                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("FirstName") }, null),
                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("LastName") }, null)
                }
            };

            JsonElement o1 = JsonDocument.Parse("{\"Title\":\"Title!\",\"FirstName\":\"FirstName!\",\"LastName\":\"LastName!\"}").RootElement;

            Assert.IsTrue(compositeExpression.IsMatch(o1, o1));

            JsonElement o2 = JsonDocument.Parse("{\"Title\":\"Title!\",\"FirstName\":\"FirstName!\"}").RootElement;

            Assert.IsTrue(compositeExpression.IsMatch(o2, o2));

            JsonElement o3 = JsonDocument.Parse("{\"Title\":\"Title!\"}").RootElement;

            Assert.IsFalse(compositeExpression.IsMatch(o3, o3));
        }

        [TestMethod]
        public void BooleanExpressionTest_RegexEqualsOperator()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, JsonDocument.Parse("\"/foo.*d/\"").RootElement);

            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"food\"]").RootElement));
            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"fooood and drink\"]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"FOOD\"]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"foo\", \"foog\", \"good\"]").RootElement));

            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, JsonDocument.Parse("\"/Foo.*d/i\"").RootElement);

            Assert.IsTrue(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"food\"]").RootElement));
            Assert.IsTrue(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"fooood and drink\"]").RootElement));
            Assert.IsTrue(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"FOOD\"]").RootElement));
            Assert.IsFalse(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"foo\", \"foog\", \"good\"]").RootElement));
        }

        [TestMethod]
        public void BooleanExpressionTest_RegexEqualsOperator_CornerCase()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, JsonDocument.Parse("\"/// comment/\"").RootElement);

            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"// comment\"]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"//comment\", \"/ comment\"]").RootElement));

            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, JsonDocument.Parse("\"/<tag>.*</tag>/i\"").RootElement);

            Assert.IsTrue(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"<Tag>Test</Tag>\", \"\"]").RootElement));
            Assert.IsFalse(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"<tag>Test<tag>\"]").RootElement));
        }

        [TestMethod]
        public void BooleanExpressionTest()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.LessThan, new List<PathFilter> { new ArrayIndexFilter() }, JsonDocument.Parse("3").RootElement);

            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[1, 2, 3, 4, 5]").RootElement));
            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[2, 3, 4, 5]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[3, 4, 5]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[4, 5]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"11\", 5]").RootElement));

            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.LessThanOrEquals, new List<PathFilter> { new ArrayIndexFilter() }, JsonDocument.Parse("3").RootElement);

            Assert.IsTrue(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[1, 2, 3, 4, 5]").RootElement));
            Assert.IsTrue(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[2, 3, 4, 5]").RootElement));
            Assert.IsTrue(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[3, 4, 5]").RootElement));
            Assert.IsFalse(e2.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[4, 5]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"11\", 5]").RootElement));
        }

        [TestMethod]
        public void BooleanExpressionTest_GreaterThanOperator()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.GreaterThan, new List<PathFilter> { new ArrayIndexFilter() }, JsonDocument.Parse("3").RootElement);

            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"2\", \"26\"]").RootElement));
            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[2, 26]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[2, 3]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"2\", \"3\"]").RootElement));
        }

        [TestMethod]
        public void BooleanExpressionTest_GreaterThanOrEqualsOperator()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.GreaterThanOrEquals, new List<PathFilter> { new ArrayIndexFilter() }, JsonDocument.Parse("3").RootElement);

            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"2\", \"26\"]").RootElement));
            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[2, 26]").RootElement));
            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[2, 3]").RootElement));
            Assert.IsTrue(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"2\", \"3\"]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[2, 1]").RootElement));
            Assert.IsFalse(e1.IsMatch(JsonSerializer.SerializeToElement(null as string), JsonDocument.Parse("[\"2\", \"1\"]").RootElement));
        }
    }
}