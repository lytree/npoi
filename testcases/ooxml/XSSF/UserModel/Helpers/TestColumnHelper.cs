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

using NPOI.OpenXmlFormats.Spreadsheet;
using NUnit.Framework;using NUnit.Framework.Legacy;
using NPOI.XSSF.Model;
using NPOI.XSSF.UserModel.Helpers;
using NPOI.XSSF.UserModel;
using System;

namespace TestCases.XSSF.UserModel.Helpers
{
    /**
     * Tests for {@link ColumnHelper}
     *
     */
    [TestFixture]
    [Obsolete("Use XSSFColumn object for all things column")]
    public class TestColumnHelper
    {
        [Test]
        public void TestCleanColumns()
        {
            CT_Worksheet worksheet = new CT_Worksheet();

            CT_Cols cols1 = worksheet.AddNewCols();
            CT_Col col1 = cols1.AddNewCol();
            col1.min = (1);
            col1.max = (1);
            col1.width = (88);
            col1.hidden = (true);
            CT_Col col2 = cols1.AddNewCol();
            col2.min = (2);
            col2.max = (3);
            CT_Cols cols2 = worksheet.AddNewCols();
            CT_Col col4 = cols2.AddNewCol();
            col4.min = (13);
            col4.max = (16384);

            // Test cleaning cols
            ClassicAssert.AreEqual(2, worksheet.sizeOfColsArray());
            int count = countColumns(worksheet);
            ClassicAssert.AreEqual(16375, count);
            // Clean columns and Test a clean worksheet
            ColumnHelper helper = new ColumnHelper(worksheet);
            ClassicAssert.AreEqual(1, worksheet.sizeOfColsArray());
            count = countColumns(worksheet);
            ClassicAssert.AreEqual(16375, count);
            // Remember - POI column 0 == OOXML column 1
            ClassicAssert.AreEqual(88.0, helper.GetColumn(0, false).width, 0.0);
            ClassicAssert.IsTrue(helper.GetColumn(0, false).hidden);
            ClassicAssert.AreEqual(0.0, helper.GetColumn(1, false).width, 0.0);
            ClassicAssert.IsFalse(helper.GetColumn(1, false).hidden);
        }
        [Test]
        public void TestSortColumns()
        {
            CT_Cols cols1 = new CT_Cols();
            CT_Col col1 = cols1.AddNewCol();
            col1.min = (1);
            col1.max = (1);
            col1.width = (88);
            col1.hidden = (true);
            CT_Col col2 = cols1.AddNewCol();
            col2.min = (2);
            col2.max = (3);
            CT_Col col3 = cols1.AddNewCol();
            col3.min = (13);
            col3.max = (16750);
            ClassicAssert.AreEqual(3, cols1.sizeOfColArray());
            CT_Col col4 = cols1.AddNewCol();
            col4.min = (8);
            col4.max = (11);
            ClassicAssert.AreEqual(4, cols1.sizeOfColArray());
            CT_Col col5 = cols1.AddNewCol();
            col5.min = (4);
            col5.max = (5);
            ClassicAssert.AreEqual(5, cols1.sizeOfColArray());
            CT_Col col6 = cols1.AddNewCol();
            col6.min = (8);
            col6.max = (9);
            col6.hidden = (true);
            CT_Col col7 = cols1.AddNewCol();
            col7.min = (6);
            col7.max = (8);
            col7.width = (17.0);
            CT_Col col8 = cols1.AddNewCol();
            col8.min = (25);
            col8.max = (27);
            CT_Col col9 = cols1.AddNewCol();
            col9.min = (20);
            col9.max = (30);
            ClassicAssert.AreEqual(9, cols1.sizeOfColArray());
            ClassicAssert.AreEqual(20u, cols1.GetColArray(8).min);
            ClassicAssert.AreEqual(30u, cols1.GetColArray(8).max);
            ColumnHelper.SortColumns(cols1);
            ClassicAssert.AreEqual(9, cols1.sizeOfColArray());
            ClassicAssert.AreEqual(25u, cols1.GetColArray(8).min);
            ClassicAssert.AreEqual(27u, cols1.GetColArray(8).max);
        }
        [Test]
        public void TestCloneCol()
        {
            CT_Worksheet worksheet = new CT_Worksheet();
            ColumnHelper helper = new ColumnHelper(worksheet);

            CT_Cols cols = new CT_Cols();
            CT_Col col = new CT_Col();
            col.min = (2);
            col.max = (8);
            col.hidden = (true);
            col.width = (13.4);
            CT_Col newCol = helper.CloneCol(cols, col);
            ClassicAssert.AreEqual(2u, newCol.min);
            ClassicAssert.AreEqual(8u, newCol.max);
            ClassicAssert.IsTrue(newCol.hidden);
            ClassicAssert.AreEqual(13.4, newCol.width, 0.0);
        }
        [Test]
        public void TestAddCleanColIntoCols()
        {
            CT_Worksheet worksheet = new CT_Worksheet();
            ColumnHelper helper = new ColumnHelper(worksheet);

            CT_Cols cols1 = new CT_Cols();
            CT_Col col1 = cols1.AddNewCol();
            col1.min = (1);
            col1.max = (1);
            col1.width = (88);
            col1.hidden = (true);
            CT_Col col2 = cols1.AddNewCol();
            col2.min = (2);
            col2.max = (3);
            CT_Col col3 = cols1.AddNewCol();
            col3.min = (13);
            col3.max = (16750);
            ClassicAssert.AreEqual(3, cols1.sizeOfColArray());
            CT_Col col4 = cols1.AddNewCol();
            col4.min = (8);
            col4.max = (9);
            ClassicAssert.AreEqual(4, cols1.sizeOfColArray());

            // No overlap
            helper.AddCleanColIntoCols(cols1, createCol(4, 5));
            ClassicAssert.AreEqual(5, cols1.sizeOfColArray());

            // Overlaps with 8 - 9 (overlap and after replacements required)
            CT_Col col6 = createCol(8, 11);
            col6.hidden = (true);
            helper.AddCleanColIntoCols(cols1, col6);
            ClassicAssert.AreEqual(6, cols1.sizeOfColArray());

            // Overlaps with 8 - 9 (before and overlap replacements required)
            CT_Col col7 = createCol(6, 8);
            col7.width = (17.0);
            helper.AddCleanColIntoCols(cols1, col7);
            ClassicAssert.AreEqual(8, cols1.sizeOfColArray());

            // Overlaps with 13 - 16750 (before, overlap and after replacements required)
            helper.AddCleanColIntoCols(cols1, createCol(20, 30));
            ClassicAssert.AreEqual(10, cols1.sizeOfColArray());

            // Overlaps with 20 - 30 (before, overlap and after replacements required)
            helper.AddCleanColIntoCols(cols1, createCol(25, 27));

            // TODO - assert something interesting
            ClassicAssert.AreEqual(12, cols1.col.Count);
            ClassicAssert.AreEqual(1u, cols1.GetColArray(0).min);
            ClassicAssert.AreEqual(16750u, cols1.GetColArray(11).max);
        }

