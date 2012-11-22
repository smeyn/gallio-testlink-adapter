/* 
Nunit TestLink Adapter 
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
using System.Text;
using NUnit.Core.Extensibility;
using NUnit.Core;
using System.Reflection;
using System.IO;

namespace Meyn.TestLink.NUnitExport
{
    /// <summary>
    /// this class is installed by the AddIn and in turn is called by the 
    /// NUnit execution framework when the tests have been run. For any 
    /// NUnit testfixtures that have the TestLinkFixture attribute it reports
    /// the test results back to Testlink
    /// </summary>
    public class ResultExporter:EventListener
    {

        /// <summary>
        /// stores the testLinkFixtureAttributes against class names
        /// </summary>
        public Dictionary<string, TestLinkFixtureAttribute> fixtures = new Dictionary<string, TestLinkFixtureAttribute>();       

        /// <summary>
        ///  handles all comms to Testlink
        /// </summary>
        TestLinkAdaptor adaptor = new TestLinkAdaptor();

        private string currentTestOutput  = "";

        /// <summary>
        /// uses the Nunit trace facility. To set the trace levels
        /// you need to modify the nunit-console.exe.config file
        /// </summary>
        static Logger log = InternalTrace.GetLogger(typeof(TestLinkAddOn));

 

        #region EventListener Overrides
        public void RunStarted(string name, int testCount)
        {
        }

        /// <summary>
        /// called at the end. Here we export the results to test link
        /// </summary>
        /// <param name="result"></param>
        public void RunFinished(TestResult result)
        {
            log.Debug(string.Format("RunFinished Description='{0}', Success={1}, Name='{2}', Message='{3}'", result.Description,
            result.IsSuccess, result.Name, result.Message));
            //result.Name is the fully qualified dll path?


            log.Info("Starting to export results to TestLink");

            processResults(result);
            
            log.Info("Completed exporting results to TestLink");
        }

 
        public void RunFinished(Exception exception)
        {
        }
        public void TestStarted(TestName testName)
        {
            currentTestOutput = "";
        }
        /// <summary>
        /// a test has finished. 
        /// </summary>
        /// <param name="result"></param>
        public void TestFinished(TestResult result)
        {
            log.Debug(String.Format("  Test Finished Description='{0}', Success={1}, Name='{2}', Message='{3}'", result.Description,
            result.IsSuccess, result.Name, result.Message));
        }

        public void SuiteStarted(TestName testName)
        {
        }
        public void SuiteFinished(TestResult result)
        {
            //log.Debug(string.Format("SuiteFinished Description='{0}', Success={1}, Name='{2}', Message='{3}'", 
                
            //    result.Description,
            //result.IsSuccess, result.Name, result.Message));
        }
        public void UnhandledException(Exception exception)
        {
        }
        /// <summary>
        /// capture any console output during the testing.
        /// </summary>
        /// <param name="testOutput"></param>
        public void TestOutput(TestOutput testOutput)
        {
            currentTestOutput = testOutput.Text;
        }

        #endregion
        /// <summary>
        /// extracts the type name of the test fixture name.
        /// Assumes the test fixture name is the fully qualified testmethod name.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string extractTestFixture(string path)
        {
            log.Debug(String.Format("Extracting Test Fixture from '{0}'", path));
            int index = path.LastIndexOf(".");
            if (index < 1)
                return "";

            string candidate = path.Substring(0, index);
            return candidate;
        }

        private string lastTestFixtureName = "";


        /// <summary>
        /// parse results and sub results. If it is a test case then try to record it in testlink
        /// </summary>
        /// <param name="result"></param>
        private void processResults(TestResult result)
        {

            if (IsDllPath(result.Name))
                extractTestFixtureAttribute(result.Name);

            if (result.HasResults)
            {

                foreach (TestResult subResult in result.Results)
                {
                    processResults(subResult);
                }
            }
            else 
            {
               
                string testFixtureName = extractTestFixture(result.FullName);
                log.Debug(string.Format("Processing results for test {0} in fixture: {1}",result.Name, testFixtureName));
                if (fixtures.ContainsKey(testFixtureName))
                {
                    Meyn.TestLink.TestLinkFixtureAttribute tlfa = fixtures[testFixtureName];
                    //tlfa.ConsiderConfigFile(); // ensure that a config file is read in
                    reportResult(result, tlfa);
                }
                else
                {
                    if (lastTestFixtureName != testFixtureName) // do this warning once per test fixture
                    {
                        log.Warning(string.Format("Test fixture '{0}' has no TestLinkFixture attribute",
                            testFixtureName));
                        lastTestFixtureName = testFixtureName;
                    }
                    log.Warning(string.Format("Failed to record test case '{0}'", result.Name));
                }
            }
        }

        private bool IsDllPath(string path)
        {
            log.Debug("IsDllPath:");
            log.Debug(path);
            bool result = (path.ToLower().EndsWith(".dll")); 
            return result;
        }

        /// <summary>
        /// gather all the necessary information prior to 
        /// reporting the results to testlink.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="tlfa"></param>
        private void reportResult(TestResult result, Meyn.TestLink.TestLinkFixtureAttribute tlfa)
        {

            adaptor.ConnectionData = tlfa; // update the connection and retrieve  key base data from testlink
 
            try
            {
                string TestName = result.Name;

                if (adaptor.ConnectionValid == false)
                {
                    log.Warning(string.Format("Failed to export tesult for testcase {0}", result.Name));
                    Console.WriteLine("Can't export results because invalid connection");
                    return;
                }

                try
                {
                    int TCaseId = adaptor.GetTestCaseId(TestName);

                    if (TCaseId > 0)
                    {
                        sendResultToTestlink(result, tlfa, TCaseId);
                        //Console.WriteLine("exported testcase '{0}'. ", TestName);
                    }
                }
                catch (TestLinkException tlex)
                {
                    Console.WriteLine("Failed to export testcase '{0}'. {1}", TestName, tlex.Message);
                    log.Error(string.Format("Failed to export testcase '{0}'. {1}", TestName, tlex.Message));
                }
            }
            catch (TestLinkException tlex)
            {
                log.Error(tlex.Message, tlex);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// after everything has been setup, record the actual result.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="tlfa"></param>
        /// <param name="testPlanId"></param>
        /// <param name="TCaseId"></param>
        private void sendResultToTestlink(TestResult tcResult , TestLinkFixtureAttribute tlfa,  int TCaseId)
        {
            TestCaseResultStatus status = TestCaseResultStatus.Blocked;

            StringBuilder notes = new StringBuilder();
            notes.AppendLine(tcResult.Message);
            notes.AppendLine(currentTestOutput);

            switch (tcResult.ResultState)  //RunState)
            {
                case ResultState.NotRunnable:
                    status = TestCaseResultStatus.Blocked;
                    break;
                case ResultState.Skipped:
                    status = TestCaseResultStatus.Blocked;
                    notes.AppendLine ("++++ SKIPPED +++");
                    break;
                case ResultState.Ignored:
                    status = TestCaseResultStatus.Blocked;
                    notes.AppendLine("++++ IGNORED +++");
                    break;
                case ResultState.Success: status = TestCaseResultStatus.Pass; break;
                case ResultState.Failure: status = TestCaseResultStatus.Fail; break;
                case ResultState.Error: status = TestCaseResultStatus.Fail; break;                  
            }

            GeneralResult result = adaptor.RecordTheResult(TCaseId, status, notes.ToString());
            if (result.status != true)
            {
                Console.WriteLine("Failed to export Result. Testlink reported: '{0}'", result.message);
                log.Warning(string.Format("Failed to export Result. Testlink reported: '{0}'", result.message));
            }
            else
             log.Info(
                string.Format("Reported Result (TCName=\"{0}\", TestPlan=\"{1}\", Status=\"{2}\").",
                tcResult.Name,
                tlfa.TestPlan,
                tcResult.ResultState.ToString()));          
        }

        /// <summary>
        /// load the dll and extract the testfixture attribute from each class
        /// </summary>
        /// <param name="path"></param>
        private void extractTestFixtureAttribute(string path)
        {

            // assembly loading requires an absolute path
            if (Path.IsPathRooted(path) == false)
            {
                DirectoryInfo di = new DirectoryInfo(".");
                path = Path.Combine(di.FullName, path);
            }       
            
            log.Debug(string.Format("Loading assembly '{0}'", path));
            Assembly target = Assembly.LoadFile(path);
            Type[] allTypes = target.GetExportedTypes();

            foreach (Type t in allTypes)
            {
                log.Debug(string.Format("Examining Type {0}", t.FullName));
                foreach (System.Attribute attribute in t.GetCustomAttributes(typeof(TestLinkFixtureAttribute), false))
                {
                    TestLinkFixtureAttribute tlfa = attribute as TestLinkFixtureAttribute;
                    if (tlfa != null)
                    {
                        tlfa.ConsiderConfigFile(Path.GetDirectoryName(path)); // trigger the attribute to look for a config file which may overload individual items
                        log.Info(string.Format("Found fixture attribute for test fixture: {0}", t.FullName));
                        if (fixtures.ContainsKey(t.FullName))
                            fixtures[t.FullName] = tlfa;
                        else
                            fixtures.Add(t.FullName, tlfa);
                    }
                }
            }
        }


    }
}
