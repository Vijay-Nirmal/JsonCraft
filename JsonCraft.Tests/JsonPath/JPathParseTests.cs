
using JsonCraft.JsonPath;
using System.Text.Json;

namespace JsonCraft.Tests.JsonPath
{
    [TestClass]
    public class JPathParse
    {
        [TestMethod]
        public void BooleanQuery_TwoValues()
        {
            JPath path = new JPath("[?(1 > 2)]");
            Assert.AreEqual(1, path.Filters.Count);
            BooleanQueryExpression booleanExpression = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(1, ((JsonElement)booleanExpression.Left).GetInt32());
            Assert.AreEqual(2, ((JsonElement)booleanExpression.Right).GetInt32());
            Assert.AreEqual(QueryOperator.GreaterThan, booleanExpression.Operator);
        }

        [TestMethod]
        public void BooleanQuery_TwoPaths()
        {
            JPath path = new JPath("[?(@.price > @.max_price)]");
            Assert.AreEqual(1, path.Filters.Count);
            BooleanQueryExpression booleanExpression = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> leftPaths = (List<PathFilter>)booleanExpression.Left;
            List<PathFilter> rightPaths = (List<PathFilter>)booleanExpression.Right;

            Assert.AreEqual("price", ((FieldFilter)leftPaths[0]).Name.Value.Span.ToString());
            Assert.AreEqual("max_price", ((FieldFilter)rightPaths[0]).Name.Value.Span.ToString());
            Assert.AreEqual(QueryOperator.GreaterThan, booleanExpression.Operator);
        }

