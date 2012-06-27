using System;
using NUnit.Framework;

namespace Setty.Settings.Test.Tests
{
    public class MySettings
    {
        [SettingsProperty("MyApp.Name")]
        public String Name { get; set; }

        [SettingsProperty("MyApp.Value")]
        public String Value { get; set; }

        [SettingsProperty("MyApp.Year")]
        public Int32 Year { get; set; }

        [SettingsProperty("MyApp.Double")]
        public Double Double { get; set; }

        [SettingsProperty("MyApp.Decimal")]
        public Double Decimal { get; set; }

        [SettingsProperty]
        public MyInnerSettings InnerSettings { get; set; }
    }

    public class MyInnerSettings
    {
        [SettingsProperty("MyApp.Name")]
        public String Name { get; set; }

        [SettingsProperty("MyApp.Value")]
        public String Value { get; set; }

        [SettingsProperty("MyApp.Year")]
        public String Year { get; set; }

        [SettingsProperty("MyApp.Boolean.True")]
        public Boolean True { get; set; }

        [SettingsProperty("MyApp.Boolean.False")]
        public Boolean False { get; set; }

        [SettingsProperty("MyApp.Boolean.Yes")]
        public Boolean Yes { get; set; }

        [SettingsProperty("MyApp.Boolean.No")]
        public Boolean No { get; set; }
    }

    [TestFixture]
    public class SettingsMapperTest
    {
        [Test]
        public void SimpleTest()
        {
            var obj1 = SettingsMapper.Map<MySettings>();

            var obj2 = new MySettings();
            SettingsMapper.Map(obj2);

            var obj3 = (MySettings)SettingsMapper.Map(typeof(MySettings), null);

            var obj4 = new MySettings();
            SettingsMapper.Map(obj4);

            Assert.AreNotEqual(obj1, obj2);
            Assert.AreNotEqual(obj1, obj3);
            Assert.AreNotEqual(obj1, obj4);

            Check(obj1);
            Check(obj2);
            Check(obj3);
            Check(obj4);
        }

        public void Check(MySettings obj)
        {
            Assert.AreEqual(obj.Name, "TestName");
            Assert.AreEqual(obj.Value, "Hello");
            Assert.AreEqual(obj.Year, 2011);

            Assert.AreEqual(obj.InnerSettings.True, true);
            Assert.AreEqual(obj.InnerSettings.False, false);

            Assert.AreEqual(obj.InnerSettings.Yes, true);
            Assert.AreEqual(obj.InnerSettings.No, false);

            Assert.AreEqual(obj.Double, 176.23);
            Assert.AreEqual(obj.Decimal, 132476.4523);
        }
    }
}
