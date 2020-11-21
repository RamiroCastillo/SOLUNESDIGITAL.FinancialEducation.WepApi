using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.Drawing;

namespace SOLUNESDIGITAL.Framework.Common
{
    public class Tools
    {
        public static string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[20];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        public static string GetBase64Image(string nameComplete, string evento, MemoryStream memoryStreamCargado)
        {
            string newImage = "";
            PdfStringFormat stringFormatText= new PdfStringFormat();
            PdfDocument document = new PdfDocument();
            MemoryStream stream = new MemoryStream();
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 25);

            try
            {   /*Cargado de imagen*/

                Image imageRequest = Image.FromStream(memoryStreamCargado);

                Image bitmap = Image.FromFile(string.Format(@"{0}/Resources/certificado.jpeg", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));

                stringFormatText.Alignment = PdfTextAlignment.Center;
                stringFormatText.LineAlignment = (PdfVerticalAlignment)PdfTextAlignment.Center;


                var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                newImage = System.Convert.ToBase64String(ms.ToArray());

                document.PageSettings.Orientation = PdfPageOrientation.Landscape;
                document.PageSettings.Size = PdfPageSize.Letter;
                document.PageSettings.Margins.All = 0;
//                PdfImage image = PdfImage.FromStream(ms);
                PdfImage image = PdfImage.FromStream(memoryStreamCargado);
                PdfPage page = document.Pages.Add();
                page.Graphics.DrawImage(image, 0, 0, document.PageSettings.Width, document.PageSettings.Height);

                page.Graphics.DrawString(nameComplete, font, PdfBrushes.Black, (bitmap.Width / 2), (bitmap.Height / 2), stringFormatText);
                page.Graphics.DrawString(evento, font, PdfBrushes.Black, (bitmap.Width / 2), (bitmap.Height / 2) + 60, stringFormatText);
                ms.Dispose();


                document.Save(stream);

                byte[] docBytes = stream.ToArray();
                stream.Position = 0;
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
