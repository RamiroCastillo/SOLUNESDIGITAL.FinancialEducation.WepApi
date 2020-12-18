using SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SOLUNESDIGITAL.Tools.Images
{
    public class ToolImage
    {
        public static string GetBase64Image(List<SendCertificateRequest.CertificateParameter> listParameters)
        {
            PdfStringFormat stringFormatText = new PdfStringFormat();
            PdfDocument document = new PdfDocument();
            try
            {
                Image bitmap = Image.FromFile(string.Format(@"{0}/Resources/certificado.png", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));

                stringFormatText.Alignment = PdfTextAlignment.Center;
                stringFormatText.LineAlignment = (PdfVerticalAlignment)PdfTextAlignment.Center;

                var memoryStreamImage = new MemoryStream();
                bitmap.Save(memoryStreamImage, System.Drawing.Imaging.ImageFormat.Jpeg);

                document.PageSettings.Orientation = PdfPageOrientation.Landscape;
                document.PageSettings.Size = PdfPageSize.Letter;
                document.PageSettings.Margins.All = 0;
                PdfImage image = PdfImage.FromStream(memoryStreamImage);
                PdfPage page = document.Pages.Add();
                page.Graphics.DrawImage(image, 0, 0, document.PageSettings.Width, document.PageSettings.Height);

                float spaceTotalIncrement = listParameters.First().FontSize;
                bool firstLabel = true;
                float fontSizeSeparate = 25;
                foreach (var item in listParameters)
                {
                    if (firstLabel)
                    {
                        if ((!string.IsNullOrEmpty(item.Label)) && item.FontSize > 0)
                        {
                            PdfFont fontLabel = new PdfStandardFont(PdfFontFamily.Helvetica, item.FontSizeLabel);
                            page.Graphics.DrawString(item.Label, fontLabel, PdfBrushes.Black, item.HorizontalTextDirection, (item.VerticalTextDirection - ( fontSizeSeparate * 2) ), stringFormatText);
                        }
                        firstLabel = false;
                    }
                    else
                    {
                        if ((!string.IsNullOrEmpty(item.Label)) && item.FontSize > 0)
                        {
                            PdfFont fontLabel = new PdfStandardFont(PdfFontFamily.Helvetica, item.FontSizeLabel);
                            page.Graphics.DrawString(item.Label, fontLabel, PdfBrushes.Black, item.HorizontalTextDirection, (item.VerticalTextDirection - fontSizeSeparate), stringFormatText);
                        }
                    }
                    PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, item.FontSize);
                    page.Graphics.DrawString(item.Value, font, PdfBrushes.Red, item.HorizontalTextDirection, item.VerticalTextDirection, stringFormatText);
                }
                memoryStreamImage.Dispose();

                MemoryStream streamPdf = new MemoryStream();
                document.Save(streamPdf);

                byte[] docBytes = streamPdf.ToArray();
                streamPdf.Position = 0;
                var base64 = "data:application/pdf;base64," + System.Convert.ToBase64String(docBytes);
                document.Close();
                return base64;
            }
            catch (Exception ex)
            {
                document.Close();
                throw new Exception(ex.Message);
            }
        }
    }
}
