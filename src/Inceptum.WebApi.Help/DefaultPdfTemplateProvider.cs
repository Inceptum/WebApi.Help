using System;
using Inceptum.WebApi.Help.Builders;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;

namespace Inceptum.WebApi.Help
{
    internal class DefaultPdfTemplateProvider : IPdfTemplateProvider
    {
         
        public Paragraph GetTemplate(string name, object data, BaseFont defaultFont)
        {
            if (name == "apiMethod" && data is ApiDocumentationBuilder.ApiDescriptionDto)
            {
                var d = data as ApiDocumentationBuilder.ApiDescriptionDto;
                var paragraph = new Paragraph { Font = new Font(defaultFont, 12) };
                paragraph.Add(new Chunk(d.Documentation??""));
                var uriParagraph = new Paragraph((d.HttpMethod ?? "") + ": " + (d.FullPath ?? ""));
                uriParagraph.SpacingAfter = 20;
                uriParagraph.SpacingBefore = 20;
                uriParagraph.Role=PdfName.FRM;
                
                paragraph.Add(uriParagraph);
                var paragraph1 = new Paragraph();
                paragraph.Add(paragraph1);
                
                var pdfTable = new PdfPTable(4);
                pdfTable.DefaultCell.Padding = 3;
                pdfTable.WidthPercentage = 100; // percentage
                pdfTable.DefaultCell.BorderWidth = 2;
                pdfTable.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfTable.HeaderRows = 1;  // this is the end of the table header

                pdfTable.DefaultCell.BorderWidth = 1;


                                
                var headerCellFont = new Font(defaultFont, 10,Font.BOLD);

                paragraph1.Add(pdfTable);
                pdfTable.AddCell(new Phrase("Name",headerCellFont));
                pdfTable.AddCell(new Phrase("Type",headerCellFont));
                pdfTable.AddCell(new Phrase("Description",headerCellFont));
                pdfTable.AddCell(new Phrase("Annotation", headerCellFont));

                int i = 0;
                var cellFont = new Font(defaultFont, 10);
                    
                var altRow = new BaseColor(242, 242, 242);
                foreach (var parameter in d.UriParameters)
                {
                    i++;
                    if (i % 2 == 1)
                        pdfTable.DefaultCell.BackgroundColor = altRow;

                    pdfTable.AddCell(new Phrase(parameter.Name, cellFont));
                    pdfTable.AddCell(new Phrase(parameter.TypeDocumentation ?? parameter.TypeName, cellFont));
                    pdfTable.AddCell(new Phrase(parameter.Documentation, cellFont));
                    pdfTable.AddCell(new Phrase(string.Join(Environment.NewLine, parameter.Annotations), cellFont));
                    if (i % 2 == 1)
                        pdfTable.DefaultCell.BackgroundColor = BaseColor.WHITE;
                }


                return paragraph;
            }
            return null;
        }
    }
}