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
namespace NPOI.XWPF.Model
{
    using System;
    using NPOI.XWPF.UserModel;
    using NPOI.OpenXmlFormats.Wordprocessing;
    using System.IO;
    using System.Xml.Serialization;
    using System.Xml;
    using NPOI.OpenXmlFormats.Vml;
    using NPOI.OpenXmlFormats.Vml.Office;
    using System.Diagnostics;
    using static System.Net.WebRequestMethods;

    /**
     * A .docx file can have no headers/footers, the same header/footer
     *  on each page, odd/even page footers, and optionally also 
     *  a different header/footer on the first page.
     * This class handles sorting out what there is, and giving you
     *  the right headers and footers for the document.
     */
    public class XWPFHeaderFooterPolicy
    {
        public static ST_HdrFtr DEFAULT = ST_HdrFtr.@default;
        public static ST_HdrFtr EVEN = ST_HdrFtr.even;
        public static ST_HdrFtr FIRST = ST_HdrFtr.first;

        private XWPFDocument doc;

        private XWPFHeader firstPageHeader;
        private XWPFFooter firstPageFooter;

        private XWPFHeader evenPageHeader;
        private XWPFFooter evenPageFooter;

        private XWPFHeader defaultHeader;
        private XWPFFooter defaultFooter;

        /**
         * Figures out the policy for the given document,
         *  and Creates any header and footer objects
         *  as required.
         */
        public XWPFHeaderFooterPolicy(XWPFDocument doc)
            : this(doc, null)
        {
        }

        /**
         * Figures out the policy for the given document,
         *  and Creates any header and footer objects
         *  as required.
         */
        public XWPFHeaderFooterPolicy(XWPFDocument doc, CT_SectPr sectPr)
        {
            // Grab what headers and footers have been defined
            // For now, we don't care about different ranges, as it
            //  doesn't seem that .docx properly supports that
            //  feature of the file format yet
            if (sectPr == null)
            {
                CT_Body ctBody = doc.Document.body;
                sectPr = ctBody.IsSetSectPr()
                        ? ctBody.sectPr
                        : ctBody.AddNewSectPr();
            }
            this.doc = doc;
            for (int i = 0; i < sectPr.SizeOfHeaderReferenceArray(); i++)
            {
                // Get the header
                CT_HdrFtrRef ref1 = sectPr.GetHeaderReferenceArray(i);
                POIXMLDocumentPart relatedPart = doc.GetRelationById(ref1.id);
                XWPFHeader hdr = null;
                if (relatedPart != null && relatedPart is XWPFHeader part)
                {
                    hdr = part;
                }
                // Assign it
                ST_HdrFtr type = ref1.type;
                assignHeader(hdr, type);
            }
            for (int i = 0; i < sectPr.SizeOfFooterReferenceArray(); i++)
            {
                // Get the footer
                CT_HdrFtrRef ref1 = sectPr.GetFooterReferenceArray(i);
                POIXMLDocumentPart relatedPart = doc.GetRelationById(ref1.id);
                XWPFFooter ftr = null;
                if (relatedPart != null && relatedPart is XWPFFooter part)
                {
                    ftr = part;
                }
                // Assign it
                ST_HdrFtr type = ref1.type;
                assignFooter(ftr, type);
            }
        }

        private void assignFooter(XWPFFooter ftr, ST_HdrFtr type)
        {
            if (type == ST_HdrFtr.first)
            {
                firstPageFooter = ftr;
            }
            else if (type == ST_HdrFtr.even)
            {
                evenPageFooter = ftr;
            }
            else
            {
                defaultFooter = ftr;
            }
        }

        private void assignHeader(XWPFHeader hdr, ST_HdrFtr type)
        {
            if (type == ST_HdrFtr.first)
            {
                firstPageHeader = hdr;
            }
            else if (type == ST_HdrFtr.even)
            {
                evenPageHeader = hdr;
            }
            else
            {
                defaultHeader = hdr;
            }
        }

        public XWPFHeader CreateHeader(ST_HdrFtr type)
        {
            return CreateHeader(type, null);
        }

        public XWPFHeader CreateHeader(ST_HdrFtr type, XWPFParagraph[] pars)
        {
            XWPFHeader header = GetHeader(type);

            if (header == null)
            {
                HdrDocument hdrDoc = new HdrDocument();

                XWPFRelation relation = XWPFRelation.HEADER;
                int i = GetRelationIndex(relation);

                XWPFHeader wrapper = (XWPFHeader)doc.CreateRelationship(relation,
                        XWPFFactory.GetInstance(), i);
                wrapper.SetXWPFDocument(doc);

                String pStyle = "Header";
                CT_HdrFtr hdr = buildHdr(type, pStyle, wrapper, pars);
                wrapper.SetHeaderFooter(hdr);
                hdrDoc.SetHdr((CT_Hdr)hdr);
                assignHeader(wrapper, type);
                header = wrapper;
            }

            return header;
        }

