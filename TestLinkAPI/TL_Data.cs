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


using System;
using CookComputing.XmlRpc;

namespace Meyn.TestLink
{
    /// <summary>
    /// base class for all response classes from the TL Api
    /// </summary>
    public abstract class TL_Data
    {
        /// <summary>
        /// robust convert to int. can take int strings as well as ints 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public int toInt(XmlRpcStruct data, string name)
        {
            if (data.ContainsKey(name))
            {
                int n;
                object val = data[name];
                if (val is string)
                {
                    bool good = int.TryParse((string)val, out n);
                    if (good)
                        return n;
                }
                else if (val is int)
                    return (int)(val);
            }
            return 0;
        }
        /// <summary>
        /// robust convert a data bit to a bool 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool? toBool(XmlRpcStruct data, string name)
        {
            if (data.ContainsKey(name))
            {
                object val = data[name];
                if (val is string)
                {
                    bool result;
                    bool good = bool.TryParse(val as string, out result);
                    return result;
                }
                return data[name] as bool?;
            }
            return null;
        }
        public DateTime toDate(XmlRpcStruct data, string name)
        {
            if (data.ContainsKey(name))
            {
                DateTime n;
                bool good = DateTime.TryParse((string)data[name], out n);
                if (good)
                    return n;
            }
            return DateTime.MinValue;
        }

        public char toChar(XmlRpcStruct data, string name)
        {
            if (data.ContainsKey(name) && (data[name] is string))
            {
                string s = (string)(data[name]);
                return s[0];
            }
            return '\x00';
        }
    }


    public class TLErrorMessage : TL_Data
    {
        public int code;
        public string message;
        public TLErrorMessage(XmlRpcStruct data)
        {
            code = toInt(data,"code");
            message = (string)data["message"];
        }
    }
    public class Build : TL_Data
    {
        public int id;
        public bool active;
        public string name;
        public string notes;
        public int testplan_id;
        public bool is_open;
        public Build(XmlRpcStruct data)
        {
            id = toInt(data, "id");
            active = toInt(data, "active") == 1;
            name = (string)data["name"];
            notes = (string)data["notes"];
            testplan_id = toInt(data, "testplan_id");
            is_open = toInt(data, "is_open") == 1;
        }
    }


    public class GeneralResult : TL_Data
    {
        public string message;
        public bool? status;
        public int id;
        public GeneralResult(XmlRpcStruct data)
        {
            message = (string)data["message"];
            id = toInt(data, "id");
            status = toBool(data, "status");
        }
        public GeneralResult()
        {
            status = false;
            message = "no response from server";
        }
    }

    /// <summary>
    /// test cases as they are returned from a test plan
    /// </summary>
    /// <remarks>This is different from TestCase as it returns additional info from the testplan. 
    /// Maybe this should be refactored with a testplandetails subclass</remarks>
    public class TestCaseFromTestPlan : TL_Data
    {
        public string z;
        public string type;
        public int execution_order;
        public int exec_id;
        public int tc_id;
        public int tcversion_number;
        public string status;
        public string external_id;
        public string name;
        public string exec_status;
        public int exec_on_tplan;
        public int executed;//5
        public int feature_id;
        public int assigner_id;
        public int user_id;
        public bool active;
        public int version;
        public int testsuite_id;
        public int tcversion_id;
        public string expected_results;
        public string steps;
        public string summary;
        public int execution_type;

