using NUnit.Framework;

namespace Setty.Test
{
    public class TestBase
    {
        [TestFixtureSetUp]
        public void PrepareConfigFiles()
        {
            Helper.PrepareCoreConfigFiles();
        }
    }
}
