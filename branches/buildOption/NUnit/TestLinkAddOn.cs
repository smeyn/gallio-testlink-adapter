using System;
using NUnit.Core.Extensibility;
using NUnit.Core;
using NUnit.Framework;

using log4net;
using log4net.Config;
using System.IO;


namespace Meyn.TestLink.NUnitExport
{
    /// <summary>
    /// this is the bas addIn class that the NUnit extension framework loads.
    /// It installs  an event listener to do all the work.
    /// </summary>
    [NUnitAddin(Name="TestLinkAddon", Description="Exports test run results to Testlink")]
    public class TestLinkAddOn:IAddin
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TestLinkAddOn));
        //static Logger log = InternalTrace.GetLogger(typeof(TestLinkAddOn));

        ResultExporter exporter;
        //TestCaseInspector tci = new TestCaseInspector();

        #region IAddin Members

        public bool Install(IExtensionHost host)
        {
            FileInfo configFile = new FileInfo(@".\NunitTestLinkExporterLog.xml");
            //XmlConfigurator.Configure(configFile); //(logConfigUri);
            log.Debug("Starting install");

            exporter = new ResultExporter();
            IExtensionPoint listeners = host.GetExtensionPoint("EventListeners");
            listeners.Install(exporter);
            
            //IExtensionPoint testCaseBuilderEP = host.GetExtensionPoint("TestCaseBuilders");
            //testCaseBuilderEP.Install(tci);

            return true;
        }

        #endregion
    }
}