        [Test]
        public void TestAddCleanColIntoColsExactOverlap()
        {
            CT_Cols cols = createHiddenAndBestFitColsWithHelper(1, 1, 1, 1);
            ClassicAssert.AreEqual(1, cols.sizeOfColArray());
            assertMinMaxHiddenBestFit(cols, 0, 1, 1, true, true);
        }

        [Test]
        public void TestAddCleanColIntoColsOverlapsOverhangingBothSides()
        {
            CT_Cols cols = createHiddenAndBestFitColsWithHelper(2, 2, 1, 3);
            ClassicAssert.AreEqual(3, cols.sizeOfColArray());
            assertMinMaxHiddenBestFit(cols, 0, 1, 1, false, true);
            assertMinMaxHiddenBestFit(cols, 1, 2, 2, true, true);
            assertMinMaxHiddenBestFit(cols, 2, 3, 3, false, true);
        }
        [Test]
        public void TestAddCleanColIntoColsOverlapsCompletelyNested()
        {
            CT_Cols cols = createHiddenAndBestFitColsWithHelper(1, 3, 2, 2);
            ClassicAssert.AreEqual(3, cols.sizeOfColArray());
            assertMinMaxHiddenBestFit(cols, 0, 1, 1, true, false);
            assertMinMaxHiddenBestFit(cols, 1, 2, 2, true, true);
            assertMinMaxHiddenBestFit(cols, 2, 3, 3, true, false);
        }

