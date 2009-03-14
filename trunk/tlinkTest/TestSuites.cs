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

using System;
using System.Collections.Generic;
using MbUnit.Framework;
using TestLinkAPI;
using TlGallioAddOn;

namespace tlinkTest
{
    /// <summary>
    /// test TestLinkAPI.TestSuite related calls
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
      TestSuite = "TestSuites",
      DevKey = "ae28ffa45712a041fa0b31dfacb75e29")]
    public class TestSuites : Testbase
    {
        //string apiKey = "ae28ffa45712a041fa0b31dfacb75e29";

       
       // int apiTestProjectId;// = 1291;
      //  int emptyProjectId;
       // string testPlanName = "Automated Testing";
        //int TestLinkAPI.TestSuiteid = 1306;

        //TestProject project;
 

        [SetUp]
        public void setup()
        {
            base.Setup();

            Assert.IsNotEmpty(AllProjects, "Setup failed to get list of projects");
            loadProjectIds();
        }

       
        [Test]
        public void TestSuitesForTestPlan()
        {
            TestPlan plan = getTestPlan(theTestPlanName);

            List<TestLinkAPI.TestSuite> list = proxy.GetTestSuitesForTestPlan(plan.id);
            Assert.IsNotEmpty(list);
            foreach (TestLinkAPI.TestSuite ts in list)
                Console.WriteLine("{0}:{1}", ts.id, ts.name);

        }



        [Test]
        public void TestSuitesForTestProject()
        {
            List<TestLinkAPI.TestSuite> list = proxy.GetFirstLevelTestSuitesForTestProject(ApiTestProjectId);
            Assert.IsNotEmpty(list);
            foreach (TestLinkAPI.TestSuite ts in list)
                Console.WriteLine("{0}:{1}", ts.id, ts.name);

        }
        [Test]
        public void TestSuitesForEmptyTestProject()
        {
            List<TestLinkAPI.TestSuite> list = proxy.GetFirstLevelTestSuitesForTestProject(EmptyProjectId);
            Assert.IsEmpty(list, "empty project is not empty but has {0} test suites", list.Count);
        }
    }
}
