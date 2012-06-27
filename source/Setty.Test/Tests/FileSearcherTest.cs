using System.Collections.Generic;
using NUnit.Framework;
using Setty.Utils;

namespace Setty.Test.Tests
{
    [TestFixture()]
    public class FileSearcherTest : TestBase
    {
        [Test]
        public void TestFileSearcher()
        {
            var result = FileSearcher.Search(Helper.GetDataPath(), new List<string>() { "Web.config.xslt", "App.config.xslt" });

            Assert.AreEqual(result.Count, 3);
        }
    }
}
