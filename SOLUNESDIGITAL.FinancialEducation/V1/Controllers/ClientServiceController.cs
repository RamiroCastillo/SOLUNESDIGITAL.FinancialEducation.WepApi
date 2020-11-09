using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using BC = BCrypt.Net.BCrypt;
using SOLUNESDIGITAL.Connector.Token.Mangers;
using SOLUNESDIGITAL.FinancialEducation.DataAccess.V1;
using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests;
using SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses;
using SOLUNESDIGITAL.Framework.Logs;
using Microsoft.AspNetCore.Http;

namespace SOLUNESDIGITAL.FinancialEducation.V1.Controllers
{
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ClientServiceController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IUser _user;
        private readonly IUserPolicy _userPolicy;
        private readonly IClient _client;
        private readonly IConsumptionHistory _consumptionHistory;
        private readonly ITokenManger _tokenManager;
        private readonly double _timeExpirationTokenRefresh;
        private readonly IRefreshToken _refreshToken;
        private readonly IModule _module;
        private readonly double _scoreByQuestion;
        private readonly IClienAnswer _clientAnswer;

        public ClientServiceController(IConfiguration configuration, ILogger logger, IUser user, IUserPolicy userPolicy, IClient client, IConsumptionHistory consumptionHistory, ITokenManger tokenManager, IRefreshToken refreshToken, IModule module, IClienAnswer clientAnswer)
        {
            _configuration = configuration;
            _logger = logger;
            _user = user;
            _userPolicy = userPolicy;
            _client = client;
            _consumptionHistory = consumptionHistory;
            _tokenManager = tokenManager;
            _timeExpirationTokenRefresh = configuration.GetValue<double>("JwtSettings:TimeExpirationTokenRefresh");
            _refreshToken = refreshToken;
            _module = module;
            _scoreByQuestion = configuration.GetValue<double>("DetailScore:Score") / ((configuration.GetValue<double>("DetailScore:Modules")) + 1);
            _clientAnswer = clientAnswer;
        }

