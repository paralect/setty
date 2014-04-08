using System.Collections.Generic;
using NUnit.Framework;

namespace Setty.Test.Tests
{
    [TestFixture]
    public class SettingsModelTest
    {
        [Test]
        public void key_not_found()
        {
            var model = new SettingsModel(new Dictionary<string, string>());
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                var t = model["test"];
            });
            Assert.AreEqual("Key test was not found in settings file", ex.Message);
        } 
        
        [Test]
        public void key_not_found2()
        {
            var model = new SettingsModel(new Dictionary<string, string>());
            var ex = Assert.Throws<KeyNotFoundException>(() =>
            {
                var t = model["AnotherKey"];
            });
            Assert.AreEqual("Key AnotherKey was not found in settings file", ex.Message);
        }
    }
}