        public XWPFFooter CreateFooter(ST_HdrFtr type)
        {
            return CreateFooter(type, null);
        }

        public XWPFFooter CreateFooter(ST_HdrFtr type, XWPFParagraph[] pars)
        {
            XWPFFooter footer = GetFooter(type);

            if (footer == null)
            {
                FtrDocument ftrDoc = new FtrDocument();

                XWPFRelation relation = XWPFRelation.FOOTER;
                int i = GetRelationIndex(relation);

                XWPFFooter wrapper = (XWPFFooter)doc.CreateRelationship(relation,
                        XWPFFactory.GetInstance(), i);
                wrapper.SetXWPFDocument(doc);

                String pStyle = "Footer";
                CT_HdrFtr ftr = buildFtr(type, pStyle, wrapper, pars);
                wrapper.SetHeaderFooter(ftr);
                ftrDoc.SetFtr((CT_Ftr)ftr);
                assignFooter(wrapper, type);
                footer = wrapper;
            }

            return footer;
        }

        private int GetRelationIndex(XWPFRelation relation)
        {
            int i = 1;
            foreach (POIXMLDocumentPart.RelationPart rp in doc.RelationParts)
            {
                if (rp.Relationship.RelationshipType.Equals(relation.Relation))
                {
                    i++;
                }
            }
            return i;
        }

        private CT_HdrFtr buildFtr(ST_HdrFtr type, String pStyle, XWPFHeaderFooter wrapper, XWPFParagraph[] pars)
        {
            //CTHdrFtr ftr = buildHdrFtr(pStyle, pars);				// MB 24 May 2010
            CT_HdrFtr ftr = buildHdrFtr(pStyle, pars, wrapper);		// MB 24 May 2010
            SetFooterReference(type, wrapper);
            return ftr;
        }

        private CT_HdrFtr buildHdr(ST_HdrFtr type, String pStyle, XWPFHeaderFooter wrapper, XWPFParagraph[] pars)
        {
            //CTHdrFtr hdr = buildHdrFtr(pStyle, pars);				// MB 24 May 2010
            CT_HdrFtr hdr = buildHdrFtr(pStyle, pars, wrapper);		// MB 24 May 2010
            SetHeaderReference(type, wrapper);
            return hdr;
        }

        /**
         * MB 24 May 2010. Created this overloaded buildHdrFtr() method because testing demonstrated
         * that the XWPFFooter or XWPFHeader object returned by calls to the CreateHeader(int, XWPFParagraph[])
         * and CreateFooter(int, XWPFParagraph[]) methods or the GetXXXXXHeader/Footer methods where
         * headers or footers had been Added to a document since it had been Created/opened, returned
         * an object that Contained no XWPFParagraph objects even if the header/footer itself did contain
         * text. The reason was that this line of code; CTHdrFtr ftr = CTHdrFtr.Factory.NewInstance(); 
         * Created a brand new instance of the CTHDRFtr class which was then populated with data when
         * it should have recovered the CTHdrFtr object encapsulated within the XWPFHeaderFooter object
         * that had previoulsy been instantiated in the CreateHeader(int, XWPFParagraph[]) or 
         * CreateFooter(int, XWPFParagraph[]) methods.
         */
        private static CT_HdrFtr buildHdrFtr(String pStyle, XWPFParagraph[] paragraphs, XWPFHeaderFooter wrapper)
        {
            CT_HdrFtr ftr = wrapper._getHdrFtr();
            if (paragraphs != null) {
                for (int i = 0 ; i < paragraphs.Length ; i++) {
                    CT_P p = ftr.AddNewP();
                    ftr.SetPArray(i, paragraphs[i].GetCTP());
                }
            }
            else {
                //CT_P p = ftr.AddNewP();
                //CT_Body body = doc.Document.body;
                //if (body.SizeOfPArray() > 0)
                //{
                //    CT_P p0 = body.GetPArray(0);
                //    if (p0.IsSetRsidR())
                //    {
                //        byte[] rsidr = p0.rsidR;
                //        byte[] rsidrdefault = p0.rsidRDefault;
                //        p.rsidP = rsidr;
                //        p.rsidRDefault = rsidrdefault;
                //    }
                //}
                //CT_PPr pPr = p.AddNewPPr();
                //pPr.AddNewPStyle().val = (pStyle);
            }
            return ftr;
        }


