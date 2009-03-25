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
using Meyn.TestLink;
using MbUnit.Framework;


namespace tlinkTest
{
    /// <summary>
    /// This is the base class containing common stuff for all test fixtures.
    /// prior to using the smoke tests you need to modify the settings below
    /// and setup the test project. 
    /// </summary>
    public class Testbase
    {
        /// <summary>
        /// this apiKey needs to be set whenever a new user is created.
        /// </summary>
        protected const string apiKey = "ae28ffa45712a041fa0b31dfacb75e29";
        protected const string userName = "admin";

        protected const string testProjectName = "apiSandbox";
        protected const string emptyProjectName = "Empty TestProject";
        /// <summary>
        /// the test plan used for most automated testing
        /// </summary>
        protected const string theTestPlanName = "Automated Testing";
        protected const string testSuiteName2 = "Function Requirements";
        protected const string testSuiteName1 = "business rules";
        protected const string subTestSuiteName1 = "child test suite with test cases";
        protected const string subTestSuiteName2 = "empty Child Test Suite";
        protected const string testCasetoHaveResults = "Test Case with many results";

        private int apiTestProjectId;
        private int emptyProjectId;
        private int businessRulesTestSuiteId;
        private List<TestProject> allProjects = null;
        private TestPlan planCalledAutomatedTesting = null;

        protected TestLink proxy;

        /// <summary>
        /// the id of the project that is used for automated testing of the API
        /// </summary>
        protected int ApiTestProjectId
        {
            get {
                if (apiTestProjectId == 0)
                    loadProjectIds();
                return apiTestProjectId; }
        }
        /// <summary>
        /// get the APITest project
        /// </summary>
        protected TestProject ApiTestProject
        {
            get
            {
                if (allProjects == null)
                    loadProjectIds();
                foreach (TestProject tp in allProjects)
                    if (tp.name == testProjectName)
                        return tp;
                return null ; // not found
            }
        }
        /// <summary>
        /// the id of a project that is emptu
        /// </summary>
        protected int EmptyProjectId
        {
            get
            {
             if (emptyProjectId == 0)
                    loadProjectIds(); 
                return emptyProjectId;
            }
        }
        /// <summary>
        /// the id of the test suite where we can create test cases and record test results
        /// </summary>
        protected int BusinessRulesTestSuiteId
        {
            get
            {
                if (businessRulesTestSuiteId == 0)
                {
                    List<Meyn.TestLink.TestSuite> allSuites = proxy.GetFirstLevelTestSuitesForTestProject(ApiTestProjectId);
                    foreach (Meyn.TestLink.TestSuite ts in allSuites)
                        if (ts.name == testSuiteName2)
                            businessRulesTestSuiteId = ts.id;
                }
                return businessRulesTestSuiteId;
            }
        }

        protected TestPlan PlanCalledAutomatedTesting
        {
            get
            {
                if (planCalledAutomatedTesting == null)
                    planCalledAutomatedTesting = getTestPlan(theTestPlanName);
                return planCalledAutomatedTesting;
            }
        }

        /// <summary>
        /// get a list of all projects;
        /// </summary>
        protected List<TestProject> AllProjects
        {
            get {
                if (allProjects == null)
                    allProjects = proxy.GetProjects();
                return allProjects;
            }

        }
        /// <summary>
        /// in case we want to force a retrieve of all projects
        /// </summary>
        protected void clearAllProjects()
        {
            allProjects = null;
        }

        protected void Setup()
        {
            proxy = new TestLink(apiKey, "http://localhost/testlink/lib/api/xmlrpc.php", true);
        }
        /// <summary>
        /// load a list of all projects
        /// </summary>
        protected void loadProjectIds()
        {
            foreach (TestProject project in AllProjects)
            {
                switch (project.name)
                {
                    case testProjectName: apiTestProjectId = project.id; break;
                    case emptyProjectName: emptyProjectId = project.id; break;
                }
            }
        }

        #region test plan
        TestPlan plan;

        protected TestPlan getTestPlan(string testPlanName)
        {
            List<TestPlan> plans = proxy.GetProjectTestPlans(ApiTestProjectId);
            Assert.IsNotEmpty(plans, "Setup failed, couldn't find testplans for project {0}");
            plan = null;
            foreach (TestPlan candidate in plans)
                if (candidate.name == testPlanName)
                {
                    plan = candidate;
                    break;
                }
            if (plan == null)
                Assert.Fail("Setup failed, could not find test plan named '{0}'", testPlanName);
            return plan;
        }
        #endregion
    }
}
