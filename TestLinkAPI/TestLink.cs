/* 
TestLink API library
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
using CookComputing.XmlRpc;

namespace Meyn.TestLink
{
    public enum TestCaseResultStatus { Pass, Fail, Blocked}

    /// <summary>
    /// this is the proxy class to connect to TestLink.
    /// It provides a list of functions that map straight into the Tstlink API as it stands at V 1.8 RC5
    /// </summary>
    /// <remarks>This class makes use of XML-RPC.NET Copyright (c) 2006 Charles Cook</remarks>
    public class TestLink
    {
        string devkey = "";
        ITestLink proxy = null;
        public bool log = false;

        #region constructors
        /// <summary>
        /// default constructor 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="url"></param>
        public TestLink(string apiKey, string url)
            : this(apiKey, url, false)
        {
        }
        /// <summary>
        /// default constructor wothout URL. Uses default localhost url
        /// </summary>
        /// <param name="apiKey"></param>
        public TestLink(string apiKey) : this(apiKey, "", false) { }

        /// <summary>
        /// constructor with debug key
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="log"></param>
        public TestLink(string apiKey, bool log): this(apiKey,"",log)
        {
        }

        public TestLink(string apiKey, string url, bool log)
        {
            devkey = apiKey;
            proxy = XmlRpcProxyGen.Create<ITestLink>();
            if (log)
            {
                proxy.RequestEvent += new XmlRpcRequestEventHandler(myHandler);
                proxy.ResponseEvent += new XmlRpcResponseEventHandler(proxy_ResponseEvent);
            }
            if ((url != null) && (url != string.Empty))
                proxy.Url = url;
        }
        #endregion

        #region logging
        private string lastRequest;
        /// <summary>
        /// last xmlrpc request sent to testlink. only works if debug was sent on construction
        /// </summary>
        public string LastRequest
        {
            get { return lastRequest; }
        }
        private string lastResponse;
        /// <summary>
        /// debug last response reseved from testlink xmlrpc call. only works if debug was sent on construction
        /// </summary>
        public string LastResponse
        {
            get { return lastResponse; }
        }
        void proxy_ResponseEvent(object sender, XmlRpcResponseEventArgs args)
        {
            args.ResponseStream.Seek(0, System.IO.SeekOrigin.Begin);
            System.IO.StreamReader sr = new System.IO.StreamReader(args.ResponseStream);
            lastResponse = sr.ReadToEnd();
        }

        public void myHandler(object sender, CookComputing.XmlRpc.XmlRpcRequestEventArgs args)
        {
            long l = args.RequestStream.Length;
            args.RequestStream.Seek(0, System.IO.SeekOrigin.Begin);
            System.IO.StreamReader sr = new System.IO.StreamReader(args.RequestStream);
            lastRequest = sr.ReadToEnd();
        }
        #endregion
        #region error responses
        /// <summary>
        /// generic message handler.
        /// </summary>
        /// <param name="o"></param>
        private void handleErrorMessage(object errorMessage)
        {
            if (errorMessage is object[])
                handleErrorMessage(errorMessage as object[]);
            
        }
        private void handleErrorMessage(object[] errorMessage)
        {
            List<TLErrorMessage> errs = decodeErrors(errorMessage);
            if (errs.Count > 0)
            {
                string msg = string.Format("{0}:{1}", errs[0].code, errs[0].message);
                throw new TestLinkException(msg, errs);
            }

        }
        private List<TLErrorMessage> decodeErrors(object messageList)
        {
            return decodeErrors(messageList as object[]);
        }
        /// <summary>
        /// try to conver the struct to an error message. Return null if it wasn't one
        /// </summary>
        /// <param name="message"></param>
        /// <returns>a TLErrorMessage or null</returns>
        private List<TLErrorMessage> decodeErrors(object [] messageList)
        {
            List<TLErrorMessage> result= new List<TLErrorMessage>();
            if (messageList == null)
                return result;
            foreach (XmlRpcStruct message in messageList)
            {
                if (message.ContainsKey("code") && message.ContainsKey("message"))
                  result.Add(new TLErrorMessage(message));
            }
            return result;
        }
        private TLErrorMessage decodeSingleError(XmlRpcStruct message)
        {
            if (message.ContainsKey("code") && message.ContainsKey("message"))
                return new TLErrorMessage(message);
            else
                return null;
        }

        #endregion

        /// <summary>
        /// add a test case to a test plan (no way of removing one)
        /// </summary>
        /// <param name="testprojectid"></param>
        /// <param name="testplanid"></param>
        /// <param name="testcaseexternalid">the id that is displayed on the GUI</param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <remarks>this testExternalid is a string and a concatenation of the 
        /// test project prefix and the externalid that is reported in the test case creation.
        /// </remarks>
        public int addTestCaseToTestPlan(int testprojectid, int testplanid, string testcaseexternalid, int version)
        {
            object o = proxy.addTestCaseToTestPlan(devkey, testprojectid, testplanid, testcaseexternalid, version);
            handleErrorMessage(o);
            if (o is XmlRpcStruct)
            {
                XmlRpcStruct data = o as XmlRpcStruct;
                if ((data != null) && (data.ContainsKey("feature_id")))
                {
                    string val = (string)data["feature_id"];
                    int result;
                    bool good = int.TryParse(val, out result);
                    if (good)
                        return result;

                }                
            }           
            return 0;
        }

        /// <summary>
        /// create a build for a testplan
        /// </summary>
        /// <param name="testplanid"></param>
        /// <param name="buildname"></param>
        /// <param name="buildnotes"></param>
        /// <returns></returns>
        public GeneralResult CreateBuild(int testplanid, string buildname, string buildnotes)
        {
            object[] o = proxy.createBuild(devkey, testplanid, buildname, buildnotes);
            handleErrorMessage(o);
            foreach (XmlRpcStruct data in o)
                return new GeneralResult(data);
            return null;
        }
        public enum ActionOnDuplicatedName { Block, GenerateNew, CreateNewVersion }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="testsuiteid"></param>
        /// <param name="testcasename"></param>
        /// <param name="testprojectid"></param>
        /// <param name="summary"></param>
        /// <param name="steps">each step enclosed in html paragraph blocks '&lt;p&gt; text '&lt;/p&gt;'</param>
        /// <param name="expectedresults">each step enclosed in html paragraph blocks'&lt;p&gt; text '&lt;/p&gt;'</param>
        /// <param name="keywords"></param>
        /// <param name="order">defaultOrder = 0, otherwise location in sequence to other testcases</param>
        /// <param name="checkduplicatedname">1=yes, 0=no</param>
        /// <param name="actiononduplicatedname">one of block, generate_new, create_new_version</param>
        /// <param name="executiontype">manual:1, automated: 2, </param>
        /// <param name="importance"></param>

        public TestCaseCreationResult CreateTestCase(string authorLogin, int testsuiteid, string testcasename, int testprojectid,
                string summary, string steps, string expectedresults, string keywords,
                int order, bool checkduplicatedname, ActionOnDuplicatedName actiononduplicatedname,
                int executiontype, int importance)
        {

            string actionFlag= "block";
            switch (actiononduplicatedname)
            {
                case ActionOnDuplicatedName.Block: actionFlag = "block"; break;
                case ActionOnDuplicatedName.CreateNewVersion: actionFlag = "create_new_version"; break;
                case ActionOnDuplicatedName.GenerateNew: actionFlag = "generate_new"; break;
            }



            object response = proxy.createTestCase(
                devkey, authorLogin, testsuiteid, testcasename, testprojectid,
                summary, steps, expectedresults, keywords,
                order, checkduplicatedname ? 1 : 0, actionFlag,
                executiontype, importance);
            
            handleErrorMessage(response);

            if (response is object[])
            {
                object[] list = response as object[];
                foreach (XmlRpcStruct data in list)
                    return new TestCaseCreationResult(data);
            }
            return null;
        }

        /// <summary>
        /// get the newest build for a test plan
        /// </summary>
        /// <param name="tplanid"></param>
        /// <returns></returns>
        public Build GetLatestBuildForTestPlan(int tplanid)
        {
            object response = proxy.getLatestBuildForTestPlan(devkey,  tplanid);
            object[] objectList = response as object[];
            XmlRpcStruct data = response as XmlRpcStruct;

            List<TLErrorMessage> msgs = decodeErrors(objectList);
            if (msgs.Count > 0)
            {
                if  (msgs[0].code == 3031) //no builds               
                    return null; // no build      
                    else throw new TestLinkException(msgs);
            }
            if (data != null)
                return new Build(data);
            
            return null;
        }
        /// <summary>
        /// get the last execution result
        /// </summary>
        /// <param name="testplanid">id of the test plan</param>
        /// <param name="testcaseid">id of test case</param>
        /// <returns>a list of results, should contain 1 or no entries</returns>
        public List<ExecutionResult> GetLastExecutionResult(int testplanid, int testcaseid)
        {
            object[] response = proxy.getLastExecutionResult(devkey, testplanid, testcaseid);

            List<ExecutionResult> result = new List<ExecutionResult>();
            if ((response.Length == 0) || (response[0] is int))// that signifies no execution results
                    return result;

            handleErrorMessage(response);
            if (response != null)
            {
                // check if it is a no result indicator

                foreach (XmlRpcStruct data in response)
                {
                    result.Add(new ExecutionResult(data));
                }
            }
            return result;
        }

        /// <summary>
        /// get a list of all projects
        /// </summary>
        /// <returns></returns>
        public List<TestProject> GetProjects()
        {
             object response = null;

             try
             {
                 response = proxy.getProjects(devkey);
             }
             catch (XmlRpcServerException xrsex)
             {
                 throw new TestLinkException(xrsex.Message);
             }

            List<TestProject> retval = new List<TestProject>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return retval;
            object[] list = response as object[];
            handleErrorMessage(list);
            foreach (XmlRpcStruct entry in list)
            {
                TestProject tp = new TestProject(entry);
                retval.Add(tp);
            }
            return retval;
        }
        /// <summary>
        /// get a list of all builds for a testplan
        /// </summary>
        /// <param name="testplanid">the id of the testplan</param>
        /// <returns>a list (may be empty)</returns>
        public List<Build> GetBuildsForTestPlan(int testplanid)
        {
          
            object response = proxy.getBuildsForTestPlan(devkey, testplanid);
            List<Build> retval = new List<Build>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return retval; 
            object[] oList = response as object[];
                foreach (XmlRpcStruct data in oList)
                {
                    Build b = new Build(data);
                    retval.Add(b);
                }
            return retval;
        }

        /// <summary>
        /// get a list of all testplans for a project
        /// </summary>
        /// <param name="projectid"></param>
        /// <returns></returns>
        public List<TestPlan> GetProjectTestPlans(int projectid)
        {
            object response = proxy.getProjectTestPlans(devkey, projectid);
            List<TestPlan> retval = new List<TestPlan>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return retval;
            XmlRpcStruct[] results = response as XmlRpcStruct[];
            handleErrorMessage(results);
            object[] oList = response as object[];
            if ((oList.Length == 0) || (oList[0] is string))
                return retval;
            XmlRpcStruct result = oList[0] as XmlRpcStruct;
            //foreach (object o in oList)
            //{
            // //   System.Console.WriteLine("'{0}'", o.ToString());
            //    if ((o is string) && (((string)o) == string.Empty))
            //        return retval; // empty list

            //}
            //foreach (XmlRpcStruct data in oList)
            //{
            //    TestPlan tp = new TestPlan(data);
            //    retval.Add(tp);
            //}
            ////XmlRpcStruct result = results[0];
            foreach (string key in result.Keys)
            {
                XmlRpcStruct planStruct = (XmlRpcStruct)result[key];
                TestPlan tp = new TestPlan(planStruct);
                retval.Add(tp);
            }
            return retval;
        }
        /// <summary>
        /// get test suites for a test plan
        /// </summary>
        /// <param name="deep">false: get only top level, true: get all sub suties as well</param>
        /// <param name="testplanid"></param>
        /// <returns></returns>
        public List<TestSuite> GetTestSuitesForTestPlan(int testplanid)
        {
            List<TestSuite> result = new List<TestSuite>();
            // empty string means none, otherwise it is name, id combo
            object o = proxy.getTestSuitesForTestPlan(devkey, testplanid);

            handleErrorMessage(result);

            if (o is string)
                return result;
            object [] oList = o as object[];

            if (oList != null)
            {
                foreach (XmlRpcStruct data in oList)
                    result.Add(new TestSuite(data));
            }
            return result;
        }


        /// <summary>
        /// get all top level test suites for a test project
        /// </summary>
        /// <param name="testprojectid"></param>
        /// <returns></returns>
        public List<TestSuite> GetFirstLevelTestSuitesForTestProject(int testprojectid)
        {
            object[] response = proxy.getFirstLevelTestSuitesForTestProject(devkey, testprojectid);
            List<TLErrorMessage> errors = decodeErrors(response);
            List<TestSuite> result = new List<TestSuite>();
            if (errors.Count > 0)
            {
                if (errors[0].code != 7008) // project has no test suites, we return an emptu result
                    handleErrorMessage(response);
            }
            else foreach (XmlRpcStruct data in response)
            {
                result.Add(new TestSuite(data));
            }
            return result;
        }
        /// <summary>
        /// Report a result for a single test case 
        /// </summary>
        /// <param name="tcid">Test Case ID</param>
        /// <param name="tpid">Test Plan Id</param>
        /// <param name="status">Status</param>
        /// <remarks> 
        /// Status is one of p, f or b (for pass, fail, blocked) </remarks>
        public GeneralResult ReportTCResult(int tcid, int tpid, TestCaseResultStatus status)
        {
            string statusChar="";
            switch (status)
            {
                case TestCaseResultStatus.Blocked: statusChar = "b"; break;
                case TestCaseResultStatus.Pass: statusChar = "p"; break;
                case TestCaseResultStatus.Fail: statusChar = "f"; break;
            }
            object response = proxy.reportTCResult(devkey, tcid, tpid, statusChar, true);
            handleErrorMessage(response);
            return handleReportTCResult(response);
        }
        private GeneralResult handleReportTCResult(object response)
        {
            if (response is object[])
            {
                object[] responseList = response as object[];
                if (responseList.Length > 0)
                {
                    XmlRpcStruct msg = (XmlRpcStruct)responseList[0];
                    GeneralResult result = new GeneralResult(msg);
                    return result;
                }
            }
            GeneralResult noResult = new GeneralResult();

            return noResult;
        }

        public GeneralResult ReportTCResult(int tcid, int tpid, TestCaseResultStatus status, string notes)
        {
            string statusChar="";
            switch (status)
            {
                case TestCaseResultStatus.Blocked: statusChar = "b"; break;
                case TestCaseResultStatus.Pass: statusChar = "p"; break;
                case TestCaseResultStatus.Fail: statusChar = "f"; break;
            }
            object response = proxy.reportTCResult(devkey, tcid, tpid, statusChar, notes, true);
            handleErrorMessage(response);
            return handleReportTCResult(response);
        }

        public GeneralResult ReportTCResult(int tcid, int tpid, int buildId, TestCaseResultStatus status, string notes)
        {
            string statusChar = "";
            switch (status)
            {
                case TestCaseResultStatus.Blocked: statusChar = "b"; break;
                case TestCaseResultStatus.Pass: statusChar = "p"; break;
                case TestCaseResultStatus.Fail: statusChar = "f"; break;
            }
            object response = proxy.reportTCResult(devkey, tcid, tpid, statusChar, buildId, notes, true);
            handleErrorMessage(response);
            return handleReportTCResult(response);
        }

        #region getTestCasesForTestPlan

        public List<TestCaseFromTestPlan> GetTestCasesForTestPlan(int testplanid, int testcaseid, int buildid, int keywordid, int executed, int assignedTo, string executedstatus)
        {
            object response = proxy.getTestCasesForTestPlan(devkey, testplanid, testcaseid, buildid, keywordid, executed, assignedTo, executedstatus);
            List<TestCaseFromTestPlan> result = new List<TestCaseFromTestPlan>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return result;
            handleErrorMessage(response);
            XmlRpcStruct list = response as XmlRpcStruct;
            if (list != null)
            {
                foreach (XmlRpcStruct data in list.Values)
                {
                    TestCaseFromTestPlan tc = new TestCaseFromTestPlan(data);
                    result.Add(tc);
                }
            }
            return result;
        }
        public List<TestCaseFromTestPlan> GetTestCasesForTestPlan(int testplanid, int testcaseid, int buildid, int keywordid, int executed, int assignedTo)
        {
            object response = proxy.getTestCasesForTestPlan(devkey, testplanid, testcaseid, buildid, executed, assignedTo);
            List<TestCaseFromTestPlan> result = new List<TestCaseFromTestPlan>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return result;
            handleErrorMessage(response);
            XmlRpcStruct list = response as XmlRpcStruct;
            if (list != null)
            {
                foreach (XmlRpcStruct data in list.Values)
                {
                    TestCaseFromTestPlan tc = new TestCaseFromTestPlan(data);
                    result.Add(tc);
                }
            }
            return result;
        }
        public List<TestCaseFromTestPlan> GetTestCasesForTestPlan(int testplanid, int testcaseid, int buildid, int keywordid, int executed)
        {
            object response = proxy.getTestCasesForTestPlan(devkey, testplanid, testcaseid, buildid, executed);
            List<TestCaseFromTestPlan> result = new List<TestCaseFromTestPlan>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return result;
            handleErrorMessage(response);
            XmlRpcStruct list = response as XmlRpcStruct;
            if (list != null)
            {
                foreach (XmlRpcStruct data in list.Values)
                {
                    TestCaseFromTestPlan tc = new TestCaseFromTestPlan(data);
                    result.Add(tc);
                }
            }
            return result;
        }
        public List<TestCaseFromTestPlan> GetTestCasesForTestPlan(int testplanid, int testcaseid, int buildid, int keywordid)
        {
            object response = proxy.getTestCasesForTestPlan(devkey, testplanid, testcaseid, buildid, keywordid);
            List<TestCaseFromTestPlan> result = new List<TestCaseFromTestPlan>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return result;
            handleErrorMessage(response);
            XmlRpcStruct list = response as XmlRpcStruct;
            if (list != null)
            {
                foreach (XmlRpcStruct data in list.Values)
                {
                    TestCaseFromTestPlan tc = new TestCaseFromTestPlan(data);
                    result.Add(tc);
                }
            }
            return result;
        }

        public List<TestCaseFromTestPlan> GetTestCasesForTestPlan(int testplanid, int testcaseid, int buildid)
        {
            object response = proxy.getTestCasesForTestPlan(devkey, testplanid, testcaseid, buildid);
            List<TestCaseFromTestPlan> result = new List<TestCaseFromTestPlan>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return result;
            handleErrorMessage(response);
            XmlRpcStruct list = response as XmlRpcStruct;
            if (list != null)
            {
                foreach (XmlRpcStruct data in list.Values)
                {
                    TestCaseFromTestPlan tc = new TestCaseFromTestPlan(data);
                    result.Add(tc);
                }
            }
            return result;
        }

        public List<TestCaseFromTestPlan> GetTestCasesForTestPlan(int testplanid, int testcaseid)
        {
            object response = proxy.getTestCasesForTestPlan(devkey, testplanid, testcaseid);
            List<TestCaseFromTestPlan> result = new List<TestCaseFromTestPlan>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return result;
            handleErrorMessage(response);
            XmlRpcStruct list = response as XmlRpcStruct;
            if (list != null)
            {
                foreach (XmlRpcStruct data in list.Values)
                {
                    TestCaseFromTestPlan tc = new TestCaseFromTestPlan(data);
                    result.Add(tc);
                }
            }
            return result;
        }

        public List<TestCaseFromTestPlan> GetTestCasesForTestPlan(int testplanid)
        {
            object response = proxy.getTestCasesForTestPlan(devkey, testplanid);
            List<TestCaseFromTestPlan> result = new List<TestCaseFromTestPlan>();
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return result;
            handleErrorMessage(response);
            XmlRpcStruct list = response as XmlRpcStruct;
            if (list != null)
            {
                foreach (XmlRpcStruct data in list.Values)
                {
                    TestCaseFromTestPlan tc = new TestCaseFromTestPlan(data);
                    result.Add(tc);
                }
            }
            return result;
        }
        #endregion

        /// <summary>
        /// get test cases for a test suite
        /// </summary>
        /// <param name="testSuiteid"></param>
        /// <param name="deep">go to sub suites as well</param>
        /// <param name="details">
        ///   'only_id'
        ///          Array that contains ONLY testcase id, no other info.         
        ///   'simple'
        ///         Array where each element is a map with following keys.
        ///            
        ///            id: testcase id
        ///            parent_id: testcase parent (a test suite id).
        ///            node_type_id: type id, for a testcase node
        ///            node_order
        ///            node_table: node table, for a testcase.
        ///            name: testcase name
        ///            
        ///   'full'
        ///            Complete info about testcase for LAST TCVERSION         
        /// </param>
        /// <returns></returns>
        public List<TestCase> GetTestCasesForTestSuite(int testSuiteid, bool deep)
        {

            //object o = proxy.getTestCasesForTestSuite(devkey, testSuiteid);
            List<TestCase> result = new List<TestCase>();
            object response = proxy.getTestCasesForTestSuite(devkey, testSuiteid, deep, "full");
            if ((response is string) && ((string)response == string.Empty))  // equals null return
                return result;
            handleErrorMessage(response);
            object[] list = response as object[];
            if (list != null)
            {
                foreach (XmlRpcStruct data in list)
                {
                    TestCase tc = new TestCase(data);
                    result.Add(tc);
                }
            }
            return result;
        }
        public List<int> getTestCaseIdsForTestSuite(int testSuiteid, bool deep)
        {
            object o = proxy.getTestCasesForTestSuite(devkey, testSuiteid, deep, "id_only");
            List<int> result = new List<int>();
            
            handleErrorMessage(o);

            if (o is object[])
            {
                object[]  list = o as object[];
                foreach (int i in list)
                    result.Add(i);
            }
            return result;
        }
        /// <summary>
        /// ask for a test case by name. 
        /// </summary>
        /// <param name="testcasename"></param>
        /// <param name="testsuitename"></param>
        /// <returns>a list of items found</returns>
        public List<TestCaseId> GetTestCaseIDByName(string testcasename, string testsuitename)
        {
            object response = proxy.getTestCaseIDByName(devkey, testcasename, testsuitename);
            List<TLErrorMessage> errs = decodeErrors(response);
            if ((errs.Count > 0) && (errs[0].code == 5030)) // 5030 is no id found
                return new List<TestCaseId>();

            handleErrorMessage(response);
            return processTestCaseId(response);           
        }
        /// <summary>
        /// get a test cases by this name
        /// </summary>
        /// <param name="testcasename"></param>
        /// <param name="testSuiteId"></param>
        /// <returns>a list containing zero to many test cases with that name that occur in the specific test suite</returns>
        public List<TestCaseId> GetTestCaseIDByName(string testcasename, int testSuiteId)
        {
            List<TestCaseId> idList = GetTestCaseIDByName(testcasename);
            if (idList.Count == 0)
                return idList;
            List<TestCaseId> result = new List<TestCaseId>();
            foreach (TestCaseId tc in idList)
                if (tc.parent_id == testSuiteId)
                    result.Add(tc);
            return result;
        }
        /// <summary>
        /// get a list of test cases with this name. 
        /// </summary>
        /// <param name="testcasename"></param>
        /// <returns>a list containing zero to many test cases with that name</returns>
        public List<TestCaseId> GetTestCaseIDByName(string testcasename)
        {
            object response = proxy.getTestCaseIDByName(devkey, testcasename);
            List<TLErrorMessage> errs = decodeErrors(response);
            if ((errs.Count > 0) && (errs[0].code == 5030)) // 5030 is no id found
                return new List<TestCaseId>();

            handleErrorMessage(response);
            return processTestCaseId(response);           
        }

        private List<TestCaseId> processTestCaseId(object response)
        {
            List<TestCaseId> result = new List<TestCaseId>();
            if (response is object[])
            {
                object[] responseList = response as object[];
                foreach (XmlRpcStruct item in responseList)
                {
                    TestCaseId id = new TestCaseId(item);
                    result.Add(id);
                }
            }
            return result;
        }

        public  string SayHello()
        {        
            string hello = proxy.sayHello();
            return hello;
        }
    }
}