        [Test]
        public void TestAddCleanColIntoColsNewOverlapsOverhangingLeftNotRightExactRight()
        {
            CT_Cols cols = createHiddenAndBestFitColsWithHelper(2, 3, 1, 3);
            ClassicAssert.AreEqual(2, cols.sizeOfColArray());
            assertMinMaxHiddenBestFit(cols, 0, 1, 1, false, true);
            assertMinMaxHiddenBestFit(cols, 1, 2, 3, true, true);
        }

        [Test]
        public void TestAddCleanColIntoColsNewOverlapsOverhangingRightNotLeftExactLeft()
        {
            CT_Cols cols = createHiddenAndBestFitColsWithHelper(1, 2, 1, 3);
            ClassicAssert.AreEqual(2, cols.sizeOfColArray());
            assertMinMaxHiddenBestFit(cols, 0, 1, 2, true, true);
            assertMinMaxHiddenBestFit(cols, 1, 3, 3, false, true);
        }

        [Test]
        public void TestAddCleanColIntoColsNewOverlapsOverhangingLeftNotRight()
        {
            CT_Cols cols = createHiddenAndBestFitColsWithHelper(2, 3, 1, 2);
            ClassicAssert.AreEqual(3, cols.sizeOfColArray());
            assertMinMaxHiddenBestFit(cols, 0, 1, 1, false, true);
            assertMinMaxHiddenBestFit(cols, 1, 2, 2, true, true);
            assertMinMaxHiddenBestFit(cols, 2, 3, 3, true, false);
        }

        [Test]
        public void TestAddCleanColIntoColsNewOverlapsOverhangingRightNotLeft()
        {
            CT_Cols cols = createHiddenAndBestFitColsWithHelper(1, 2, 2, 3);
            ClassicAssert.AreEqual(3, cols.sizeOfColArray());
            assertMinMaxHiddenBestFit(cols, 0, 1, 1, true, false);
            assertMinMaxHiddenBestFit(cols, 1, 2, 2, true, true);
            assertMinMaxHiddenBestFit(cols, 2, 3, 3, false, true);
        }
        /**
         * Creates and adds a hidden column and then a best fit column with the given min/max pairs.
         * Suitable for testing handling of overlap. 
         */
        private CT_Cols createHiddenAndBestFitColsWithHelper(int hiddenMin, int hiddenMax, int bestFitMin, int bestFitMax)
        {
            CT_Worksheet worksheet = new CT_Worksheet();
            ColumnHelper helper = new ColumnHelper(worksheet);
            CT_Cols cols = worksheet.GetColsArray(0);
            CT_Col hidden = createCol(hiddenMin, hiddenMax);
            hidden.hidden = (true);
            helper.AddCleanColIntoCols(cols, hidden);
            CT_Col bestFit = createCol(bestFitMin, bestFitMax);
            bestFit.bestFit = (true);
            helper.AddCleanColIntoCols(cols, bestFit);
            return cols;
        }
        private void assertMinMaxHiddenBestFit(CT_Cols cols, int index, int min, int max, bool hidden, bool bestFit)
        {
            CT_Col col = cols.GetColArray(index);
            ClassicAssert.AreEqual(min, col.min);
            ClassicAssert.AreEqual(max, col.max);
            ClassicAssert.AreEqual(hidden, col.hidden);
            ClassicAssert.AreEqual(bestFit, col.bestFit);
        }
        private CT_Col createCol(int min, int max)
        {
            CT_Col col = new CT_Col();
            col.min = (uint)(min);
            col.max = (uint)(max);
            return col;
        }

