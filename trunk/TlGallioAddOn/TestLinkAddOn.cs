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
using Gallio.Runner.Extensions;
using Gallio.Runner.Events;
using System.Diagnostics;
using Gallio.Runner.Reports;

namespace TlGallioAddOn
{
    /// <summary>
    /// the extension to the framework.
    /// read the associated document "The Gallio Testlink Adapter.pdf" for details 
    /// </summary>
    public class TestLinkAddOn:TestRunnerExtension
    {
        private ResultExporter exporter;

        protected override void Initialize()
        {
            exporter = new ResultExporter();
            Events.RunFinished += new EventHandler<RunFinishedEventArgs>(runFinished);
            Events.TestStepFinished += new EventHandler<TestStepFinishedEventArgs>(Events_TestStepFinished);
        }

        void Events_TestStepFinished(object sender, TestStepFinishedEventArgs e)
        {
            Debug.WriteLine("TestStep Finished");
            Debug.WriteLine(e.Test.FullName);
            Debug.WriteLine(string.Format("  IsTestCase = {0}", e.Test.IsTestCase));
            Debug.WriteLine(string.Format("  Test Result for {0}: {1}", e.TestStepRun.Step.Name, e.TestStepRun.Result.Outcome));
            Debug.WriteLine(string.Format("  CodeLocation {0}", e.TestStepRun.Step.CodeLocation.Path));
            Debug.WriteLine(string.Format("  CodeReference {0}", e.TestStepRun.Step.CodeReference.Kind));
            foreach (string key in e.TestStepRun.Step.Metadata.Keys)
            {
                Debug.WriteLine(string.Format("  {0}= {1}", key, e.TestStepRun.Step.Metadata[key][0]));
            }

            if (e.TestStepRun.Step.CodeReference.Kind == Gallio.Reflection.CodeReferenceKind.Assembly)
            {
               exporter.RetrieveTestFixture(e.TestStepRun.Step.CodeLocation.Path);
            }
            Debug.WriteLine("---------------------------------------");

        }



        public void runFinished(object sender, RunFinishedEventArgs args)
        {
            Debug.WriteLine("Run Finished");
            Console.WriteLine("Exporting results to Testlink");
            foreach (TestStepRun run in args.Report.TestPackageRun.AllTestStepRuns)
                if (run.Step.IsTestCase)
                    exporter.ReportResult(run);

            Console.WriteLine("Finished exporting results to Testlink");
            Console.WriteLine();
        }


    }
}
