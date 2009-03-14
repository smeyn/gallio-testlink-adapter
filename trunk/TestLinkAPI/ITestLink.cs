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

using CookComputing.XmlRpc;


namespace TestLinkAPI
{
 
    /// <summary>
    /// the interface mapping required for the XmlRpc api of testlink. 
    /// This interface is used by the TestLink class. 
    /// </summary>
    /// <remarks>This class makes use of XML-RPC.NET Copyright (c) 2006 Charles Cook</remarks>
    [XmlRpcUrl("")]
    public interface ITestLink : IXmlRpcProxy
    {
        [XmlRpcMethod("tl.addTestCaseToTestPlan", StructParams = true)]
        object  addTestCaseToTestPlan(string devKey, int testprojectid, int testplanid, string testcaseexternalid, int  version);


        //[XmlRpcMethod("tl.assignRequirements")]
        //string assignRequirements();


        [XmlRpcMethod("tl.createBuild", StructParams=true)]
        object[] createBuild(string devKey, int testplanid, string buildname, string buildnotes);


    
        [XmlRpcMethod("tl.createTestCase", StructParams=true)]
        object createTestCase(string devKey, string authorlogin, int testsuiteid, string testcasename,int testprojectid,  
            string summary, string steps, string expectedresults, string keywords,
            int order, int checkduplicatedname, string actiononduplicatedname, int executiontype, int importance);

        //TODO: Implement
        [XmlRpcMethod("tl.createTestProject")]
        void createTestProject();

        [XmlRpcMethod("tl.getBuildsForTestPlan", StructParams = true)]
        object getBuildsForTestPlan(string devKey, int testplanid);

        [XmlRpcMethod("tl.getLatestBuildForTestPlan", StructParams = true)]
        object getLatestBuildForTestPlan(string devKey, int testplanid);

        [XmlRpcMethod("tl.getLastExecutionResult", StructParams = true)]
        object[] getLastExecutionResult(string devKey, int testplanid, int testcaseid);
        

        [XmlRpcMethod("tl.getProjects", StructParams = true)]
        object getProjects(string devKey);
 
        [XmlRpcMethod("tl.getProjectTestPlans", StructParams = true)]
        XmlRpcStruct[] getProjectTestPlans(string devKey, int testprojectid);

        [XmlRpcMethod("tl.getTestCaseAttachments", StructParams = true)]
        string getTestCaseAttachments(string devkey);

        [XmlRpcMethod("tl.getTestCaseCustomFieldDesignValue", StructParams = true)]
        string getTestCaseCustomFieldDesignValue(string devkey);

        [XmlRpcMethod("tl.getTestCaseIDByName", StructParams = true)]
        object getTestCaseIDByName(string devKey, string testcasename, string testsuitename);
        [XmlRpcMethod("tl.getTestCaseIDByName", StructParams = true)]
        object getTestCaseIDByName(string devKey, string testcasename);


        [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
        object getTestCasesForTestPlan(string devKey, int testplanid);
        [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
        object getTestCasesForTestPlan(string devKey, int testplanid, int testcaseid);
        [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
        object getTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid);
        [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
        object getTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid, int keywordid);
        [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
        object getTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid, int keywordid, int executed);
        [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
        object getTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid, int keywordid, int executed, int assignedTo);
        [XmlRpcMethod("tl.getTestCasesForTestPlan", StructParams = true)]
        object getTestCasesForTestPlan(string devKey, int testplanid, int testcaseid, int buildid, int keywordid, int executed, int assignedTo, string executedstatus);

        [XmlRpcMethod("tl.getTestCasesForTestSuite", StructParams = true)]
        object getTestCasesForTestSuite(string devKey, int testsuiteid, bool deep, string details);
        [XmlRpcMethod("tl.getTestCasesForTestSuite", StructParams = true)]
        object getTestCasesForTestSuite(string devKey, int testsuiteid);

        [XmlRpcMethod("tl.getTestSuitesForTestPlan", StructParams = true)]
        object getTestSuitesForTestPlan(string devKey, int testplanid);

        [XmlRpcMethod("tl.getFirstLevelTestSuitesForTestProject", StructParams = true)]
        object[] getFirstLevelTestSuitesForTestProject(string devKey, int testprojectid);
        //[XmlRpcMethod("tl.repeat")]
        //string repeat();


        [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
        object reportTCResult(string devKey, int testcaseid, int testplanid, string status, int build, string notes, bool guess);
        [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
        object reportTCResult(string devKey, int testcaseid, int testplanid, string status, int build, string notes);
        [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
        object reportTCResult(string devKey, int testcaseid, int testplanid, string status, string notes, bool guess);
        [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
        object reportTCResult(string devKey, int testcaseid, int testplanid, string status, int build);
        [XmlRpcMethod("tl.reportTCResult", StructParams = true)]
        object reportTCResult(string devKey, int testcaseid, int testplanid, string status, bool guess);


        [XmlRpcMethod("tl.sayHello")]
        string sayHello();
    }
}
