using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf;

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

        public static string GetBase64Image(string nameComplete, string ci, string ciExpedition) 
        {
            Image bitmap = Image.FromFile(string.Format(@"{0}/Resources/certificado.jpg", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));
            Graphics graphicsImage = Graphics.FromImage(bitmap);

            StringFormat stringFormatnameComplete = new StringFormat();
            stringFormatnameComplete.Alignment = StringAlignment.Center;
            stringFormatnameComplete.LineAlignment = StringAlignment.Center;

            StringFormat stringFormatCi = new StringFormat();
            stringFormatCi.Alignment = StringAlignment.Center;
            stringFormatCi.LineAlignment = StringAlignment.Center;

            Color stringColorNameComplete = System.Drawing.ColorTranslator.FromHtml("#000000");
            Color stringColorCi = System.Drawing.ColorTranslator.FromHtml("#000000");

            graphicsImage.DrawString(nameComplete, new Font("arial", 15, FontStyle.Bold), new SolidBrush(stringColorNameComplete), new Point(300, 245), stringFormatnameComplete);

            graphicsImage.DrawString(string.Format("{0} {1}", ci, ciExpedition), new Font("arial", 15, FontStyle.Bold), new SolidBrush(stringColorCi), new Point(300, 350), stringFormatCi);

            string newImage = "";
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            newImage = System.Convert.ToBase64String(ms.ToArray());
            return newImage;

            /*PdfDocument document = new PdfDocument();

            PdfImage image = PdfImage.FromStream(ms);
            PdfPage page = page = document.Pages.Add();
            Syncfusion.Drawing.PointF puntoIncial = new Syncfusion.Drawing.PointF(0,0);

            page.Graphics.DrawImage(image, puntoIncial);
            ms.Dispose();

            MemoryStream stream = new MemoryStream();

            document.Save(stream);
            //Set the position as '0'.
            stream.Position = 0;

            try
            {
                var base64 = "data:application/pdf;base64," + System.Convert.ToBase64String(document);
                document.Close();
                return base64;
            }
            catch (Exception ex)
            {
                document.Close();
                throw new Exception(ex.Message);
            }*/
        }
    }
}