        [TestMethod]
        public void SingleProperty()
        {
            JPath path = new JPath("Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SingleQuotedProperty()
        {
            JPath path = new JPath("['Blah']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SingleQuotedPropertyWithWhitespace()
        {
            JPath path = new JPath("[  'Blah'  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SingleQuotedPropertyWithDots()
        {
            JPath path = new JPath("['Blah.Ha']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah.Ha", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SingleQuotedPropertyWithBrackets()
        {
            JPath path = new JPath("['[*]']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("[*]", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SinglePropertyWithRoot()
        {
            JPath path = new JPath("$.Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SinglePropertyWithRootWithStartAndEndWhitespace()
        {
            JPath path = new JPath(" $.Blah ");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void RootWithBadWhitespace()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath("$ .Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [TestMethod]
        public void NoFieldNameAfterDot()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath("$.Blah."); }, @"Unexpected end while parsing path.");
        }

        [TestMethod]
        public void RootWithBadWhitespace2()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath("$. Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [TestMethod]
        public void WildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.*");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void WildcardArrayWithRoot()
        {
            JPath path = new JPath("$.[*]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void RootArrayNoDot()
        {
            JPath path = new JPath("$[1]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void WildcardArray()
        {
            JPath path = new JPath("[*]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void WildcardArrayWithProperty()
        {
            JPath path = new JPath("[ * ].derp");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.AreEqual("derp", ((FieldFilter)path.Filters[1]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void QuotedWildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.['*']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("*", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SingleScanWithRoot()
        {
            JPath path = new JPath("$..Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((ScanFilter)path.Filters[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void QueryTrue()
        {
            JPath path = new JPath("$.elements[?(true)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("elements", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            Assert.AreEqual(QueryOperator.Exists, ((QueryFilter)path.Filters[1]).Expression.Operator);
        }

        [TestMethod]
        public void ScanQuery()
        {
            JPath path = new JPath("$.elements..[?(@.id=='AAA')]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("elements", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());

            BooleanQueryExpression expression = (BooleanQueryExpression)((QueryScanFilter)path.Filters[1]).Expression;

            List<PathFilter> paths = (List<PathFilter>)expression.Left;

            Assert.IsInstanceOfType(paths[0], typeof(FieldFilter));
        }

        [TestMethod]
        public void WildcardScanWithRoot()
        {
            JPath path = new JPath("$..*");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void WildcardScanWithRootWithWhitespace()
        {
            JPath path = new JPath("$..* ");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void TwoProperties()
        {
            JPath path = new JPath("Blah.Two");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            Assert.AreEqual("Two", ((FieldFilter)path.Filters[1]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void OnePropertyOneScan()
        {
            JPath path = new JPath("Blah..Two");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            Assert.AreEqual("Two", ((ScanFilter)path.Filters[1]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SinglePropertyAndIndexer()
        {
            JPath path = new JPath("Blah[0]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
        }

        [TestMethod]
        public void SinglePropertyAndExistsQuery()
        {
            JPath path = new JPath("Blah[ ?( @..name ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Exists, expressions.Operator);
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("name", ((ScanFilter)paths[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithWhitespace()
        {
            JPath path = new JPath("Blah[ ?( @.name=='hi' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("hi", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithEscapeQuote()
        {
            JPath path = new JPath(@"Blah[ ?( @.name=='h\'i' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("h'i", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithDoubleEscape()
        {
            JPath path = new JPath(@"Blah[ ?( @.name=='h\\i' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("h\\i", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithRegexAndOptions()
        {
            JPath path = new JPath("Blah[ ?( @.name=~/hi/i ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual("/hi/i", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithRegex()
        {
            JPath path = new JPath("Blah[?(@.title =~ /^.*Sword.*$/)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual("/^.*Sword.*$/", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithEscapedRegex()
        {
            JPath path = new JPath(@"Blah[?(@.title =~ /[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual(@"/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithOpenRegex()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath(@"Blah[?(@.title =~ /[\"); }, "Path ended with an open regex.");
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithUnknownEscape()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath(@"Blah[ ?( @.name=='h\i' ) ]"); }, @"Unknown escape character: \i");
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithFalse()
        {
            JPath path = new JPath("Blah[ ?( @.name==false ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(false, ((JsonElement)expressions.Right).GetBoolean());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithTrue()
        {
            JPath path = new JPath("Blah[ ?( @.name==true ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(true, ((JsonElement)expressions.Right).GetBoolean());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithNull()
        {
            JPath path = new JPath("Blah[ ?( @.name==null ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(JsonValueKind.Null, ((JsonElement)expressions.Right).ValueKind);
        }

        [TestMethod]
        public void FilterWithScan()
        {
            JPath path = new JPath("[?(@..name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual("name", ((ScanFilter)paths[0]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void FilterWithNotEquals()
        {
            JPath path = new JPath("[?(@.name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.NotEquals, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithNotEquals2()
        {
            JPath path = new JPath("[?(@.name!=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.NotEquals, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithLessThan()
        {
            JPath path = new JPath("[?(@.name<null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.LessThan, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithLessThanOrEquals()
        {
            JPath path = new JPath("[?(@.name<=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.LessThanOrEquals, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithGreaterThan()
        {
            JPath path = new JPath("[?(@.name>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.GreaterThan, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithGreaterThanOrEquals()
        {
            JPath path = new JPath("[?(@.name>=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.GreaterThanOrEquals, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithInteger()
        {
            JPath path = new JPath("[?(@.name>=12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(12, ((JsonElement)expressions.Right).GetInt32());
        }

        [TestMethod]
        public void FilterWithNegativeInteger()
        {
            JPath path = new JPath("[?(@.name>=-12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(-12, ((JsonElement)expressions.Right).GetInt32());
        }

        [TestMethod]
        public void FilterWithFloat()
        {
            JPath path = new JPath("[?(@.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(12.1d, ((JsonElement)expressions.Right).GetDouble());
        }

        [TestMethod]
        public void FilterExistWithAnd()
        {
            JPath path = new JPath("[?(@.name&&@.title)]");
            CompositeExpression expressions = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.And, expressions.Operator);
            Assert.AreEqual(2, expressions.Expressions.Count);

            var first = (BooleanQueryExpression)expressions.Expressions[0];
            var firstPaths = (List<PathFilter>)first.Left;
            Assert.AreEqual("name", ((FieldFilter)firstPaths[0]).Name.Value.Span.ToString());
            Assert.AreEqual(QueryOperator.Exists, first.Operator);

            var second = (BooleanQueryExpression)expressions.Expressions[1];
            var secondPaths = (List<PathFilter>)second.Left;
            Assert.AreEqual("title", ((FieldFilter)secondPaths[0]).Name.Value.Span.ToString());
            Assert.AreEqual(QueryOperator.Exists, second.Operator);
        }

        [TestMethod]
        public void FilterExistWithAndOr()
        {
            JPath path = new JPath("[?(@.name&&@.title||@.pie)]");
            CompositeExpression andExpression = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.And, andExpression.Operator);
            Assert.AreEqual(2, andExpression.Expressions.Count);

            var first = (BooleanQueryExpression)andExpression.Expressions[0];
            var firstPaths = (List<PathFilter>)first.Left;
            Assert.AreEqual("name", ((FieldFilter)firstPaths[0]).Name.Value.Span.ToString());
            Assert.AreEqual(QueryOperator.Exists, first.Operator);

            CompositeExpression orExpression = (CompositeExpression)andExpression.Expressions[1];
            Assert.AreEqual(2, orExpression.Expressions.Count);

            var orFirst = (BooleanQueryExpression)orExpression.Expressions[0];
            var orFirstPaths = (List<PathFilter>)orFirst.Left;
            Assert.AreEqual("title", ((FieldFilter)orFirstPaths[0]).Name.Value.Span.ToString());
            Assert.AreEqual(QueryOperator.Exists, orFirst.Operator);

            var orSecond = (BooleanQueryExpression)orExpression.Expressions[1];
            var orSecondPaths = (List<PathFilter>)orSecond.Left;
            Assert.AreEqual("pie", ((FieldFilter)orSecondPaths[0]).Name.Value.Span.ToString());
            Assert.AreEqual(QueryOperator.Exists, orSecond.Operator);
        }

        [TestMethod]
        public void FilterWithRoot()
        {
            JPath path = new JPath("[?($.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual(2, paths.Count);
            Assert.IsInstanceOfType(paths[0], typeof(RootFilter));
            Assert.IsInstanceOfType(paths[1], typeof(FieldFilter));
        }

        [TestMethod]
        public void BadOr1()
        {
            Assert.ThrowsException<JsonException>(() => new JPath("[?(@.name||)]"), "Unexpected character while parsing path query: )");
        }

        [TestMethod]
        public void BaddOr2()
        {
            Assert.ThrowsException<JsonException>(() => new JPath("[?(@.name|)]"), "Unexpected character while parsing path query: |");
        }

        [TestMethod]
        public void BaddOr3()
        {
            Assert.ThrowsException<JsonException>(() => new JPath("[?(@.name|"), "Unexpected character while parsing path query: |");
        }

        [TestMethod]
        public void BaddOr4()
        {
            Assert.ThrowsException<JsonException>(() => new JPath("[?(@.name||"), "Path ended with open query.");
        }

        [TestMethod]
        public void NoAtAfterOr()
        {
            Assert.ThrowsException<JsonException>(() => new JPath("[?(@.name||s"), "Unexpected character while parsing path query: s");
        }

        [TestMethod]
        public void NoPathAfterAt()
        {
            Assert.ThrowsException<JsonException>(() => new JPath("[?(@.name||@"), @"Path ended with open query.");
        }

        [TestMethod]
        public void NoPathAfterDot()
        {
            Assert.ThrowsException<JsonException>(() => new JPath("[?(@.name||@."), @"Unexpected end while parsing path.");
        }

        [TestMethod]
        public void NoPathAfterDot2()
        {
            Assert.ThrowsException<JsonException>(() => new JPath("[?(@.name||@.)]"), @"Unexpected end while parsing path.");
        }

        [TestMethod]
        public void FilterWithFloatExp()
        {
            JPath path = new JPath("[?(@.name>=5.56789e+0)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(5.56789e+0, ((JsonElement)expressions.Right).GetDouble());
        }

        [TestMethod]
        public void MultiplePropertiesAndIndexers()
        {
            JPath path = new JPath("Blah[0]..Two.Three[1].Four");
            Assert.AreEqual(6, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.AreEqual("Two", ((ScanFilter)path.Filters[2]).Name.Value.Span.ToString());
            Assert.AreEqual("Three", ((FieldFilter)path.Filters[3]).Name.Value.Span.ToString());
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[4]).Index);
            Assert.AreEqual("Four", ((FieldFilter)path.Filters[5]).Name.Value.Span.ToString());
        }

        [TestMethod]
        public void BadCharactersInIndexer()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath("Blah[[0]].Two.Three[1].Four"); }, @"Unexpected character while parsing path indexer: [");
        }

        [TestMethod]
        public void UnclosedIndexer()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath("Blah[0"); }, @"Path ended with open indexer.");
        }

        [TestMethod]
        public void IndexerOnly()
        {
            JPath path = new JPath("[111119990]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void IndexerOnlyWithWhitespace()
        {
            JPath path = new JPath("[  10  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(10, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void MultipleIndexes()
        {
            JPath path = new JPath("[111119990,3]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [TestMethod]
        public void MultipleIndexesWithWhitespace()
        {
            JPath path = new JPath("[   111119990  ,   3   ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [TestMethod]
        public void MultipleQuotedIndexes()
        {
            JPath path = new JPath("['111119990','3']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.AreEqual("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.AreEqual("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [TestMethod]
        public void MultipleQuotedIndexesWithWhitespace()
        {
            JPath path = new JPath("[ '111119990' , '3' ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.AreEqual("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.AreEqual("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [TestMethod]
        public void SlicingIndexAll()
        {
            JPath path = new JPath("[111119990:3:2]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndex()
        {
            JPath path = new JPath("[111119990:3]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndexNegative()
        {
            JPath path = new JPath("[-111119990:-3:-2]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndexEmptyStop()
        {
            JPath path = new JPath("[  -3  :  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndexEmptyStart()
        {
            JPath path = new JPath("[ : 1 : ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(1, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndexWhitespace()
        {
            JPath path = new JPath("[  -111119990  :  -3  :  -2  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void EmptyIndexer()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath("[]"); }, "Array index expected.");
        }

        [TestMethod]
        public void IndexerCloseInProperty()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath("]"); }, "Unexpected character while parsing path: ]");
        }

        [TestMethod]
        public void AdjacentIndexers()
        {
            JPath path = new JPath("[1][0][0][" + int.MaxValue + "]");
            Assert.AreEqual(4, path.Filters.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[2]).Index);
            Assert.AreEqual(int.MaxValue, ((ArrayIndexFilter)path.Filters[3]).Index);
        }

        [TestMethod]
        public void MissingDotAfterIndexer()
        {
            Assert.ThrowsException<JsonException>(() => { new JPath("[1]Blah"); }, "Unexpected character following indexer: B");
        }

        [TestMethod]
        public void PropertyFollowingEscapedPropertyName()
        {
            JPath path = new JPath("frameworks.dnxcore50.dependencies.['System.Xml.ReaderWriter'].source");
            Assert.AreEqual(5, path.Filters.Count);

            Assert.AreEqual("frameworks", ((FieldFilter)path.Filters[0]).Name.Value.Span.ToString());
            Assert.AreEqual("dnxcore50", ((FieldFilter)path.Filters[1]).Name.Value.Span.ToString());
            Assert.AreEqual("dependencies", ((FieldFilter)path.Filters[2]).Name.Value.Span.ToString());
            Assert.AreEqual("System.Xml.ReaderWriter", ((FieldFilter)path.Filters[3]).Name.Value.Span.ToString());
            Assert.AreEqual("source", ((FieldFilter)path.Filters[4]).Name.Value.Span.ToString());
        }
    }
}