        [Test]
        public void TestGetColumn()
        {
            CT_Worksheet worksheet = new CT_Worksheet();

            CT_Cols cols1 = worksheet.AddNewCols();
            CT_Col col1 = cols1.AddNewCol();
            col1.min = (1);
            col1.max = (1);
            col1.width = (88);
            col1.hidden = (true);
            CT_Col col2 = cols1.AddNewCol();
            col2.min = (2);
            col2.max = (3);
            CT_Cols cols2 = worksheet.AddNewCols();
            CT_Col col4 = cols2.AddNewCol();
            col4.min = (3);
            col4.max = (6);

            // Remember - POI column 0 == OOXML column 1
            ColumnHelper helper = new ColumnHelper(worksheet);
            ClassicAssert.IsNotNull(helper.GetColumn(0, false));
            ClassicAssert.IsNotNull(helper.GetColumn(1, false));
            ClassicAssert.AreEqual(88.0, helper.GetColumn(0, false).width, 0.0);
            ClassicAssert.AreEqual(0.0, helper.GetColumn(1, false).width, 0.0);
            ClassicAssert.IsTrue(helper.GetColumn(0, false).hidden);
            ClassicAssert.IsFalse(helper.GetColumn(1, false).hidden);
            ClassicAssert.IsNull(helper.GetColumn(99, false));
            ClassicAssert.IsNotNull(helper.GetColumn(5, false));
        }
        [Test]
        public void TestSetColumnAttributes()
        {
            CT_Col col = new CT_Col();
            col.width = (12);
            col.hidden = (true);
            CT_Col newCol = new CT_Col();
            ClassicAssert.AreEqual(0.0, newCol.width, 0.0);
            ClassicAssert.IsFalse(newCol.hidden);
            ColumnHelper helper = new ColumnHelper(new CT_Worksheet());
            helper.SetColumnAttributes(col, newCol);
            ClassicAssert.AreEqual(12.0, newCol.width, 0.0);
            ClassicAssert.IsTrue(newCol.hidden);
        }
        [Test]
        public void TestGetOrCreateColumn()
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet("Sheet 1");
            ColumnHelper columnHelper = sheet.GetColumnHelper();

            // Check POI 0 based, OOXML 1 based
            CT_Col col = columnHelper.GetOrCreateColumn1Based(3, false);
            ClassicAssert.IsNotNull(col);
            ClassicAssert.IsNull(columnHelper.GetColumn(1, false));
            ClassicAssert.IsNotNull(columnHelper.GetColumn(2, false));
            ClassicAssert.IsNotNull(columnHelper.GetColumn1Based(3, false));
            ClassicAssert.IsNull(columnHelper.GetColumn(3, false));

            CT_Col col2 = columnHelper.GetOrCreateColumn1Based(30, false);
            ClassicAssert.IsNotNull(col2);
            ClassicAssert.IsNull(columnHelper.GetColumn(28, false));
            ClassicAssert.IsNotNull(columnHelper.GetColumn(29, false));
            ClassicAssert.IsNotNull(columnHelper.GetColumn1Based(30, false));
            ClassicAssert.IsNull(columnHelper.GetColumn(30, false));
        }
        [Test]
        public void TestGetSetColDefaultStyle()
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet();
            CT_Worksheet ctWorksheet = sheet.GetCTWorksheet();
            ColumnHelper columnHelper = sheet.GetColumnHelper();

            // POI column 3, OOXML column 4
            CT_Col col = columnHelper.GetOrCreateColumn1Based(4, false);

            ClassicAssert.IsNotNull(col);
            ClassicAssert.IsNotNull(columnHelper.GetColumn(3, false));
            columnHelper.SetColDefaultStyle(3, 2);
            ClassicAssert.AreEqual(2, columnHelper.GetColDefaultStyle(3));
            ClassicAssert.AreEqual(-1, columnHelper.GetColDefaultStyle(4));
            StylesTable stylesTable = workbook.GetStylesSource();
            CT_Xf cellXf = new CT_Xf();
            cellXf.fontId = (0);
            cellXf.fillId = (0);
            cellXf.borderId = (0);
            cellXf.numFmtId = (0);
            cellXf.xfId = (0);
            stylesTable.PutCellXf(cellXf);
            CT_Col col_2 = ctWorksheet.GetColsArray(0).AddNewCol();
            col_2.min = (10);
            col_2.max = (12);
            col_2.style = (1);
            ClassicAssert.AreEqual(1, columnHelper.GetColDefaultStyle(11));
            XSSFCellStyle cellStyle = new XSSFCellStyle(0, 0, stylesTable, null);
            columnHelper.SetColDefaultStyle(11, cellStyle);
            ClassicAssert.AreEqual(0u, col_2.style);
            ClassicAssert.AreEqual(1, columnHelper.GetColDefaultStyle(10));
        }

        private static int countColumns(CT_Worksheet worksheet)
        {
            int count;
            count = 0;
            for (int i = 0; i < worksheet.sizeOfColsArray(); i++)
            {
                for (int y = 0; y < worksheet.GetColsArray(i).sizeOfColArray(); y++)
                {
                    for (long k = worksheet.GetColsArray(i).GetColArray(y).min; k <= worksheet
                            .GetColsArray(i).GetColArray(y).max; k++)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}

