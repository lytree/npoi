/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

using System;
using NPOI.XSSF.UserModel.Helpers;
using NUnit.Framework;using NUnit.Framework.Legacy;
namespace TestCases.XSSF.UserModel.Helpers
{

    /**
     * Test the header and footer helper.
     * As we go through XmlBeans, should always use &,
     *  and not &amp;
     */
    [TestFixture]
    public class TestHeaderFooterHelper
    {
        [Test]
        public void TestGetCenterLeftRightSection()
        {
            HeaderFooterHelper helper = new HeaderFooterHelper();

            String headerFooter = "&CTest the center section";
            ClassicAssert.AreEqual("Test the center section", helper.GetCenterSection(headerFooter));

            headerFooter = "&CTest the center section&LThe left one&RAnd the right one";
            ClassicAssert.AreEqual("Test the center section", helper.GetCenterSection(headerFooter));
            ClassicAssert.AreEqual("The left one", helper.GetLeftSection(headerFooter));
            ClassicAssert.AreEqual("And the right one", helper.GetRightSection(headerFooter));
        }
        [Test]
        public void TestSetCenterLeftRightSection()
        {
            HeaderFooterHelper helper = new HeaderFooterHelper();
            String headerFooter = "";
            headerFooter = helper.SetCenterSection(headerFooter, "First Added center section");
            ClassicAssert.AreEqual("First Added center section", helper.GetCenterSection(headerFooter));
            headerFooter = helper.SetLeftSection(headerFooter, "First left");
            ClassicAssert.AreEqual("First left", helper.GetLeftSection(headerFooter));

            headerFooter = helper.SetRightSection(headerFooter, "First right");
            ClassicAssert.AreEqual("First right", helper.GetRightSection(headerFooter));
            ClassicAssert.AreEqual("&CFirst Added center section&LFirst left&RFirst right", headerFooter);

            headerFooter = helper.SetRightSection(headerFooter, "First right&F");
            ClassicAssert.AreEqual("First right&F", helper.GetRightSection(headerFooter));
            ClassicAssert.AreEqual("&CFirst Added center section&LFirst left&RFirst right&F", headerFooter);

            headerFooter = helper.SetRightSection(headerFooter, "First right&");
            ClassicAssert.AreEqual("First right&", helper.GetRightSection(headerFooter));
            ClassicAssert.AreEqual("&CFirst Added center section&LFirst left&RFirst right&", headerFooter);
        }

    }
}
