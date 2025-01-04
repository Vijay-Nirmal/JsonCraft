

//namespace JsonCraft.Tests.JsonPath
//{
//    [TestClass]
//    public class QueryExpressionTests
//    {
//        [TestMethod]
//        public void AndExpressionTest()
//        {
//            CompositeExpression compositeExpression = new CompositeExpression(QueryOperator.And)
//            {
//                Expressions = new List<QueryExpression>
//                {
//                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("FirstName") }, null),
//                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("LastName") }, null)
//                }
//            };

//            JObject o1 = new JObject
//            {
//                { "Title", "Title!" },
//                { "FirstName", "FirstName!" },
//                { "LastName", "LastName!" }
//            };

//            Assert.IsTrue(compositeExpression.IsMatch(o1, o1));

//            JObject o2 = new JObject
//            {
//                { "Title", "Title!" },
//                { "FirstName", "FirstName!" }
//            };

//            Assert.IsFalse(compositeExpression.IsMatch(o2, o2));

//            JObject o3 = new JObject
//            {
//                { "Title", "Title!" }
//            };

//            Assert.IsFalse(compositeExpression.IsMatch(o3, o3));
//        }

//        [TestMethod]
//        public void OrExpressionTest()
//        {
//            CompositeExpression compositeExpression = new CompositeExpression(QueryOperator.Or)
//            {
//                Expressions = new List<QueryExpression>
//                {
//                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("FirstName") }, null),
//                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("LastName") }, null)
//                }
//            };

//            JObject o1 = new JObject
//            {
//                { "Title", "Title!" },
//                { "FirstName", "FirstName!" },
//                { "LastName", "LastName!" }
//            };

//            Assert.IsTrue(compositeExpression.IsMatch(o1, o1));

//            JObject o2 = new JObject
//            {
//                { "Title", "Title!" },
//                { "FirstName", "FirstName!" }
//            };

//            Assert.IsTrue(compositeExpression.IsMatch(o2, o2));

//            JObject o3 = new JObject
//            {
//                { "Title", "Title!" }
//            };

//            Assert.IsFalse(compositeExpression.IsMatch(o3, o3));
//        }
        
//        [TestMethod]
//        public void BooleanExpressionTest_RegexEqualsOperator()
//        {
//            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/foo.*d/"));

//            Assert.IsTrue(e1.IsMatch(null, new JArray("food")));
//            Assert.IsTrue(e1.IsMatch(null, new JArray("fooood and drink")));
//            Assert.IsFalse(e1.IsMatch(null, new JArray("FOOD")));
//            Assert.IsFalse(e1.IsMatch(null, new JArray("foo", "foog", "good")));

//            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/Foo.*d/i"));

//            Assert.IsTrue(e2.IsMatch(null, new JArray("food")));
//            Assert.IsTrue(e2.IsMatch(null, new JArray("fooood and drink")));
//            Assert.IsTrue(e2.IsMatch(null, new JArray("FOOD")));
//            Assert.IsFalse(e2.IsMatch(null, new JArray("foo", "foog", "good")));
//        }

//        [TestMethod]
//        public void BooleanExpressionTest_RegexEqualsOperator_CornerCase()
//        {
//            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/// comment/"));

//            Assert.IsTrue(e1.IsMatch(null, new JArray("// comment")));
//            Assert.IsFalse(e1.IsMatch(null, new JArray("//comment", "/ comment")));

//            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/<tag>.*</tag>/i"));

//            Assert.IsTrue(e2.IsMatch(null, new JArray("<Tag>Test</Tag>", "")));
//            Assert.IsFalse(e2.IsMatch(null, new JArray("<tag>Test<tag>")));
//        }

//        [TestMethod]
//        public void BooleanExpressionTest()
//        {
//            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.LessThan, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

//            Assert.IsTrue(e1.IsMatch(null, new JArray(1, 2, 3, 4, 5)));
//            Assert.IsTrue(e1.IsMatch(null, new JArray(2, 3, 4, 5)));
//            Assert.IsFalse(e1.IsMatch(null, new JArray(3, 4, 5)));
//            Assert.IsFalse(e1.IsMatch(null, new JArray(4, 5)));
//            Assert.IsFalse(e1.IsMatch(null, new JArray("11", 5)));

//            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.LessThanOrEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

//            Assert.IsTrue(e2.IsMatch(null, new JArray(1, 2, 3, 4, 5)));
//            Assert.IsTrue(e2.IsMatch(null, new JArray(2, 3, 4, 5)));
//            Assert.IsTrue(e2.IsMatch(null, new JArray(3, 4, 5)));
//            Assert.IsFalse(e2.IsMatch(null, new JArray(4, 5)));
//            Assert.IsFalse(e1.IsMatch(null, new JArray("11", 5)));
//        }

//        [TestMethod]
//        public void BooleanExpressionTest_GreaterThanOperator()
//        {
//            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.GreaterThan, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

//            Assert.IsTrue(e1.IsMatch(null, new JArray("2", "26")));
//            Assert.IsTrue(e1.IsMatch(null, new JArray(2, 26)));
//            Assert.IsFalse(e1.IsMatch(null, new JArray(2, 3)));
//            Assert.IsFalse(e1.IsMatch(null, new JArray("2", "3")));
//        }

//        [TestMethod]
//        public void BooleanExpressionTest_GreaterThanOrEqualsOperator()
//        {
//            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.GreaterThanOrEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

//            Assert.IsTrue(e1.IsMatch(null, new JArray("2", "26")));
//            Assert.IsTrue(e1.IsMatch(null, new JArray(2, 26)));
//            Assert.IsTrue(e1.IsMatch(null, new JArray(2, 3)));
//            Assert.IsTrue(e1.IsMatch(null, new JArray("2", "3")));
//            Assert.IsFalse(e1.IsMatch(null, new JArray(2, 1)));
//            Assert.IsFalse(e1.IsMatch(null, new JArray("2", "1")));
//        }
//    }
//}