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
        public static string GetBase64Image(List<SendCertificateRequest.CertificateParameter> listParameters,string moduleCompletionDay, string moduleCompletionMonth)
        {
            PdfStringFormat stringFormatText = new PdfStringFormat();
            PdfDocument document = new PdfDocument();
            try
            {
                Image bitmap = Image.FromFile(string.Format(@"{0}/Resources/Certificado.png", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));

                stringFormatText.Alignment = PdfTextAlignment.Center;
                stringFormatText.LineAlignment = (PdfVerticalAlignment)PdfTextAlignment.Center;

                var memoryStreamImage = new MemoryStream();
                bitmap.Save(memoryStreamImage, System.Drawing.Imaging.ImageFormat.Png);

                document.PageSettings.Orientation = PdfPageOrientation.Landscape;
                document.PageSettings.Size = PdfPageSize.Letter;
                //document.PageSettings.Width = 702;
                //document.PageSettings.Height = 496;
                document.PageSettings.Margins.All = 0;
                PdfImage image = PdfImage.FromStream(memoryStreamImage);
                PdfPage page = document.Pages.Add();
                page.Graphics.DrawImage(image, 0, 0, document.PageSettings.Width, document.PageSettings.Height);

                float spaceTotalIncrement = listParameters.First().FontSize;
                bool firstLabel = true;
                float fontSizeSeparate = 25;

                Stream file = new FileStream(string.Format(@"{0}/Resources/BalooDa2-ExtraBold.ttf", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), FileMode.Open, FileAccess.Read, FileShare.Read);
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
                    PdfFont font = new PdfTrueTypeFont(file, item.FontSize);
                    //PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, item.FontSize);
                    page.Graphics.DrawString(item.Value, font, PdfBrushes.OrangeRed, item.HorizontalTextDirection, item.VerticalTextDirection, stringFormatText);
                }
                Stream fileDate = new FileStream(string.Format(@"{0}/Resources/BalooDa2-Regular.ttf", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), FileMode.Open, FileAccess.Read, FileShare.Read);

                PdfFont fontDate = new PdfTrueTypeFont(file, 11, PdfFontStyle.Regular);

                page.Graphics.DrawString(moduleCompletionDay, fontDate, PdfBrushes.DarkMagenta, 358, 406);
                page.Graphics.DrawString(moduleCompletionMonth, fontDate, PdfBrushes.DarkMagenta, 396, 406);

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
