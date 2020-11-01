using System;
using BC = BCrypt.Net.BCrypt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SOLUNESDIGITAL.FinancialEducation.Connector.Email.Managers;
using SOLUNESDIGITAL.FinancialEducation.DataAccess.V1;
using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests;
using SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses;
using SOLUNESDIGITAL.Framework.Logs;

namespace SOLUNESDIGITAL.FinancialEducation.V1.Controllers
{
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AdministrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IUser _user;
        private readonly IUserPolicy _userPolicy;
        private readonly IClient _client;
        private readonly IConsumptionHistory _consumptionHistory;
        private readonly IEmailManager _emailmanager;


        public AdministrationController(IConfiguration configuration, ILogger logger,IUser user, IUserPolicy userPolicy, IClient client, IConsumptionHistory consumptionHistory,IEmailManager emailManager)
        {
            _configuration = configuration;
            _logger = logger;
            _user = user;
            _userPolicy = userPolicy;
            _client = client;
            _consumptionHistory = consumptionHistory;
            _emailmanager = emailManager;
        }

        [AllowAnonymous]
        [Route("PreRegister")]
        [HttpPost]
        public IActionResult PreRegistration([FromBody] PreRegistrationRequest preRegistrationRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(preRegistrationRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<PreRegistrationResponse>();
            string correlationId = string.Empty;
            try
            {
                #region Authorization Usuario y Contraseña
                if (string.IsNullOrEmpty(Request.Headers["Authorization"]))
                {
                    var validate = Models.Response.Error(null, "NotAuthenticated");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Unauthorized(response);
                }

                AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                correlationId = Request.Headers["Correlation-Id"].ToString();

                Core.Entity.User user = new Core.Entity.User()
                {
                    Public = preRegistrationRequest.PublicToken,
                    UserName = credentials[0],
                    Password = credentials[1]
                };
                var userAuthenticate = _user.Authenticate(user);
                if (userAuthenticate.Data == null)
                {
                    var validate = Models.Response.Error("NotAuthenticated");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Unauthorized(response);
                }
                Core.Entity.UserPolicy userPolicy = new Core.Entity.UserPolicy() 
                {
                    AppUserId = preRegistrationRequest.AppUserId,
                    IdUser = ((Core.Entity.User)userAuthenticate.Data).Id
                };
                Core.Entity.Policy policy = new Core.Entity.Policy() 
                {
                    Name = Request.Path.Value
                };
                var userPolicyAuthorize = _userPolicy.Authorize(userPolicy,policy);
                if (userPolicyAuthorize.Data == null)
                {
                    var validate = Models.Response.Error("NotUnauthorized");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Unauthorized(response);
                }
                #endregion

                if (preRegistrationRequest.Password != preRegistrationRequest.ConfirmPassword) 
                {
                    var validate = Models.Response.Error("PasswordNotConfirm");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return BadRequest(response);
                }

                var passwordEncrypted = BC.HashPassword(preRegistrationRequest.Password);
                Core.Entity.Client client = new Core.Entity.Client()
                {
                    Email = preRegistrationRequest.Email,
                    Ci = preRegistrationRequest.Ci,
                    Password = passwordEncrypted,
                    AcceptTerms = preRegistrationRequest.AcceptTerms,
                    VerificationTokenEmail = Framework.Common.Tools.randomTokenString(),
                    CreationUser = preRegistrationRequest.AppUserId
                };

                var clientCreateAcountInsert = _client.InsertIfNotexist(client);
                if (clientCreateAcountInsert.Data == null) 
                {
                    response.Data = null;
                    response.Message = clientCreateAcountInsert.Message;
                    response.State = clientCreateAcountInsert.State;
                    return BadRequest(response);
                }
                var newClient = (Core.Entity.Client)clientCreateAcountInsert.Data;

                if (newClient.VerifyExists) 
                {
                    var withOriginMessageAlreadyRegisteredEmail = _configuration.GetValue<string>("Connectors_Email:WithOriginMessageAlreadyRegisteredEmail");
                    var withoutOriginMessageAlreadyRegisteredEmail = _configuration.GetValue<string>("Connectors_Email:WithoutOriginMessageAlreadyRegisteredEmail");
                    _emailmanager.sendEmail(newClient.Email, "Verificacion de correo", withOriginMessageAlreadyRegisteredEmail, withoutOriginMessageAlreadyRegisteredEmail,"VERIFICACIÓN DE CUENTA YA EXISTENTE",Request.Headers["origin"], _configuration.GetValue<string>("Connectors_Email:link"));
                    var validate = Models.Response.Success(newClient, "AlreadyRegisteredEmail");

                    response.Data = new PreRegistrationResponse
                    {
                        IdCliente = Convert.ToInt64(newClient.Id),
                        Email = newClient.Email,
                        Ci = newClient.Ci

                    };
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Ok(response);

                }
                var linkToken = _configuration.GetValue<string>("Connectors_Email:link") + string.Format("?token={0}",newClient.VerificationTokenEmail);
                var withOriginMessageVerificationEmail = _configuration.GetValue<string>("Connectors_Email:WithOriginMessageVerificationEmail");
                var withoutOriginMessageVerificationEmail = _configuration.GetValue<string>("Connectors_Email:WithoutOriginMessageVerificationEmail");
                _emailmanager.sendEmail(newClient.Email, "Verificacion de Correo",withOriginMessageVerificationEmail, withoutOriginMessageVerificationEmail,"VERIFICACIÓN DE CORREO PARA ACTIVAR SU CUENTA" ,Request.Headers["origin"], linkToken);

                response.Data = new PreRegistrationResponse
                {
                    IdCliente = Convert.ToInt64(newClient.Id),
                    Email = newClient.Email,
                    Ci = newClient.Ci
                    
                };
                response.Message = Models.Response.CommentMenssage("PreRegistredCompleted");
                response.State = "000";
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.Error("Message: {0}; Exception: {1}", ex.Message, Framework.Common.SerializeJson.ToObject(ex));
                response.Data = null;
                response.Message = "Error General";
                response.State = "099";
                return BadRequest(response);
            }
            finally
            {
                DateTime dateResponse = DateTime.Now;
                Core.Entity.ConsumptionHistory consumptionHistory = new Core.Entity.ConsumptionHistory
                {
                    ApiName = Request.Path.Value,
                    Host = Dns.GetHostName() + ":" + Request.Host.Port,
                    CorrelationId = correlationId,
                    AppUserId = preRegistrationRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(preRegistrationRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", preRegistrationRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("VerifyEmail")]
        [HttpPost]
        public IActionResult VerifyEmail([FromBody] PreRegistrationRequest preRegistrationRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(preRegistrationRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<PreRegistrationResponse>();
            string correlationId = string.Empty;
            try
            {
                #region Authorization Usuario y Contraseña
                if (string.IsNullOrEmpty(Request.Headers["Authorization"]))
                {
                    var validate = Models.Response.Error(null, "NotAuthenticated");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Unauthorized(response);
                }

                AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                correlationId = Request.Headers["Correlation-Id"].ToString();

                Core.Entity.User user = new Core.Entity.User()
                {
                    Public = preRegistrationRequest.PublicToken,
                    UserName = credentials[0],
                    Password = credentials[1]
                };
                var userAuthenticate = _user.Authenticate(user);
                if (userAuthenticate.Data == null)
                {
                    var validate = Models.Response.Error("NotAuthenticated");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Unauthorized(response);
                }
                Core.Entity.UserPolicy userPolicy = new Core.Entity.UserPolicy()
                {
                    AppUserId = preRegistrationRequest.AppUserId,
                    IdUser = ((Core.Entity.User)userAuthenticate.Data).Id
                };
                Core.Entity.Policy policy = new Core.Entity.Policy()
                {
                    Name = Request.Path.Value
                };
                var userPolicyAuthorize = _userPolicy.Authorize(userPolicy, policy);
                if (userPolicyAuthorize.Data == null)
                {
                    var validate = Models.Response.Error("NotUnauthorized");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Unauthorized(response);
                }
                #endregion

                if (preRegistrationRequest.Password != preRegistrationRequest.ConfirmPassword)
                {
                    var validate = Models.Response.Error("PasswordNotConfirm");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return BadRequest(response);
                }

                var passwordEncrypted = BC.HashPassword(preRegistrationRequest.Password);
                Core.Entity.Client client = new Core.Entity.Client()
                {
                    Email = preRegistrationRequest.Email,
                    Ci = preRegistrationRequest.Ci,
                    Password = passwordEncrypted,
                    AcceptTerms = preRegistrationRequest.AcceptTerms,
                    VerificationTokenEmail = Framework.Common.Tools.randomTokenString(),
                    CreationUser = preRegistrationRequest.AppUserId
                };

                var clientCreateAcountInsert = _client.InsertIfNotexist(client);
                if (clientCreateAcountInsert.Data == null)
                {
                    response.Data = null;
                    response.Message = clientCreateAcountInsert.Message;
                    response.State = clientCreateAcountInsert.State;
                    return BadRequest(response);
                }
                var newClient = (Core.Entity.Client)clientCreateAcountInsert.Data;

                if (newClient.VerifyExists)
                {
                    var withOriginMessageAlreadyRegisteredEmail = _configuration.GetValue<string>("Connectors_Email:WithOriginMessageAlreadyRegisteredEmail");
                    var withoutOriginMessageAlreadyRegisteredEmail = _configuration.GetValue<string>("Connectors_Email:WithoutOriginMessageAlreadyRegisteredEmail");
                    _emailmanager.sendEmail(newClient.Email, "Verificacion de correo", withOriginMessageAlreadyRegisteredEmail, withoutOriginMessageAlreadyRegisteredEmail, "VERIFICACIÓN DE CUENTA YA EXISTENTE", Request.Headers["origin"], _configuration.GetValue<string>("Connectors_Email:link"));
                    var validate = Models.Response.Success(newClient, "AlreadyRegisteredEmail");

                    response.Data = new PreRegistrationResponse
                    {
                        IdCliente = Convert.ToInt64(newClient.Id),
                        Email = newClient.Email,
                        Ci = newClient.Ci

                    };
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Ok(response);

                }
                var linkToken = _configuration.GetValue<string>("Connectors_Email:link") + string.Format("?token={0}", newClient.VerificationTokenEmail);
                var withOriginMessageVerificationEmail = _configuration.GetValue<string>("Connectors_Email:WithOriginMessageVerificationEmail");
                var withoutOriginMessageVerificationEmail = _configuration.GetValue<string>("Connectors_Email:WithoutOriginMessageVerificationEmail");
                _emailmanager.sendEmail(newClient.Email, "Verificacion de Correo", withOriginMessageVerificationEmail, withoutOriginMessageVerificationEmail, "VERIFICACIÓN DE CORREO PARA ACTIVAR SU CUENTA", Request.Headers["origin"], linkToken);

                response.Data = new PreRegistrationResponse
                {
                    IdCliente = Convert.ToInt64(newClient.Id),
                    Email = newClient.Email,
                    Ci = newClient.Ci

                };
                response.Message = Models.Response.CommentMenssage("PreRegistredCompleted");
                response.State = "000";
                return Ok(response);
            }
            catch (Exception ex)
            {
                Logger.Error("Message: {0}; Exception: {1}", ex.Message, Framework.Common.SerializeJson.ToObject(ex));
                response.Data = null;
                response.Message = "Error General";
                response.State = "099";
                return BadRequest(response);
            }
            finally
            {
                DateTime dateResponse = DateTime.Now;
                Core.Entity.ConsumptionHistory consumptionHistory = new Core.Entity.ConsumptionHistory
                {
                    ApiName = Request.Path.Value,
                    Host = Dns.GetHostName() + ":" + Request.Host.Port,
                    CorrelationId = correlationId,
                    AppUserId = preRegistrationRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(preRegistrationRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", preRegistrationRequest, response);
            }
        }

    }
}