        public TestCaseFromTestPlan(XmlRpcStruct data)
        {
            active = int.Parse((string)data["active"]) == 1;
            name = (string)data["name"];
            z = (string)data["z"];
            type = (string)data["type"];
            execution_order = toInt(data, "execution_order");
            exec_id = toInt(data, "exec_id");
            tc_id = toInt(data, "tc_id");
            tcversion_number = toInt(data, "tcversion_number");
            status = (string)data["status"];
            external_id = (string)data["external_id"];
            exec_status = (string)data["exec_status"];
            exec_on_tplan = toInt(data, "exec_on_tplan");
            executed = toInt(data, "executed");
            feature_id = toInt(data, "feature_id");
            assigner_id = toInt(data, "assigner_id");
            user_id = toInt(data, "user_id");
            active = toInt(data, "active") == 1;
            version = toInt(data, "version");
            testsuite_id = toInt(data, "testsuite_id");
            tcversion_id = toInt(data, "tcversion_id");
            steps = (string)data["steps"];
            expected_results = (string)data["expected_results"];
            summary = (string)data["summary"];
            execution_type = toInt(data, "execution_type");
        }     
    }

    /// <summary>
    /// test case as it is retrieved from testsuite
    /// </summary>
    public class TestCase : TL_Data
    {
        public int id;
        public string name;
        public bool active; //+		["active"]	"1"	
        /// <summary>
        /// the version of the test case, starts with 1
        /// </summary>
        public int version;		//["version"]	"1"	
        /// <summary>
        /// the internal id of this testcase version
        /// </summary>
        public int tcversion_id;//+		["tcversion_id"]	"1303"	
        public string expected_results;//"]	""	
        public string steps; //"]	""	
        public string summary;//"]	"<b>The Test Case was generated from the assigned requirement.</b><br />"	
        /// <summary>
        /// the id that is displayed on the UI, sans the prefix
        /// </summary>
        public string external_id; //+		["tc_external_id"]	"1"	
        /// <summary>
        /// the id of the owning testsuite
        /// </summary>
        public int testSuite_id;//"]	"1301"	
        /// <summary>
        /// unknown purpose
        /// </summary>
        public bool is_open;//"]	"1"	
        public DateTime modification_ts;//"]	"0000-00-00 00:00:00"	
        public int updater_id;//"]	
        /// <summary>
        /// manual or automatic
        /// </summary>
        public int execution_type;//"]	"1"	
        //+		["node_type_id"]	"3"	
        public string details;  // not sure wher this comes from"]	"Test Cases in the Test Suite are generated from Requirements. A refinement of test scenario is highly recommended."	
        public int author_id;//"]	"1"	
        public DateTime creation_ts;//"]	"2009-03-05 20:47:00"	
        public int importance;//"]	"2"	
        //+		["node_order"]	"0"	
        //+		["node_table"]	"testcases"	
        public TestCase(XmlRpcStruct data)
        {
            active = int.Parse((string)data["active"]) == 1;
            id = toInt(data, "id");
            name = (string)data["name"];
            version = toInt(data, "version");
            tcversion_id = toInt(data, "tcversion_id");
            steps = (string)data["steps"];
            expected_results = (string)data["expected_results"];
            external_id = (string)data["external_id"];
            testSuite_id = toInt(data, "parent_id");
            is_open = int.Parse((string)data["is_open"]) == 1;
            modification_ts = base.toDate(data, "modification_ts");
            updater_id = toInt(data, "updater_id");
            execution_type = toInt(data, "execution_type");
            summary = (string)data["summary"];
            details = (string)data["details"];
            author_id = toInt(data, "author_id");
            creation_ts = base.toDate(data, "creation_ts");
            importance = toInt(data, "importance");
        }
    }


    public class TestCaseCreationResult : TL_Data
    {
        public string operation;
   		public bool status;//" => true, 
		public int id;
        public AdditionalInfo additionalInfo;//" => $op_result,
		public string message;//" => GENERAL_SUCCESS_STR);
        public TestCaseCreationResult(XmlRpcStruct data)
        {
            operation = (string)data["operation"];
            status = (bool)(data["status"]);
            id = toInt(data, "id");
            message = (string)data["message"];
            if (data["additionalInfo"] is XmlRpcStruct)
                additionalInfo = new AdditionalInfo(data["additionalInfo"] as XmlRpcStruct);
        }

    }
    public class AdditionalInfo : TL_Data
    {
        public string new_name; //	"externally created test case"	
		public bool status_ok; //	1	
		public string msg; //	"Created new version 2"	
		public int external_id; //	"5"	
		public int id; //	1313	
 		public int version_number; //	-1	
 		public bool? has_duplicate; //	true
        public AdditionalInfo(XmlRpcStruct data)
        {
            new_name = (string)data["new_name"];
            status_ok = toInt(data, "status_ok") == 1;
            msg = (string)data["msg"];
            id = toInt(data, "id");
            external_id = toInt(data, "external_id");
            version_number = toInt(data, "version_number");
            has_duplicate = toBool(data, "has_duplicate");
        }
    }

