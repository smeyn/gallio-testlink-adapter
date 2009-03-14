/* 
Gallio TestLink Adapter 
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
using Gallio.Runner.Reports;
using Gallio.Model;
using System.Reflection;
using System.Diagnostics;
using TestLinkAPI;

namespace TlGallioAddOn
{
    /// <summary>
    /// this class is responsible for exporting the test run results to TestLink
    /// </summary>
    class ResultExporter
    {
        /// <summary>
        /// built up with all TestLinkFixture Attributes for all assemblies loaded
        /// </summary>
        public Dictionary<string, TestLinkFixtureAttribute> fixtures = new Dictionary<string, TestLinkFixtureAttribute>();

        /// <summary>
        /// the url of the current server
        /// </summary>
        private string serverUrl = "";

        /// <summary>
        /// the proxy to the current server
        /// </summary>
        TestLinkAPI.TestLink proxy = null;

        /// <summary>
        /// list of all projects on the server
        /// </summary>
        private List<TestProject> allProjects;

        /// <summary>
        /// the last project we talked to
        /// </summary>
        private TestProject currentProject = null;

        /// <summary>
        /// list of all testplans for currentProject
        /// </summary>
        private List<TestPlan> plans = null;

        public void RetrieveTestFixture(string path)
        {
            Assembly target = Assembly.LoadFile(path);
            Type[] allTypes = target.GetExportedTypes();
            foreach (Type t in allTypes)
            {
                Debug.WriteLine(string.Format("Examining Type {0}", t.FullName));
                foreach (System.Attribute attribute in t.GetCustomAttributes(typeof(TestLinkFixtureAttribute), false))
                {
                    TestLinkFixtureAttribute tlfa = attribute as TestLinkFixtureAttribute;
                    if (tlfa != null)
                    {
                        fixtures.Add(t.FullName, tlfa);
                    }
                }
            }
        }
        /// <summary>
        /// export the result of the run to TestLink. As a sideeffect, this may create a test case
        /// </summary>
        /// <param name="data"></param>
        public void ReportResult(TestStepRun data)
        {
            try
            {
                TestLinkFixtureAttribute tlfa = getFixture(data.Step.FullName);
                if (tlfa == null)
                {
                    Debug.WriteLine(string.Format("Failed to find testlinkfixture of name {0}", data.Step.FullName));
                    Console.Error.WriteLine("Failed to find testlinkfixture of name {0}", data.Step.FullName);
                    return;
                }
                Debug.WriteLine(string.Format("new Testlink({0}, {1}", tlfa.DevKey, tlfa.Url));


                string TestName = data.Step.Name;

                // get testproject & TestPlanId
                int testPlanId = GetProjectAndPlans(tlfa);
                if (currentProject == null)
                {
                    Console.WriteLine("Failed to find project of name {0}", tlfa.ProjectName);
                    return;
                }
                if (testPlanId == 0)
                {
                    Console.WriteLine("Failed to find testplan of name {0}", tlfa.TestPlan);
                    return;
                }

                int testSuiteId = GetTestSuitedId(currentProject.id, tlfa.TestSuite);
                if (testSuiteId == 0)
                {
                    Console.Error.WriteLine("Failed to find testsuite of name '{0}'", tlfa.TestSuite);
                    return;
                }

                int TCaseId = getTestCaseId(TestName, testSuiteId, tlfa.UserId, currentProject.id, testPlanId);

                if (TCaseId > 0)
                    recordResult(data, tlfa, testPlanId, TCaseId);

            }
            catch (TestLinkException tlex)
            {
                Debug.WriteLine(tlex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred in TestLinkAddOn");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void recordResult(TestStepRun data, TestLinkFixtureAttribute tlfa, int testPlanId, int TCaseId)
        {
            TestCaseResultStatus status = TestCaseResultStatus.Blocked;//= (data.Result.Outcome.Status == TestStatus.Passed)
                //? TestCaseResultStatus.Pass : TestCaseResultStatus.Fail;

            switch (data.Result.Outcome.Status)
            {
                case TestStatus.Passed: status = TestCaseResultStatus.Pass; break;
                case TestStatus.Failed: status = TestCaseResultStatus.Fail; break;
                case TestStatus.Skipped: status = TestCaseResultStatus.Blocked; break;
                case TestStatus.Inconclusive: status = TestCaseResultStatus.Blocked; break;
            }                 

            string notes = data.TestLog.ToString();

            Debug.WriteLine(
                string.Format("ReportTCResult(TCName=\"{0}\", TestPlan=\"{1}\", Status=\"{2}\", Notes=\"{3}\"",
                data.Step.Name,
                tlfa.TestPlan,
                (data.Result.Outcome.Status == TestStatus.Passed) ? "p" : "f",
               notes));

            GeneralResult reportResult = proxy.ReportTCResult(TCaseId, testPlanId, status, notes);

            if (reportResult.status == true)
                Console.WriteLine("Recorded test run result for {0} as {1}",
                    data.Step.Name, data.Result.Outcome);
            else
                Console.Error.WriteLine(string.Format("recorded test result. status={0}, message ='{0}'",
                    reportResult.status, reportResult.message));
        }

        /// <summary>
        /// get a test case id. If the test case does not exist then create one
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="testSuiteId"></param>
        /// <param name="authorId"></param>
        /// <param name="projectId"></param>
        /// <returns>a valid test case id or 0 in case of failure</returns>
        private int getTestCaseId(string testName, int testSuiteId, string authorId, int projectId, int testPlanId)
        {
            int TCaseId = getTestCaseByName(testName, testSuiteId);
            if (TCaseId == 0)
            {
                // need to create test case
                TestCaseCreationResult result = proxy.CreateTestCase(authorId, testSuiteId, testName, projectId,
                    "Automated TestCase", "", "", "", 0,
                    true, TestLink.ActionOnDuplicatedName.Block , 2, 2);
                TCaseId = result.additionalInfo.id;
                int tcExternalId = result.additionalInfo.external_id;
                if (result.status == false)
                {
                    Console.Error.WriteLine("Failed to create TestCase for {0}", testName);
                    Console.Error.WriteLine(" Reason {0}", result.message);
                    return 0;
                }
                string externalId = string.Format("{0}-{1}", currentProject.prefix, tcExternalId);
                int featureId = proxy.addTestCaseToTestPlan(currentProject.id, testPlanId, externalId, result.additionalInfo.version_number);
                if (featureId == 0)
                {
                    Console.Error.WriteLine("Failed to assign TestCase {0} to testplan", testName);
                    return 0;
                }
            }
            return TCaseId;
        }

        /// <summary>
        /// get the test case by this name in this particular test suite
        /// </summary>
        /// <param name="testCaseName"></param>
        /// <param name="testSuiteId">the test suite the test case has to be in</param>
        /// <returns>a valid test case id or 0 if no test case was found</returns>
        private int getTestCaseByName(string testName, int testSuiteId)
        {
            List<TestCaseId> idList = proxy.GetTestCaseIDByName(testName);
            if (idList.Count == 0)
                return 0;
            foreach (TestCaseId tc in idList)
                if (tc.parent_id == testSuiteId)
                    return tc.id;
            return 0;
        }
        /// <summary>
        /// retrieve the testsuite id 
        /// </summary>
        /// <param name="tlfa"></param>
        /// <returns>0 or a valid test suite Id</returns>
        private int GetTestSuitedId(int projectId, string testSuiteName)
        {
            int testSuiteId = 0;
            List<TestSuite> testSuites = proxy.GetFirstLevelTestSuitesForTestProject(projectId); //GetTestSuitesForTestPlan(testPlanId);
            // testsuite must exist. Currently no way of creating them
            foreach (TestSuite ts in testSuites)
                if (ts.name == testSuiteName)
                {
                    testSuiteId = ts.id;
                    break;
                }
            return testSuiteId;
        }
        /// <summary>
        /// get the specific project and associated plans for this TestLinkFixture
        /// </summary>
        /// <param name="tlfa"></param>
        /// <returns>a valid testplanID or 0 if testplan or project was not found</returns>
        private int GetProjectAndPlans(TestLinkFixtureAttribute tlfa)
        {
            // make sure proxy is right
            SetupProxy(tlfa);
            int testPlanId = 0;
            if ((currentProject == null) || (currentProject.name != tlfa.ProjectName))
            {
                currentProject = null;
                plans = null;
                foreach (TestProject project in allProjects)
                    if (project.name == tlfa.ProjectName)
                    {
                        currentProject = project;
                        plans = proxy.GetProjectTestPlans(project.id);
                        break;
                    }
                if (currentProject == null)
                {
                    return 0;
                }
            }
            // now that currentProject and plans are up to date
            foreach (TestPlan plan in plans)
                if (plan.name == tlfa.TestPlan)
                {
                    testPlanId = plan.id;
                    break;
                }
            return testPlanId;
        }

        private void SetupProxy(TestLinkFixtureAttribute tlfa)
        {
            if (serverUrl != tlfa.Url)
            {
                serverUrl = tlfa.Url;
                proxy = new TestLink(tlfa.DevKey, tlfa.Url);
                allProjects = proxy.GetProjects();
            }
        }
        /// <summary>
        /// find a fixture that matches the name of the step
        /// </summary>
        /// <param name="stepName"></param>
        /// <returns>a fixture or null of not found</returns>
        private TestLinkFixtureAttribute getFixture(string stepName)
        {
            // remove the mbunit prefix
            string name = stepName.Substring(stepName.IndexOf('/') + 1);
            // convert
            name = name.Replace('/', '.');
            
            // find the fixture that matches the beginning of this name
            foreach (string fixtureName in fixtures.Keys)
            {
                if (name.StartsWith(fixtureName))
                    return fixtures[fixtureName];
            }
            return null;
        }
    }
}
