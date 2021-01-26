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
using SOLUNESDIGITAL.Tools.Images;
using System.IO;
using SOLUNESDIGITAL.FinancialEducation.Core.Entity;

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
        private readonly double _scoreByQuestion;
        private readonly IClienAnswer _clientAnswer;
        private readonly IClientModule _clientModule;


        public AdministrationController(IConfiguration configuration, ILogger logger,IUser user, IUserPolicy userPolicy, IClient client, IConsumptionHistory consumptionHistory,IEmailManager emailManager, IClienAnswer clientAnswer,IClientModule clientModule)
        {
            _configuration = configuration;
            _logger = logger;
            _user = user;
            _userPolicy = userPolicy;
            _client = client;
            _consumptionHistory = consumptionHistory;
            _emailmanager = emailManager;
            _scoreByQuestion = configuration.GetValue<double>("DetailScore:Score") / ((configuration.GetValue<double>("DetailScore:Modules")) + 1);
            _clientAnswer = clientAnswer;
            _clientModule = clientModule;
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

                if (!Framework.Common.Tools.WellWrittenEmail(preRegistrationRequest.Email)) 
                {
                    var validate = Models.Response.Error("InvalidEmail");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return BadRequest(response);
                }

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
                    CiExpedition = preRegistrationRequest.CiExpedition,
                    Password = passwordEncrypted,
                    AcceptTerms = preRegistrationRequest.AcceptTerms,
                    VerificationTokenEmail = Framework.Common.Tools.RandomTokenString(),
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
                    _emailmanager.SendEmail(newClient.Email, "Verificacion de correo", withOriginMessageAlreadyRegisteredEmail, withoutOriginMessageAlreadyRegisteredEmail,"VERIFICACIÓN DE CUENTA YA EXISTENTE",Request.Headers["origin"], _configuration.GetValue<string>("Connectors_Email:linkAlreadyRegisterEmail"));
                    var validate = Models.Response.Success(newClient, "AlreadyRegisteredEmail");

                    response.Data = new PreRegistrationResponse
                    {
                        Email = newClient.Email,
                        Ci = newClient.Ci,
                    };
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Ok(response);

                }
                var linkToken = _configuration.GetValue<string>("Connectors_Email:link") + string.Format("?token={0}",newClient.VerificationTokenEmail);
                var withOriginMessageVerificationEmail = _configuration.GetValue<string>("Connectors_Email:WithOriginMessageVerificationEmail");
                var withoutOriginMessageVerificationEmail = _configuration.GetValue<string>("Connectors_Email:WithoutOriginMessageVerificationEmail");
                _emailmanager.SendEmail(newClient.Email, "Verificacion de Correo",withOriginMessageVerificationEmail, withoutOriginMessageVerificationEmail,"VERIFICACIÓN DE CORREO PARA ACTIVAR SU CUENTA" ,Request.Headers["origin"], linkToken, newClient.VerificationTokenEmail);

                response.Data = new PreRegistrationResponse
                {
                    Email = newClient.Email,
                    Ci = newClient.Ci,
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
        public IActionResult VerifyEmail([FromBody] VerifyEmailRequest verifyEmailRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(verifyEmailRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<VerifyEmailResponse>();
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
                    Public = verifyEmailRequest.PublicToken,
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
                    AppUserId = verifyEmailRequest.AppUserId,
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

                var verifiedEmail = _client.UpdateByVerifyEmmail(verifyEmailRequest.TokenEmailVerify,verifyEmailRequest.AppUserId);
                if (!(Convert.ToInt32(verifiedEmail.Data) > 0))
                {
                    response.Data = null;
                    response.Message = verifiedEmail.Message;
                    response.State = verifiedEmail.State;
                    return BadRequest(response);
                }

                response.Data = new VerifyEmailResponse
                {
                    TokenEmailVerify = verifyEmailRequest.TokenEmailVerify,
                    Verify = true
                };
                response.Message = Models.Response.CommentMenssage("VerifyEmailComplete");
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
                    AppUserId = verifyEmailRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(verifyEmailRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", verifyEmailRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("ForgotPassword")]
        [HttpPost]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(forgotPasswordRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<ForgotPasswordResponse>();
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
                    Public = forgotPasswordRequest.PublicToken,
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
                    AppUserId = forgotPasswordRequest.AppUserId,
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
                var clientUpdateResetToken = _client.UpdateClientForgotPassword(forgotPasswordRequest.Email, Framework.Common.Tools.RandomTokenString());
                if (clientUpdateResetToken.Data == null)
                {
                    response.Data = null;
                    response.Message = clientUpdateResetToken.Message;
                    response.State = clientUpdateResetToken.State;
                    return BadRequest(response);
                }
                var clientResetedToken = (Core.Entity.Client)clientUpdateResetToken.Data;

                var linkToken = _configuration.GetValue<string>("Connectors_Email:linkForgotPassword") + string.Format("?token={0}", clientResetedToken.ResetToken);
                var withOriginMessageVerificationEmail = _configuration.GetValue<string>("Connectors_Email:WithOriginMessagesendPasswordResetEmail");
                var withoutOriginMessageVerificationEmail = _configuration.GetValue<string>("Connectors_Email:WithoutOriginMessagesendPasswordResetEmail");
                _emailmanager.SendEmail(clientResetedToken.Email, "Restablecer Contraseña", withOriginMessageVerificationEmail, withoutOriginMessageVerificationEmail, "VERIFICACIÓN DE CORREO PARA RECUPERAR CONTRASEÑA", Request.Headers["origin"], linkToken, clientResetedToken.ResetToken);

                response.Data = new ForgotPasswordResponse
                {
                    Email = clientResetedToken.Email
                };
                response.Message = Models.Response.CommentMenssage("ForgotPasswordCheck");
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
                    AppUserId = forgotPasswordRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(forgotPasswordRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", forgotPasswordRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("ResetPassword")]
        [HttpPost]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(resetPasswordRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<ForgotPasswordResponse>();
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
                    Public = resetPasswordRequest.PublicToken,
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
                    AppUserId = resetPasswordRequest.AppUserId,
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
                if (resetPasswordRequest.Password != resetPasswordRequest.ConfirmPassword)
                {
                    var validate = Models.Response.Error("PasswordNotConfirm");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return BadRequest(response);
                }
                var passwordEncrypted = BC.HashPassword(resetPasswordRequest.Password);
                var clientChangePassword = _client.UpdateByEmailForChangePassword(resetPasswordRequest.Email, resetPasswordRequest.Token,passwordEncrypted);
                if (clientChangePassword.Data == null)
                {
                    response.Data = null;
                    response.Message = clientChangePassword.Message;
                    response.State = clientChangePassword.State;
                    return BadRequest(response);
                }

                response.Data = new ForgotPasswordResponse
                {
                    Email = resetPasswordRequest.Email
                };
                response.Message = Models.Response.CommentMenssage("PasswordResetSuccessful");
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
                    AppUserId = resetPasswordRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(resetPasswordRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", resetPasswordRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("GetWinners")]
        [HttpPost]
        public IActionResult GetWinners([FromBody] WinnersRequest winnersdRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(winnersdRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<WinnersdResponse>();
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
                    Public = winnersdRequest.PublicToken,
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
                    AppUserId = winnersdRequest.AppUserId,
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

                var winnersResponse = _client.GetWinners();
                if (winnersResponse.Data == null)
                {
                    response.Data = null;
                    response.Message = winnersResponse.Message;
                    response.State = winnersResponse.State;
                    return BadRequest(response);
                }

                response.Data = (WinnersdResponse)winnersResponse.Data;
                response.Message = Models.Response.CommentMenssage("Winners");
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
                    AppUserId = winnersdRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(winnersdRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", winnersdRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("SendCertificate")]
        [HttpPost]
        public IActionResult SendCertificate([FromBody]  SendCertificateRequest sendCertificateRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(sendCertificateRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<SendCertificateResponse>();
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
                    Public = sendCertificateRequest.PublicToken,
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
                    AppUserId = sendCertificateRequest.AppUserId,
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
                var customeModuleEndDate = _clientModule.GetModuleEndDate(sendCertificateRequest.Email);
                if (customeModuleEndDate.Data == null)
                {
                    response.Data = null;
                    response.Message = customeModuleEndDate.Message;
                    response.State = customeModuleEndDate.State;
                    return BadRequest(response);
                }
                var customeModuleEndDateData = (CustomModuleEndDate) customeModuleEndDate.Data;
                var pdf = ToolImage.GetBase64Image(sendCertificateRequest.CertificateParameters, customeModuleEndDateData.Day,customeModuleEndDateData.Month);

                SendCertificateResponse responseImage = new SendCertificateResponse()  
                {
                    pdfCertificate = pdf
                };

                response.Data = responseImage;
                response.Message = Models.Response.CommentMenssage("CertificateCreatedSuccesfuly");
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
                    AppUserId = sendCertificateRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(sendCertificateRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", sendCertificateRequest, response);
            }
        }
    }
}
