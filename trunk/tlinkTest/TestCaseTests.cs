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
using Meyn.TestLink;
using Meyn.TestLink.GallioExporter;

namespace tlinkTest
{
  
    /// <summary>
    /// test test case related functions.
    /// Excludes test case creation which has its own test fixture
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
        ProjectName="TestLinkApi",
        UserId = "admin",
        TestPlan= "Automatic Testing",
        TestSuite = "TestCaseTests",
        DevKey = "b6e8fee35d143cd018d3b683e0777c51")]
    public class TestCaseTests  :Testbase
    {
        [SetUp]
        public void setup()
        {
            base.Setup();
            Assert.IsNotNull(AllProjects);           
        }
        [Test]
        public void GetNonExistentTc()
        {
           List<TestCaseId> idList = proxy.GetTestCaseIDByName("Does Not Exist");
           Assert.IsEmpty(idList);
        }

        [Test]
        public void DealWithDuplicateTestCases()
        {
            List<TestCaseId> idList = proxy.GetTestCaseIDByName("duplicate test case");
            Assert.AreEqual(2, idList.Count, "there should be two test cases");
            Console.WriteLine("Business Rules Test Suite Id = {0}", base.BusinessRulesTestSuiteId);
           
            foreach (TestCaseId id in idList)
            {
                  Console.WriteLine("TC-id {0}, parent Id = {1}", id.tc_external_id, id.parent_id);
            }
        }

    }
}
