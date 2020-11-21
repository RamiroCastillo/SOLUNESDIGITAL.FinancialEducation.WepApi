using System;
using System.IO;

namespace SOLUNESDIGITAL.Prosecution.Image
{
    public class Pdf
    {
        public static string GeneredPdfByImage(string base64Image)
        {
            var memoryStream = new MemoryStream(System.Convert.FromBase64String(base64Image));
            var img = System.Drawing.Image.FromStream(memoryStream);

            return "";
        }
    }
}
