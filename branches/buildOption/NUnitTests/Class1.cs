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
    ConfigFile = "tlinkconfig.xml",
    TestSuite = "nunitAddOnSampleTests")]
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
