using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using smsSurvery.Surveryer.Utilities;

namespace smsSurvery.Surveryer.Tests.Utility
{
   [TestClass]
   public class SanitizerTests
   {

      private TestContext testContextInstance;
      public TestContext TestContext
      {
         get { return testContextInstance; }
         set { testContextInstance = value; }
      }

      [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "SanitizerInput.csv", "SanitizerInput#csv", DataAccessMethod.Sequential)]
      [DeploymentItem("d:\\Work\\SmsSurvey\\smsSurvery.Surveryer.Tests\\Utility\\SanitizerInput.csv")]
      [TestMethod]
      public void Sanitize_DifferentInputCombinations_ReturnsExpectedOutputs()
      {
         string input = TestContext.DataRow[0] as string;
         string expectedOutput =  TestContext.DataRow[1] as string;
         Assert.AreEqual(expectedOutput, Sanitizer.Sanitize(input));
      }
   }
}
