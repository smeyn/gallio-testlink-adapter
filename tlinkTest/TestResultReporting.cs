/* 
TestLink API Unit Tests
Copyright (c) 2009, Stephan Meyn <stephanmeyn@gmail.com>

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using MbUnit.Framework;
using Meyn.TestLink;
using Meyn.TestLink.GallioExporter;

namespace tlinkTest
{
    /// <summary>
    /// tests the API for functions on storing test run results
    /// </summary>
    /// <remarks>
    /// See class TestBase for basic configuration.
    /// 
    /// This unit test has been marked up with a testlinkfixture attribute. 
    /// This fixture uses the Gallio Testlink Adapter to export results of this test 
    ///  fixture to Testlink. If you do not use the adapter, you can ignore this attribute.
    ///  if you DO use the adapter then you need to make sure that the parameters below are setup corectly
    ///  
    /// Please remember that this tests the API, not testlink.
    /// </remarks>
    [TestFixture]
    [TestLinkFixture(
       Url = "http://localhost/testlink/lib/api/xmlrpc.php",
       ProjectName = "TestLinkApi",
       UserId = "admin",
       TestPlan = "Automatic Testing",
       TestSuite = "TestResultReporting",
       DevKey = "b6e8fee35d143cd018d3b683e0777c51")]
    public class TestResultReporting : Testbase
    {

     //   TestLink proxy;
        int tcIdToHaveResults = 0;
        int tPlanId = 0;
        [SetUp]
        public void setup()
        {
            base.Setup();
            TestPlan plan = getTestPlan(theTestPlanName);
            tPlanId = plan.id;
            
            List<TestCaseId> list =  proxy.GetTestCaseIDByName(testCasetoHaveResults);
            Assert.IsNotEmpty(list, "Failure in Setup. Couldn't find test case");
            tcIdToHaveResults = list[0].id;

        }


        [Test]
        [ExpectedException(typeof(TestLinkException))]
        public void ShouldFailWithInvalidTestCaseId()
        {
            proxy.ReportTCResult(1, tPlanId,  TestCaseResultStatus.Blocked);
        }
        [Test]
        [Category("Changes Database")]
        public void ReportATestCaseExecution()
        {
            GeneralResult result = proxy.ReportTCResult(tcIdToHaveResults, tPlanId, TestCaseResultStatus.Blocked);
            Assert.AreEqual(true, result.status);
        }

        [Test]
        [Category("Changes Database")]
        public void ReportTestCaseExecutionAsPassed()
        {
            GeneralResult result = proxy.ReportTCResult(tcIdToHaveResults, tPlanId, TestCaseResultStatus.Pass);
            Assert.AreEqual(true, result.status);
        }
        [Test]
        [Category("Changes Database")]
        public void ReportTestCaseExecutionAsFailed()
        {
            GeneralResult result = proxy.ReportTCResult(tcIdToHaveResults, tPlanId, TestCaseResultStatus.Fail);
            Assert.AreEqual(true, result.status);
        }

        [Test]
        [Category("Changes Database")]
        public void ReportTestCaseAgainstOlderBuild()
        {
            List<Build> builds = proxy.GetBuildsForTestPlan(PlanCalledAutomatedTesting.id);
            Assert.IsNotEmpty(builds, "Can't proceed. Got empty list of builds for plan");
            // remove inactive builds
            for (int i = builds.Count - 1; i >= 0; i--)
                if (builds[i].is_open == false)
                    builds.Remove(builds[i]);

            Assert.IsTrue(builds.Count>1, "Can't proceed. Need at least two active builds");
            // select oldest build (lowest id)
            Build target = builds[0];
            foreach (Build b in builds)
                if (target.id > b.id)
                    target = b;
            System.Console.WriteLine("Test case id:{0} against test build {1}:{2} recorded as failed",
                                tcIdToHaveResults, target.id, target.name);
            GeneralResult result = proxy.ReportTCResult(tcIdToHaveResults, tPlanId, target.id, TestCaseResultStatus.Fail, "test case assigned to older build");
            Assert.AreEqual(true, result.status);
            
        }

        // the tests below are not valid
        //[Category("Changes Database")]
        //[Test]
        //public void ReportTestCaseExecutionAsUnknown()
        //{
        //    GeneralResult result = proxy.ReportTCResult(tcIdToHaveResults, tPlanId, TestCaseResultStatus.Unknown);
        //    Assert.AreEqual(true, result.status);
        //}
        //[Category("Changes Database")]
        //[Test]
        //public void ReportTestCaseExecutionAsNotRun()
        //{
        //    GeneralResult result = proxy.ReportTCResult(tcIdToHaveResults, tPlanId, TestCaseResultStatus.NotRun);
        //    Assert.AreEqual(true, result.status);
        //}

    }
}