    public class TestSuite : TL_Data
    {
        public int id;
        public string name;
        public int parentId;

        public TestSuite(XmlRpcStruct data)
        {
            name = (string)data["name"];
            id = toInt(data, "id");
            parentId = toInt(data, "parent_id");
        }
    }
    public class ExecutionResult : TL_Data
    {
        public int id;
        public DateTime execution_ts;   //"]	"2009-03-05 15:31:59"	
        public int execution_type;      //"]	"2"	
        public int build_id;            //"]	"1"	
        public int testplan_id;         //"]	"18"	
        public int tcversion_id;        //"]	"5"	
        public string notes;            //"]	"tried record a fail result"	
        public int tester_id;           //"]	"1"	
        public int tcversion_number;    //"]	"1"	
        public char status;             //"]	"f"	
        public ExecutionResult(XmlRpcStruct data)
        {
            id = toInt(data, "id");
            notes = (string)data["notes"];
            execution_ts = toDate(data, "execution_ts");
            execution_type = toInt(data, "execution_type");
            build_id = toInt(data, "build_id");
            tcversion_id = toInt(data, "tcversion_id");
            tcversion_number = toInt(data, "tcversion_number");
            status = toChar(data, "status");
        }

 
    }

    public class TestProject : TL_Data
    {
        public int id;
        public string notes;
        public string color;
        public bool active;
        public bool option_reqs;
        public bool option_priority;
        public string prefix;
        public int tc_counter;
        public bool option_automation;
        public string name;
        public TestProject(XmlRpcStruct data)
        {
            id = toInt(data, "id");
            notes = (string)data["notes"];
            color = (string)data["color"];
            active = toInt(data, "active") == 1;
            option_reqs = toInt(data, "option_reqs") == 1;
            option_priority = toInt(data, "option_priority") == 1;
            prefix = (string)data["prefix"];
            tc_counter = toInt(data, "tc_counter");
            option_automation = toInt(data, "option_automation") == 1;
            name = (string)data["name"];
        }
    }

    public class TestPlan : TL_Data
    {
        public bool active;
        public string name;
        public int testproject_id;
        public string notes;
        public int id;
        internal TestPlan(XmlRpcStruct data)
        {
            //Console.WriteLine("Testplan constructor");
            //Console.WriteLine("Nr data items: {0}", data.Values.Count);
            //Console.WriteLine("Nr key items: {0}", data.Keys.Count);
            //foreach (object key in data.Keys)
            //    Console.WriteLine("{0}:'{1}'", key, data[key].ToString());
            //Console.WriteLine("---");
            active = toInt(data, "active") == 1;
            id = toInt(data, "id");
            name = (string)data["name"];
            notes = (string)data["notes"];
            testproject_id = toInt(data, "testproject_id");
        }
    }

    public class TestCaseId : TL_Data
    {
        public int tc_external_id;
        public string name;
        public int parent_id;
        public int id;
        public string tsuite_name;
        public TestCaseId(XmlRpcStruct data)
        {
            parent_id = toInt(data, "parent_id");
            tc_external_id = toInt(data, "tc_external_id");
            id = toInt(data, "id");
            name = (string)data["name"];
            tsuite_name = (string)data["tsuite_name"];
        }
    }
}
