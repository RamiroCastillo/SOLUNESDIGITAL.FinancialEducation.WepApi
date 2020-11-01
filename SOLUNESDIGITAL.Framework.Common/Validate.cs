using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace SOLUNESDIGITAL.Framework.Common
{
    public class Validate
    {
        public static bool ToEmail(string email)
        {
            try
            {
                var mailCollect = new MailAddressCollection();
                foreach (var item in email.Split(';'))
                {
                    if (!string.IsNullOrEmpty(item))
                        mailCollect.Add(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