        private void SetFooterReference(ST_HdrFtr type, XWPFHeaderFooter wrapper)
        {
            CT_HdrFtrRef ref1 = doc.Document.body.sectPr.AddNewFooterReference();
            ref1.type = (type);
            ref1.id = (doc.GetRelationId(wrapper));
        }


        private void SetHeaderReference(ST_HdrFtr type, XWPFHeaderFooter wrapper)
        {
            CT_HdrFtrRef ref1 = doc.Document.body.sectPr.AddNewHeaderReference();
            ref1.type = (type);
            ref1.id = (doc.GetRelationId(wrapper));
        }


        private static XmlSerializerNamespaces Commit(XWPFHeaderFooter wrapper)
        {
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] {
                new XmlQualifiedName("ve", "http://schemas.openxmlformats.org/markup-compatibility/2006"),
                new XmlQualifiedName("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"),
                new XmlQualifiedName("m", "http://schemas.openxmlformats.org/officeDocument/2006/math"),
                new XmlQualifiedName("v", "urn:schemas-microsoft-com:vml"),
                new XmlQualifiedName("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"),
                new XmlQualifiedName("w10", "urn:schemas-microsoft-com:office:word"),
                new XmlQualifiedName("wne", "http://schemas.microsoft.com/office/word/2006/wordml"),
                 new XmlQualifiedName("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main")
             });
            return namespaces;
        }

        public XWPFHeader GetFirstPageHeader()
        {
            return firstPageHeader;
        }
        public XWPFFooter GetFirstPageFooter()
        {
            return firstPageFooter;
        }
        /**
         * Returns the odd page header. This is
         *  also the same as the default one...
         */
        public XWPFHeader GetOddPageHeader()
        {
            return defaultHeader;
        }
        /**
         * Returns the odd page footer. This is
         *  also the same as the default one...
         */
        public XWPFFooter GetOddPageFooter()
        {
            return defaultFooter;
        }
        public XWPFHeader GetEvenPageHeader()
        {
            return evenPageHeader;
        }
        public XWPFFooter GetEvenPageFooter()
        {
            return evenPageFooter;
        }
        public XWPFHeader GetDefaultHeader()
        {
            return defaultHeader;
        }
        public XWPFFooter GetDefaultFooter()
        {
            return defaultFooter;
        }

        /**
         * Get the header that applies to the given
         *  (1 based) page.
         * @param pageNumber The one based page number
         */
        public XWPFHeader GetHeader(int pageNumber)
        {
            if (pageNumber == 1 && firstPageHeader != null)
            {
                return firstPageHeader;
            }
            if (pageNumber % 2 == 0 && evenPageHeader != null)
            {
                return evenPageHeader;
            }
            return defaultHeader;
        }

        /**
         * Get this section header for the given type
         *
         * @param type of header to return
         * @return {@link XWPFHeader} object
         */
        public XWPFHeader GetHeader(ST_HdrFtr type)
        {
            if (type == ST_HdrFtr.even)
            {
                return evenPageHeader;
            }
            else if (type == ST_HdrFtr.first)
            {
                return firstPageHeader;
            }
            return defaultHeader;
        }

        /**
         * Get the footer that applies to the given
         *  (1 based) page.
         * @param pageNumber The one based page number
         */
        public XWPFFooter GetFooter(int pageNumber)
        {
            if (pageNumber == 1 && firstPageFooter != null)
            {
                return firstPageFooter;
            }
            if (pageNumber % 2 == 0 && evenPageFooter != null)
            {
                return evenPageFooter;
            }
            return defaultFooter;
        }

        /**
         * Get this section footer for the given type
         *
         * @param type of footer to return
         * @return {@link XWPFFooter} object
         */
        public XWPFFooter GetFooter(ST_HdrFtr type)
        {
            if (type == ST_HdrFtr.even)
            {
                return evenPageFooter;
            }
            else if (type == ST_HdrFtr.first)
            {
                return firstPageFooter;
            }
            return defaultFooter;
        }

        public void CreateWatermark(String text)
        {
            XWPFParagraph[] pars = new XWPFParagraph[1];
            pars[0] = GetWatermarkParagraph(text, 1);
            CreateHeader(DEFAULT, pars);
            pars[0] = GetWatermarkParagraph(text, 2);
            CreateHeader(FIRST, pars);
            pars[0] = GetWatermarkParagraph(text, 3);
            CreateHeader(EVEN, pars);
        }

        /*
         * This is the default Watermark paragraph; the only variable is the text message
         * TODO: manage all the other variables
         */
        private XWPFParagraph GetWatermarkParagraph(String text, int idx)
        {
            CT_P p = new CT_P();
            byte[] rsidr = null;
            byte[] rsidrdefault = null;
            CT_Body ctBody = doc.Document.body;
            if(ctBody.SizeOfPArray() != 0)
            {
                CT_P ctp = ctBody.GetPArray(0);
                rsidr = ctp.rsidR;
                rsidrdefault = ctp.rsidRDefault;
            }
            else
            {
                // TODO generate rsidr and rsidrdefault
            }
            p.rsidP = rsidr;
            p.rsidRDefault = rsidrdefault;
            CT_PPr pPr = p.AddNewPPr();
            pPr.AddNewPStyle().val = "Header";
            // start watermark paragraph
            NPOI.OpenXmlFormats.Wordprocessing.CT_R r = p.AddNewR();
            CT_RPr rPr = r.AddNewRPr();
            rPr.AddNewNoProof();
            CT_Picture pict = r.AddNewPict();

            CT_Group group = new CT_Group();
            CT_Shapetype shapetype = group.AddNewShapetype();
            shapetype.id = ("_x0000_t136");
            shapetype.coordsize = ("1600,21600");
            shapetype.spt = (136);
            shapetype.adj = ("10800");
            shapetype.path2 = ("m@7,0l@8,0m@5,21600l@6,21600e");
            CT_Formulas formulas = shapetype.AddNewFormulas();
            formulas.AddNewF().eqn=("sum #0 0 10800");
            formulas.AddNewF().eqn = ("prod #0 2 1");
            formulas.AddNewF().eqn = ("sum 21600 0 @1");
            formulas.AddNewF().eqn = ("sum 0 0 @2");
            formulas.AddNewF().eqn = ("sum 21600 0 @3");
            formulas.AddNewF().eqn = ("if @0 @3 0");
            formulas.AddNewF().eqn = ("if @0 21600 @1");
            formulas.AddNewF().eqn = ("if @0 0 @2");
            formulas.AddNewF().eqn = ("if @0 @4 21600");
            formulas.AddNewF().eqn = ("mid @5 @6");
            formulas.AddNewF().eqn = ("mid @8 @5");
            formulas.AddNewF().eqn = ("mid @7 @8");
            formulas.AddNewF().eqn = ("mid @6 @7");
            formulas.AddNewF().eqn = ("sum @6 0 @5");
            CT_Path path = shapetype.AddNewPath();
            path.textpathok=(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t);
            path.connecttype=(ST_ConnectType.custom);
            path.connectlocs=("@9,0;@10,10800;@11,21600;@12,10800");
            path.connectangles=("270,180,90,0");
            CT_TextPath shapeTypeTextPath = shapetype.AddNewTextpath();
            shapeTypeTextPath.on=(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t);
            shapeTypeTextPath.fitshape=(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t);
            CT_Handles handles = shapetype.AddNewHandles();
            CT_H h = handles.AddNewH();
            h.position=("#0,bottomRight");
            h.xrange=("6629,14971");
            NPOI.OpenXmlFormats.Vml.Office.CT_Lock lock1 = shapetype.AddNewLock();
            lock1.ext=(ST_Ext.edit);
            CT_Shape shape = group.AddNewShape();
            shape.id = ("PowerPlusWaterMarkObject" + idx);
            shape.spid = ("_x0000_s102" + (4 + idx));
            shape.type = ("#_x0000_t136");
            shape.style = ("position:absolute;left:0;text-align:left;margin-left:0;margin-top:0;width:415pt;height:73.15pt;rotation:315;z-index:-251654144;mso-wrap-edited:f;mso-position-horizontal:center;mso-position-horizontal-relative:margin;mso-position-vertical:center;mso-position-vertical-relative:margin");
            //shape.wrapcoords = ("616 5068 390 16297 39 16921 -39 17155 7265 17545 7186 17467 -39 17467 18904 17467 10507 17467 8710 17545 18904 17077 18787 16843 18358 16297 18279 12554 19178 12476 20701 11774 20779 11228 21131 10059 21248 8811 21248 7563 20975 6316 20935 5380 19490 5146 14022 5068 2616 5068");
            shape.fillcolor = "silver";
            shape.stroked = (NPOI.OpenXmlFormats.Vml.ST_TrueFalse.@false);
            CT_TextPath shapeTextPath = shape.AddNewTextpath();
            shapeTextPath.style="font-family:\"Cambria\";;font-size:1pt";
            shapeTextPath.@string=(text);
            var shapefill=shape.AddNewFill();
            shapefill.opacity = ".5";
            pict.Set(group);
            // end watermark paragraph
            return new XWPFParagraph(p, doc);
        }
    }

}
