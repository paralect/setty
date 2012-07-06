using System;
using System.Collections.Generic;
using System.Dynamic;
using NUnit.Framework;
using Setty.Host;

namespace Setty.Test.Tests
{
    [TestFixture]
    public class ApplicationTest : TestBase
    {
        [Test]
        public void Simple()
        {
            var args = new List<String> {
            };

            var app = new Application(args.ToArray());

            Assert.AreEqual(app.ContextFolder, Environment.CurrentDirectory);
            Assert.AreEqual(app.SettingsFolder, String.Empty);
        }

        [Test]
        public void ContextSpecified()
        {
            const string context = "c:\\some\\path";

            var args = new List<String> {
                String.Format("/context:\"{0}\"", context)
            };

            var app = new Application(args.ToArray());

            Assert.AreEqual(app.ContextFolder, context);
            Assert.AreEqual(app.SettingsFolder, String.Empty);
        }

        [Test]
        public void ContextSpecifiedWithoutQuotes()
        {
            const string context = "c:\\some\\path";

            var args = new List<String> {
                    String.Format("/context:{0}", context)
                };

            var app = new Application(args.ToArray());

            Assert.AreEqual(app.ContextFolder, context);
            Assert.AreEqual(app.SettingsFolder, String.Empty);            
        }

        [Test]
        public void SettingsSpecified()
        {
            const string context = "c:\\some\\path";
            const string settings = "c:\\settings\\path";

            var args = new List<String> {
                String.Format("/context:\"{0}\"", context),
                String.Format("/settings:\"{0}\"", settings),
            };

            var app = new Application(args.ToArray());

            Assert.AreEqual(app.ContextFolder, context);
            Assert.AreEqual(app.SettingsFolder, settings);            
        }

        [Test]
        public void SettingsSpecifiedInAnotherOrder()
        {
            const string context = "c:\\some\\path";
            const string settings = "c:\\settings\\path";

            var args = new List<String> {
                String.Format("/settings:\"{0}\"", settings),
                String.Format("/context:\"{0}\"", context),
            };

            var app = new Application(args.ToArray());

            Assert.AreEqual(app.ContextFolder, context);
            Assert.AreEqual(app.SettingsFolder, settings);
        }

        [Test]
        public void ExpandoPropertiesTest()
        {
            var x = new ExpandoObject() as IDictionary<string, Object>;
            x.Add("NewProp", "test");
            dynamic model = x as dynamic;
            Assert.AreEqual(model.NewProp, "test");
        }
    }
}
