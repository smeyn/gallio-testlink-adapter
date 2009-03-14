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
using System.Linq;
using System.Text;

namespace TlGallioAddOn
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestLinkFixtureAttribute : System.Attribute
    {
        private string url;

        /// <summary>
        /// the url for the Testlink XmlRPC api.
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        private string projectName;

        /// <summary>
        /// the name of the test project in testlink
        /// </summary>
        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; }
        }
        private string userId;

        /// <summary>
        /// the user name to be used for creating new testcases (must conicide with the DevKey)
        /// </summary>
        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        //private string password;

        //public string Password
        //{
        //    get { return password; }
        //    set { password = value; }
        //}
        private string devKey;

        /// <summary>
        /// the devkey or ApiKey for the above userid. provided by testlink
        /// </summary>
        public string DevKey
        {
            get { return devKey; }
            set { devKey = value; }
        }

        private string testPlan;
        /// <summary>
        /// the name of the test plan containing the test case results
        /// </summary>
        public string TestPlan
        {
            get { return testPlan; }
            set { testPlan = value; }
        }

        private string testSuite;

        /// <summary>
        /// the name of the top level test suite where the test cases are expected.
        /// </summary>
        public string TestSuite
        {
            get { return testSuite; }
            set { testSuite = value; }
        }
        public TestLinkFixtureAttribute()
        {
        }
    }
}
