using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Connector.Email.Managers
{
    public interface IEmailManager
    {
        public Response SendEmail(string to, string subject, string withOriginMessage, string withoutOriginMessage, string header, string origin, string link, string token = "",string numberModule = "",string coupon = "",string contestDate = "");
    } 

    public class EmailManager : IEmailManager
    {
        private readonly string _from;
        private readonly string _host;
        private readonly string _port;
        private readonly string _user;
        private readonly string _password;
        private readonly string _message;
        private readonly bool _flagEnableUserPassword;        
        SmtpClient _client;


        public EmailManager(string host, string port, string from, string user, string password,bool flagEnableUserPassword,string message)
        {
            _host = host;
            _port = port;
            _from = from;
            _user = user;
            _password = password;
            _flagEnableUserPassword = flagEnableUserPassword;
            _message = message;
            _client = new SmtpClient(_host, Convert.ToInt32(_port));
        }


        public Response SendEmail(string to, string subject,string withOriginMessage, string withoutOriginMessage,string header,string origin, string link, string token = "",string numberModule = "",string coupon = "", string contestDate = "")
        {
            Response response = new Response();
            string finalMessage = "";
            if (!Validate.ToEmail(to)) 
            {
                var validate = Response.Error(to, "InvalidEmail");
                response.Data = null;
                response.Message = validate.Message;
                response.State = validate.State;

                return response;
            }

            MailAddress _fromMailAddress = new MailAddress(_from);

            MailMessage message = new MailMessage();
            if (_flagEnableUserPassword)
            {
                var basicCredential = new NetworkCredential(_user, _password);
                _client.UseDefaultCredentials = false;
                _client.EnableSsl = true;
                _client.Credentials = basicCredential;
                _client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            }
            if (!string.IsNullOrEmpty(origin))
            {
                finalMessage = _message.Replace("@message", withOriginMessage);
            }
            else 
            {
                finalMessage = _message.Replace("@message", withoutOriginMessage);
            }
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.From = _fromMailAddress;
            var bodyMessage = "";
            foreach (var item in to.Split(';'))
            {

                if (!string.IsNullOrEmpty(item))
                {
                    message.To.Add(item);
                    bodyMessage = finalMessage.Replace("@email", item).Replace("@link", link).Replace("@Bussines", _from).Replace("@header", header).Replace("@token", token).Replace("@numeroModulo",numberModule).Replace("@cupon",coupon).Replace("@fecha",contestDate);
                    message.Body = bodyMessage;
                    /*AlternateView view = AlternateView.CreateAlternateViewFromString(bodyMessage, null, MediaTypeNames.Text.Html);

                    LinkedResource resource = new LinkedResource(string.Format(@"{0}/Resources/MailImages/header-bg.png", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)),"image/png");
                    resource.ContentId = "id1";
                    resource.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    view.LinkedResources.Add(resource);

                    LinkedResource resource2 = new LinkedResource(string.Format(@"{0}/Resources/MailImages/module-completed.png", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), "image/png");
                    resource2.ContentId = "id2";
                    resource2.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    view.LinkedResources.Add(resource2);

                    LinkedResource resource3 = new LinkedResource(string.Format(@"{0}/Resources/MailImages/bg-mail.png", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), "image/png");
                    resource3.ContentId = "id3";
                    resource3.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    view.LinkedResources.Add(resource3);

                    LinkedResource resource4 = new LinkedResource(string.Format(@"{0}/Resources/MailImages/click-logo.png", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), "image/png");
                    resource4.ContentId = "id4";
                    resource4.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    view.LinkedResources.Add(resource4);

                    LinkedResource resource5 = new LinkedResource(string.Format(@"{0}/Resources/MailImages/ecofuturo-logo.png", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), "image/png");
                    resource5.ContentId = "id5";
                    resource5.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    view.LinkedResources.Add(resource5);

                    LinkedResource resource6 = new LinkedResource(string.Format(@"{0}/Resources/MailImages/bg-ticket.png", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), "image/png");
                    resource6.ContentId = "id6";
                    resource6.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    view.LinkedResources.Add(resource6);

                    message.AlternateViews.Add(view);
                    bodyMessage = "";*/

                }
            }
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            try
            {
                _client.Send(message);
                return Response.Success(true);
            }
            catch (Exception ex)
            {
                Logger.Error("Message: {0}; Exception: {1}", ex.Message, SerializeJson.ToObject(ex));
                return Response.Error(to, "InvalidEmail");
            }
        }
    }
}
