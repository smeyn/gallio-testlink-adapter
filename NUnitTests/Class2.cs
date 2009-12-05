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
    public class Class2Tests
    {
        [Test, Description("this one has to fail")]
        public void FailThis2()
        {
            Assert.Fail("Failed because it had to");
        }
        [Test]
        public void Succeed2()
        {
        }
        [Test, Combinatorial]
        public void MyTest(
            [Values(1,2,3)] int x,
            [Values("A","B")] string s)
        {
            if (s == "A")
                Assert.AreEqual(2, x, "if A then x needs to be 2");
            else
                Assert.AreNotEqual(2, x, "If B then x should not be 2");
        }
    }
}
