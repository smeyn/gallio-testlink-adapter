# Introduction #

To make use of the GTA, you must first add a new attribute to each TestFixture. This attribute, called TestLinkFixture takes parameters that tell the adapter where to find the test cases to record results to.
Typically the tests are run as part of a build script or a batch file. The GTA is an extension to the command line runner.
When the tests have been run, the extension is called with the test results.
The extension parses the results, loads the test assemblies that have been run and extracts the test link fixture. It then contacts Testlink to look for test cases with the same name as they test methods in the unit test.
If the test case does not exist, GTA will create them and assign them to the Testplan.
It then records the test result against that test case.
# Installation #
## Run Time Files ##
There are three DLLs that need to be copied into the bin directory of the Gallio folder:
  * CookComputing.XmlRpcV2.dll
  * TestLinkAPI.dll
  * TlGallioAddOn.dll
## Invocation ##
The exporter works with the console runner Gallio.Echo.
Gallio v3.0.5
```
Gallio.Echo.exe /extension:TestLinkAddOn,TlGallioAddon.dll  [unitTestAssembly]
```

Gallio v3.0.6 to v 3.3
```
Gallio.Echo.exe /re:TestLinkAddOn,TlGallioAddon.dll   [unitTestAssembly]
```


If you are using the Gallio NAnt or MSBuild tasks, the process is similar. Refer to the documentation of these tasks for details.  (http://blog.bits-in-motion.com/2008/05/announcing-gallio-v30-alpha-3.html)
## TestLink Setup ##
In order for this to work you need to have in testlink:
  1. Enabled the API (see installation manual)
  1. Created an account  for testing
  1. Created an ApiKey for this account
  1. Setup a test project
  1. Set up test suite(s) (top level test suite) for the test cases
  1. In the test project set up a test plan
  1. Optionally created test cases in the test suite and assigned to the test plan
  1. Have an active build defined in the testplan

Step 7 really is optionally because:
  1. You must make sure you name the test cases exactly as they are defined in your unit tests
  1. If you use advanced features such as contract verifiers and row tests in MBUnit you have to guess what these test cases will be named because they are dynamically generated
  1. If you don’t create test cases, GTA will do it for you – much easier

## Unit Test setup ##
Write your test cases as you are used to.
Then have the project reference TestLinkFixture.dll and add a Using Statement for it.
Add the TestLinkFixture to your testfixture.
The parameters are:
  * URL of the TestLink Api
  * UserName under which the test cases are authored
  * An ApiKey (provided by the testlink application (uses the name DevKey)
  * The test project name
  * The test plan in the test project
  * The test suite name that will contain the test cases

Example
```
using MbUnit.Framework;
using TlGallioAddOn;
using Meyn.TestLink;

namespace gallioAddOnSampleTests
{
    [TestFixture]
    [TestLinkFixture(
        Url = "http://localhost/testlink/lib/api/xmlrpc.php",
        ProjectName = "TestLinkApi",
        UserId = "admin",
        TestPlan = "Automatic Testing",
        TestSuite = "gallioAddOnSampleTests",
        PlatformName = "Windows Server 2008"
        DevKey = "ae28ffa45712a041fa0b31dfacb75e29")]
    public class SampleFixture
    {
        [Test]
        public void Test2Succeed()
        {
            Assert.IsTrue(true);
        }
    }
}
```


Voila.
This will create a test case named Test2Succeed and record test case result against it.

## Note ##
Please make sure the fixture class is marked public. Gallio will run and execute classes not marked public but this adapter won't find the fixture and hence won't export.

Also look at the new use of the ConfigFile at UsingTheConfigurationFile

## Limitations ##
This has been tested with Gallio & MBUnit V3.0.5 & V3.0.6 & V3.3, C# and Testlink V 1.9.3

It should work with VB.NET.

It does work with NUnit.

You use this code at your own risk.