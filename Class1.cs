using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Meyn.TestLink;

namespace nunitTests
{
    [TestFixture]
    [TestLinkFixture(
    Url = "http://localhost/testlink/lib/api/xmlrpc.php",
    ProjectName = "FMFS",
    UserId = "admin",
    TestPlan = "Automatic Testing",
    TestSuite = "nunitAddOnSampleTests",
    DevKey = "b6e8fee35d143cd018d3b683e0777c51")]
    public class Class1Tests
    {
        [Test]
        public void FailThis()
        {
            Assert.Fail("Failed because it had to");
        }
        [Test]
        public void Succeed()
        {
        }
    }
}