        [Route("RegistrationComplete")]
        [HttpPost, Authorize]
        public IActionResult RegistrationComplete([FromBody] RegistrationCompleteRequest registrationCompleteRequest)
        {
                Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(registrationCompleteRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<RegistrationCompleteResponse>();
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

                //AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                //var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                correlationId = Request.Headers["Correlation-Id"].ToString();

                Core.Entity.User user = new Core.Entity.User()
                {
                    Public = registrationCompleteRequest.PublicToken,
                    UserName = registrationCompleteRequest.UserAplication,
                    Password = registrationCompleteRequest.PasswordAplication
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
                    AppUserId = registrationCompleteRequest.AppUserId,
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
                if (registrationCompleteRequest.Age <= 18 && (DateTime.Now.Year - registrationCompleteRequest.Birthdate.Year) <= 18)
                {
                    var validate = Models.Response.Error("UserNotAgeApropiate");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return BadRequest(response);

                }
                Core.Entity.Client client = new Core.Entity.Client()
                {
                    Email = registrationCompleteRequest.Email,
                    NameComplete = registrationCompleteRequest.NameComplete,
                    Gender = registrationCompleteRequest.Gender,
                    Birthdate = registrationCompleteRequest.Birthdate,
                    Age = registrationCompleteRequest.Age,
                    Department = registrationCompleteRequest.Department,
                    City = registrationCompleteRequest.City,
                    Address = registrationCompleteRequest.Address,
                    CellPhone = registrationCompleteRequest.CellPhone,
                    Phone = registrationCompleteRequest.Phone,
                    EducationLevel = registrationCompleteRequest.EducationLevel,
                    Disability = registrationCompleteRequest.Disability,
                    ReferenceName = registrationCompleteRequest.ReferenceName,
                    ReferencePhone = registrationCompleteRequest.ReferencePhone,
                    ModificationUser = registrationCompleteRequest.AppUserId
                    
                };

                var clientCreateAcountInsert = _client.UpdateRegistrationComplete(client,registrationCompleteRequest.AppUserId);
                if (!(Convert.ToInt32(clientCreateAcountInsert.Data) > 0))
                {
                    response.Data = null;
                    response.Message = clientCreateAcountInsert.Message;
                    response.State = clientCreateAcountInsert.State;
                    return BadRequest(response);
                }

                response.Data = new RegistrationCompleteResponse
                {
                    Email = registrationCompleteRequest.Email,
                    RegistrationComplete = true,
                    NameComplete = registrationCompleteRequest.NameComplete

                };
                response.Message = Models.Response.CommentMenssage("RegistrationCompleted");
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
                    AppUserId = registrationCompleteRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(registrationCompleteRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", registrationCompleteRequest, response);
            }
        }

        [Route("QuestionsAndAswers")]
        [HttpPost, Authorize]
        public IActionResult GetQuestionsAswer([FromBody] QuestionAswerRequest questionAswerRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(questionAswerRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<QuestionAswerResponse>();
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

                //AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                //var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                correlationId = Request.Headers["Correlation-Id"].ToString();

                Core.Entity.User user = new Core.Entity.User()
                {
                    Public = questionAswerRequest.PublicToken,
                    UserName = questionAswerRequest.UserAplication,
                    Password = questionAswerRequest.PasswordAplication
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
                    AppUserId = questionAswerRequest.AppUserId,
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

                var answerAndQuestion = _module.GetAnswerAndQuestions(questionAswerRequest.IdModule);
                if (answerAndQuestion.Data == null)
                {
                    var validate = Models.Response.Error("NotAnswerQuestion");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Unauthorized(response);
                }
                var answerAndQuestionResponse = (QuestionAswerResponse)answerAndQuestion.Data;
                foreach (QuestionAswerResponse.Question question in answerAndQuestionResponse.Questions) 
                {
                    foreach (string answer in question.AnswerWithoutProcess.Split("@")) 
                    {
                        var answerResponse = answer.Split(":");
                        question.Answers.Add(new QuestionAswerResponse.Question.Answer 
                        {
                            IdAnswer = Convert.ToInt64(answerResponse[0]),
                            DetailAnswer = answerResponse[1].ToString(),
                            State = answerResponse[2].Equals("1") ? true : false
                        });
                    }
                }
                response.Data = answerAndQuestionResponse;
                response.Message = Models.Response.CommentMenssage("Completed");
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
                    AppUserId = "token",
                    Request = Framework.Common.SerializeJson.ToObject(questionAswerRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", questionAswerRequest, response);
            }
        }

        [Route("AnswerQuestion")]
        [HttpPost, Authorize]
        public IActionResult AnswerQuestion([FromBody] AnswersRequest answersRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(answersRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<AnswersResponse>();
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

                //AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                //var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                correlationId = Request.Headers["Correlation-Id"].ToString();

                Core.Entity.User user = new Core.Entity.User()
                {
                    Public = answersRequest.PublicToken,
                    UserName = answersRequest.UserAplication,
                    Password = answersRequest.PasswordAplication
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
                    AppUserId = answersRequest.AppUserId,
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
                var questionscore = _scoreByQuestion / answersRequest.Answers.Count;
                int count = 0;
                foreach (AnswersRequest.Answer answer in answersRequest.Answers) 
                {
                    var clienteAnswerRegistred = _clientAnswer.InsertClientAnswerValidate(answersRequest.Email,answer.IdAnswer,answer.IdQuestion,answer.State, questionscore, answersRequest.AppUserId);
                    count += Convert.ToInt32(clienteAnswerRegistred.Data);
                }
                if (! (count > 0) )
                {
                    var validate = Models.Response.Error(null, "sql");

                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return BadRequest(response);
                }

                AnswersResponse questionAswerResponse = new AnswersResponse()
                {
                    Email = answersRequest.Email
                };
                response.Data = questionAswerResponse;
                response.Message = Models.Response.CommentMenssage("AnswerRegistred");
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
                    AppUserId = "token",
                    Request = Framework.Common.SerializeJson.ToObject(answersRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", answersRequest, response);
            }
        }

        [AllowAnonymous]
        [Route("Authenticate")]
        [HttpPost]
        public IActionResult Login([FromBody] AuthenticateRequest authenticateRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(authenticateRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<AuthenticateResponse>();
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
                    Public = authenticateRequest.PublicToken,
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
                    AppUserId = authenticateRequest.AppUserId,
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
                var clientValidated = _client.GetClientValitated(authenticateRequest.Email, authenticateRequest.Password);
                if (clientValidated.Data == null)
                {
                    response.Data = null;
                    response.Message = clientValidated.Message;
                    response.State = clientValidated.State;
                    return BadRequest(response);
                }

                var clientLogin = (Core.Entity.Client)clientValidated.Data;
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Iat,Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, clientLogin.NameComplete),
                    new Claim(ClaimTypes.Email,clientLogin.Email),
                };
                var accessToken = _tokenManager.GenerateAccessToken(claims);

                string ipAddress = "";
                if (Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    ipAddress = Request.Headers["X-Forwarded-For"];
                }
                else 
                {
                    ipAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                }
                var refreshToken = _tokenManager.GenerateRefreshToken(ipAddress, _timeExpirationTokenRefresh);

                refreshToken.CreationUser = authenticateRequest.AppUserId;
                refreshToken.EmailClient = authenticateRequest.Email;

                var refreshTokenInsertClient = _refreshToken.Insert(refreshToken);

                if (refreshTokenInsertClient.Data == null) 
                {
                    response.Data = null;
                    response.State = refreshTokenInsertClient.State;
                    response.Message = refreshTokenInsertClient.Message;

                    return BadRequest(response);
                }

                var refreshTokenRemove = _refreshToken.RemoveOldRefreshTokens(authenticateRequest.Email, _configuration.GetValue<int>("JwtSettings:RefreshTokenTTL"));
                if (refreshTokenRemove.Data == null)
                {
                    response.Data = null;
                    response.State = refreshTokenRemove.State;
                    response.Message = refreshTokenRemove.Message;

                    return BadRequest(response);
                }

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(_configuration.GetValue<double>("JwtSettings:TimeExpirationTokenRefresh"))
                };
                Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

                var clientValidatedData = (Core.Entity.Client)clientValidated.Data;
                response.Data = new AuthenticateResponse()
                {
                    Email = clientValidatedData.Email,
                    Role = clientValidatedData.Role == Core.Entity.Role.Admin ? "Admin" : "User",
                    Verify = clientValidatedData.IsVerified,
                    RegistredCompleted = clientValidatedData.CompleteRegister,
                    Token = accessToken,
                    RefreshToken = refreshToken.Token
                };
                response.Message = Models.Response.CommentMenssage("LoginSuccessful");
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
                    AppUserId = "token",
                    Request = Framework.Common.SerializeJson.ToObject(authenticateRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", authenticateRequest, response);
            }
        }

    }
}
