﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Searchlight;
using Searchlight.Parsing;
using Searchlight.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Searchlight.Tests
{
    [TestClass]
    public class LinqExecutorTests
    {
        private DataSource src;

        public class EmployeeObj
        {
            public string name { get; set; }
            public int id { get; set; }
            public DateTime hired { get; set; }
            public decimal paycheck { get; set; }
            public bool onduty { get; set; }
        }

        public List<EmployeeObj> GetTestList()
        {
            List<EmployeeObj> list = new List<EmployeeObj>();
            list.Add(new EmployeeObj()
                {hired = DateTime.Today, id = 1, name = "Alice Smith", onduty = true, paycheck = 1000.00m});
            list.Add(new EmployeeObj()
            {
                hired = DateTime.Today.AddMonths(-1), id = 2, name = "Bob Rogers", onduty = true, paycheck = 1000.00m
            });
            list.Add(new EmployeeObj()
            {
                hired = DateTime.Today.AddMonths(-6), id = 3, name = "Charlie Prentiss", onduty = false,
                paycheck = 800.0m
            });
            list.Add(new EmployeeObj()
            {
                hired = DateTime.Today.AddMonths(-12), id = 4, name = "Danielle O'Shea", onduty = false,
                paycheck = 1200.0m
            });
            list.Add(new EmployeeObj()
            {
                hired = DateTime.Today.AddMonths(1), id = 5, name = "Ernest Nofzinger", onduty = true,
                paycheck = 1000.00m
            });
            list.Add(new EmployeeObj()
                {hired = DateTime.Today.AddMonths(4), id = 6, name = null, onduty = false, paycheck = 10.00m});
            list.Add(new EmployeeObj()
            {
                hired = DateTime.Today.AddMonths(2), id = 7, name = "Roderick 'null' Sqlkeywordtest", onduty = false,
                paycheck = 578.00m
            });
            return list;
        }

        public LinqExecutorTests()
        {
            this.src = DataSource.Create(null, typeof(EmployeeObj), AttributeMode.Loose);
        }

        [TestMethod]
        public void QueryListCollection()
        {
            var list = GetTestList();

            // Construct a simple query and check that it comes out correct
            var syntax = src.Parse("id gt 1 and paycheck le 1000");
            Assert.AreEqual(2, syntax.Filter.Count());
            Assert.AreEqual(ConjunctionType.AND, syntax.Filter[0].Conjunction);
            Assert.AreEqual("id", ((CriteriaClause) syntax.Filter[0]).Column.FieldName);
            Assert.AreEqual(OperationType.GreaterThan, ((CriteriaClause) syntax.Filter[0]).Operation);
            Assert.AreEqual(1, ((CriteriaClause) syntax.Filter[0]).Value);
            Assert.AreEqual("paycheck", ((CriteriaClause) syntax.Filter[1]).Column.FieldName);
            Assert.AreEqual(OperationType.LessThanOrEqual, ((CriteriaClause) syntax.Filter[1]).Operation);
            Assert.AreEqual(1000.0m, ((CriteriaClause) syntax.Filter[1]).Value);

            // Execute the query and ensure that each result matches
            var results = syntax.QueryCollection<EmployeeObj>(list).ToArray();
            Assert.AreEqual(5, results.Length);
            foreach (var e in results)
            {
                Assert.IsTrue(e.id > 1);
                Assert.IsTrue(e.paycheck <= 1000.0m);
            }
        }


        [TestMethod]
        public void NestedClauseQuery()
        {
            var list = GetTestList();

            // Construct a simple query and check that it comes out correct
            var syntax = src.Parse("id gt 1 and (paycheck lt 1000 or paycheck gt 1000)");
            Assert.AreEqual(2, syntax.Filter.Count());
            Assert.AreEqual(ConjunctionType.AND, syntax.Filter[0].Conjunction);
            Assert.AreEqual("id", ((CriteriaClause) syntax.Filter[0]).Column.FieldName);
            Assert.AreEqual(OperationType.GreaterThan, ((CriteriaClause) syntax.Filter[0]).Operation);
            Assert.AreEqual(1, ((CriteriaClause) syntax.Filter[0]).Value);

            // Did we get a nested clause?
            var cc = syntax.Filter[1] as CompoundClause;
            Assert.IsNotNull(cc);
            Assert.AreEqual(2, cc.Children.Count);
            Assert.AreEqual("paycheck", ((CriteriaClause) cc.Children[0]).Column.FieldName);
            Assert.AreEqual(OperationType.LessThan, ((CriteriaClause) cc.Children[0]).Operation);
            Assert.AreEqual(1000.0m, ((CriteriaClause) cc.Children[0]).Value);
            Assert.AreEqual("paycheck", ((CriteriaClause) cc.Children[1]).Column.FieldName);
            Assert.AreEqual(OperationType.GreaterThan, ((CriteriaClause) cc.Children[1]).Operation);
            Assert.AreEqual(1000.0m, ((CriteriaClause) cc.Children[1]).Value);

            // Execute the query and ensure that each result matches
            var results = syntax.QueryCollection<EmployeeObj>(list).ToArray();
            Assert.AreEqual(4, results.Length);
            foreach (var e in results)
            {
                Assert.IsTrue(e.id > 1);
                Assert.IsTrue(e.paycheck is 800.0m or 1200.0m or 10.0m or 578.00m);
            }
        }

        [TestMethod]
        public void BetweenQuery()
        {
            var list = GetTestList();

            // Note that the "between" clause is inclusive
            var syntax = src.Parse("id between 2 and 4");
            Assert.AreEqual(1, syntax.Filter.Count);
            Assert.AreEqual(ConjunctionType.NONE, syntax.Filter[0].Conjunction);
            Assert.AreEqual("id", ((BetweenClause) syntax.Filter[0]).Column.FieldName);
            Assert.AreEqual(2, ((BetweenClause) syntax.Filter[0]).LowerValue);
            Assert.AreEqual(4, ((BetweenClause) syntax.Filter[0]).UpperValue);

            // Execute the query and ensure that each result matches
            var results = syntax.QueryCollection<EmployeeObj>(list).ToArray();
            Assert.AreEqual(3, results.Length);
            foreach (var e in results)
            {
                Assert.IsTrue(e.id > 1);
                Assert.IsTrue(e.id < 5);
            }
        }


        [TestMethod]
        public void StartsWithQuery()
        {
            var list = GetTestList();

            // Note that the "between" clause is inclusive
            var syntax = src.Parse("name startswith 'A'");
            Assert.AreEqual(1, syntax.Filter.Count());
            Assert.AreEqual(ConjunctionType.NONE, syntax.Filter[0].Conjunction);
            Assert.AreEqual("name", ((CriteriaClause) syntax.Filter[0]).Column.FieldName);
            Assert.AreEqual(OperationType.StartsWith, ((CriteriaClause) syntax.Filter[0]).Operation);
            Assert.AreEqual("A", ((CriteriaClause) syntax.Filter[0]).Value);

            // Execute the query and ensure that each result matches
            var results = syntax.QueryCollection<EmployeeObj>(list).ToArray();
            Assert.AreEqual(1, results.Length);
            foreach (var e in results)
            {
                Assert.IsTrue(e.name[0] == 'A');
            }
        }


        [TestMethod]
        public void EndsWithQuery()
        {
            var list = GetTestList();

            // Note that the "between" clause is inclusive
            var syntax = src.Parse("name endswith 's'");
            Assert.AreEqual(1, syntax.Filter.Count());
            Assert.AreEqual(ConjunctionType.NONE, syntax.Filter[0].Conjunction);
            Assert.AreEqual("name", ((CriteriaClause) syntax.Filter[0]).Column.FieldName);
            Assert.AreEqual(OperationType.EndsWith, ((CriteriaClause) syntax.Filter[0]).Operation);
            Assert.AreEqual("s", ((CriteriaClause) syntax.Filter[0]).Value);

            // Execute the query and ensure that each result matches
            var results = syntax.QueryCollection<EmployeeObj>(list).ToArray();
            Assert.AreEqual(2, results.Length);
            foreach (var e in results)
            {
                Assert.IsTrue(e.name.EndsWith("s", StringComparison.OrdinalIgnoreCase));
            }
        }


        [TestMethod]
        public void ContainsQuery()
        {
            var list = GetTestList();

            // Note that the "between" clause is inclusive
            var syntax = src.Parse("name contains 's'");
            Assert.AreEqual(1, syntax.Filter.Count());
            Assert.AreEqual(ConjunctionType.NONE, syntax.Filter[0].Conjunction);
            Assert.AreEqual("name", ((CriteriaClause) syntax.Filter[0]).Column.FieldName);
            Assert.AreEqual(OperationType.Contains, ((CriteriaClause) syntax.Filter[0]).Operation);
            Assert.AreEqual("s", ((CriteriaClause) syntax.Filter[0]).Value);

            // Execute the query and ensure that each result matches
            var results = syntax.QueryCollection<EmployeeObj>(list);
            var resultsArr = results.ToArray();
            Assert.AreEqual(6, resultsArr.Length);
            foreach (var e in resultsArr)
            {
                Assert.IsTrue(e.name.Contains("s", StringComparison.OrdinalIgnoreCase));
            }
        }


        [TestMethod]
        public void NotEqualQuery()
        {
            var list = GetTestList();

            var syntax = src.Parse("Name != 'Alice Smith'");

            var result = syntax.QueryCollection<EmployeeObj>(list);

            Assert.AreEqual(list.Count - 1, result.Count());
            Assert.IsFalse(result.Any(p => p.name == "Alice Smith"));
        }


        [TestMethod]
        public void BooleanContains()
        {
            var list = GetTestList();

            // Note that the "between" clause is inclusive
            Assert.ThrowsException<FieldTypeMismatch>(() =>
            {
                var syntax = src.Parse("onduty contains 's'");
            });
            Assert.ThrowsException<FieldTypeMismatch>(() =>
                {
                    var syntax = src.Parse("onduty contains True");
                }
            );
            Assert.ThrowsException<FieldTypeMismatch>(() =>
                {
                    var syntax = src.Parse("onduty startswith True");
                }
            );
            Assert.ThrowsException<FieldTypeMismatch>(() =>
                {
                    var syntax = src.Parse("onduty endswith True");
                }
            );
        }


        [TestMethod]
        public void IsNullQuery()
        {
            var list = GetTestList();

            var syntax = src.Parse("Name is NULL");

            var result = syntax.QueryCollection<EmployeeObj>(list);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.AreEqual(1, result.Count());
        }


        [TestMethod]
        public void ContainsNull()
        {
            var list = GetTestList();
            // Searchlight interprets the un-apostrophed word "null" here to be the string value "null"
            // instead of a null.
            var syntax = src.Parse("Name contains null");

            var result = syntax.QueryCollection<EmployeeObj>(list);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.Any(p => p.name == "Roderick 'null' Sqlkeywordtest"));
        }


        [TestMethod]
        public void InQuery()
        {
            var list = GetTestList();

            var syntax = src.Parse("name in ('Alice Smith', 'Bob Rogers', 'Sir Not Appearing in this Film')");

            var result = syntax.QueryCollection<EmployeeObj>(list);

            Assert.IsTrue(result.Any(p => p.name == "Alice Smith"));
            Assert.IsTrue(result.Any(p => p.name == "Bob Rogers"));
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }


        [TestMethod]
        public void InQueryInts()
        {
            var list = GetTestList();
            // getting not implemented error on this line
            // make sure using right formatting, if so then in operator needs adjustment
            var syntax = src.Parse("id in (1,2,57)");

            var result = syntax.QueryCollection<EmployeeObj>(list);

            Assert.IsTrue(result.Any(p => p.id == 1));
            Assert.IsTrue(result.Any(p => p.id == 2));
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void InQueryDecimals()
        {
            var list = GetTestList();

            var syntax = src.Parse("paycheck in (578.00, 1.234)");

            var result = syntax.QueryCollection(list);
            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.ToList()[0].id == 7);
        }
    }
}