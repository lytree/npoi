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

namespace TestCases.HSSF.Record
{

    using NUnit.Framework;using NUnit.Framework.Legacy;
    using NPOI.HSSF.Model;
    using NPOI.HSSF.Record.Common;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using TestCases.HSSF;
    using NPOI.HSSF.Record;
    using TestCases.HSSF.UserModel;
    /**
     * Tests for <tt>FeatRecord</tt>
     * 
     * @author Josh Micich
     */
    [TestFixture]
    public class TestFeatRecord
    {
        public void TestWithoutFeatRecord()
        {
            HSSFWorkbook hssf =
                   HSSFTestDataSamples.OpenSampleWorkbook("46136-WithWarnings.xls");
            InternalWorkbook wb = HSSFTestHelper.GetWorkbookForTest(hssf);

            ClassicAssert.AreEqual(1, hssf.NumberOfSheets);

            int countFR = 0;
            int countFRH = 0;

            // Check on the workbook, but shouldn't be there!
            foreach (Record r in wb.Records)
            {
                if (r is FeatRecord)
                {
                    countFR++;
                }
                else if (r.Sid == FeatRecord.sid)
                {
                    countFR++;
                }
                if (r is FeatHdrRecord)
                {
                    countFRH++;
                }
                else if (r.Sid == FeatHdrRecord.sid)
                {
                    countFRH++;
                }
            }

            ClassicAssert.AreEqual(0, countFR);
            ClassicAssert.AreEqual(0, countFRH);

            // Now check on the sheet
            HSSFSheet s = (HSSFSheet)hssf.GetSheetAt(0);
            InternalSheet sheet = HSSFTestHelper.GetSheetForTest(s);

            foreach (RecordBase rb in sheet.Records)
            {
                if (rb is Record)
                {
                    Record r = (Record)rb;
                    if (r is FeatRecord)
                    {
                        countFR++;
                    }
                    else if (r.Sid == FeatRecord.sid)
                    {
                        countFR++;
                    }
                    if (r is FeatHdrRecord)
                    {
                        countFRH++;
                    }
                    else if (r.Sid == FeatHdrRecord.sid)
                    {
                        countFRH++;
                    }
                }
            }

            ClassicAssert.AreEqual(0, countFR);
            ClassicAssert.AreEqual(0, countFRH);
        }

        public void TestReadFeatRecord()
        {
            HSSFWorkbook hssf =
                   HSSFTestDataSamples.OpenSampleWorkbook("46136-NoWarnings.xls");
            InternalWorkbook wb = HSSFTestHelper.GetWorkbookForTest(hssf);

            FeatRecord fr = null;
            FeatHdrRecord fhr = null;

            ClassicAssert.AreEqual(1, hssf.NumberOfSheets);

            // First check it isn't on the Workbook
            int countFR = 0;
            int countFRH = 0;
            foreach (Record r in wb.Records)
            {
                if (r is FeatRecord)
                {
                    fr = (FeatRecord)r;
                    countFR++;
                }
                else if (r.Sid == FeatRecord.sid)
                {
                    Assert.Fail("FeatRecord SID found but not Created correctly!");
                }
                if (r is FeatHdrRecord)
                {
                    countFRH++;
                }
                else if (r.Sid == FeatHdrRecord.sid)
                {
                    Assert.Fail("FeatHdrRecord SID found but not Created correctly!");
                }
            }

            ClassicAssert.AreEqual(0, countFR);
            ClassicAssert.AreEqual(0, countFRH);

            // Now find it on our sheet
            HSSFSheet s = (HSSFSheet)hssf.GetSheetAt(0);
            InternalSheet sheet = HSSFTestHelper.GetSheetForTest(s);

            foreach (RecordBase rb in sheet.Records)
            {
                if (rb is Record)
                {
                    Record r = (Record)rb;
                    if (r is FeatRecord)
                    {
                        fr = (FeatRecord)r;
                        countFR++;
                    }
                    else if (r.Sid == FeatRecord.sid)
                    {
                        countFR++;
                    }
                    if (r is FeatHdrRecord)
                    {
                        fhr = (FeatHdrRecord)r;
                        countFRH++;
                    }
                    else if (r.Sid == FeatHdrRecord.sid)
                    {
                        countFRH++;
                    }
                }
            }

            ClassicAssert.AreEqual(1, countFR);
            ClassicAssert.AreEqual(1, countFRH);
            ClassicAssert.IsNotNull(fr);
            ClassicAssert.IsNotNull(fhr);

            // Now check the contents are as expected
            ClassicAssert.AreEqual(
                    FeatHdrRecord.SHAREDFEATURES_ISFFEC2,
                    fr.Isf_sharedFeatureType
            );

            // Applies to one cell only
            ClassicAssert.AreEqual(1, fr.CellRefs.Length);
            ClassicAssert.AreEqual(0, fr.CellRefs[0].FirstRow);
            ClassicAssert.AreEqual(0, fr.CellRefs[0].LastRow);
            ClassicAssert.AreEqual(0, fr.CellRefs[0].FirstColumn);
            ClassicAssert.AreEqual(0, fr.CellRefs[0].LastColumn);

            // More Checking of shared features stuff
            ClassicAssert.AreEqual(4, fr.CbFeatData);
            ClassicAssert.AreEqual(4, fr.SharedFeature.DataSize);
            ClassicAssert.AreEqual(typeof(FeatFormulaErr2), fr.SharedFeature.GetType());

            FeatFormulaErr2 fferr2 = (FeatFormulaErr2)fr.SharedFeature;
            ClassicAssert.AreEqual(0x04, fferr2.RawErrorCheckValue);

            ClassicAssert.IsFalse(fferr2.CheckCalculationErrors);
            ClassicAssert.IsFalse(fferr2.CheckDateTimeFormats);
            ClassicAssert.IsFalse(fferr2.CheckEmptyCellRef);
            ClassicAssert.IsFalse(fferr2.CheckInconsistentFormulas);
            ClassicAssert.IsFalse(fferr2.CheckInconsistentRanges);
            ClassicAssert.IsTrue(fferr2.CheckNumbersAsText);
            ClassicAssert.IsFalse(fferr2.CheckUnprotectedFormulas);
            ClassicAssert.IsFalse(fferr2.PerformDataValidation);
        }

        /**
         *  cloning sheets with feat records 
         */
        public void TestCloneSheetWithFeatRecord()
        {
            IWorkbook wb =
                HSSFTestDataSamples.OpenSampleWorkbook("46136-WithWarnings.xls");
            wb.CloneSheet(0);
        }
    }

}