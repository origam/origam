using Origam.Workflow.NetMembershipService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Security;
using System.Collections.Specialized;

namespace NetMembershipTests
{
    
    
    /// <summary>
    ///This is a test class for NetMembershipServiceAgentTest and is intended
    ///to contain all NetMembershipServiceAgentTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NetMembershipServiceAgentTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext xxx)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for createEmailRequirementsXml
        ///</summary>
        [TestMethod()]
        public void createEmailRequirementsXmlTest()
        {            
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            //actual = NetMembershipServiceAgent.createPasswordAttributes().OuterXml;         
            //System.Diagnostics.Debug.WriteLine(actual);
            
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }


        /// <summary>
        ///A test for updateUser
        ///</summary>
        [TestMethod()]
        public void updateUserTest()
        {
            /*
            string username = "urbanekv"; // TODO: Initialize to an appropriate value
            string email = "vaclav.urbanek@ggg.cz"; // TODO: Initialize to an appropriate value
            bool setApproval = true; // TODO: Initialize to an appropriate value
            bool isApproved = true; // TODO: Initialize to an appropriate value
            NetMembershipServiceAgent.updateUser(username, email, setApproval, isApproved);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
             */
        }
    }
}
