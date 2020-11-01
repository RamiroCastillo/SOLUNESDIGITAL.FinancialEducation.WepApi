using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Connector.Email.Managers
{
    public interface IEmailManager
    {
        public Response sendEmail(string to, string subject, string withOriginMessage, string withoutOriginMessage, string header, string origin, string link);
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


        public Response sendEmail(string to, string subject,string withOriginMessage, string withoutOriginMessage,string header,string origin, string link)
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
            foreach (var item in to.Split(';'))
            {

                if (!string.IsNullOrEmpty(item))
                {
                    message.To.Add(item);
                    message.Body = finalMessage.Replace("@email", item).Replace("@link", link).Replace("@Bussines", _from).Replace("@header",header);
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
