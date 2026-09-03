using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class DDDTestAssembly
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            if (!context.Properties.TryGetValue("RunDDDTests", out var value))
            {
                Assert.Inconclusive("DDD integration tests are disabled. Run dotnet test with -p:RunDDDTests=true in a configured integration-test environment.");
                return;
            }

            if (!bool.TryParse(value?.ToString(), out var enabled))
            {
                Assert.Fail("The RunDDDTests test-run parameter must be true or false.");
                return;
            }

            if (!enabled)
                Assert.Inconclusive("DDD integration tests are disabled. Set the RunDDDTests test-run parameter to true to enable them.");
        }
    }
}
