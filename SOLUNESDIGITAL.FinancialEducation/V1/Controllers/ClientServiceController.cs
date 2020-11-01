using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using SOLUNESDIGITAL.Connector.Token.Mangers;
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


        public ClientServiceController(IConfiguration configuration, ILogger logger, IUser user, IUserPolicy userPolicy, IClient client, IConsumptionHistory consumptionHistory, ITokenManger tokenManager)
        {
            _configuration = configuration;
            _logger = logger;
            _user = user;
            _userPolicy = userPolicy;
            _client = client;
            _consumptionHistory = consumptionHistory;
            _tokenManager = tokenManager;
            _timeExpirationTokenRefresh = configuration.GetValue<double>("TimeExpirationTokenRefresh");
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

                AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                correlationId = Request.Headers["Correlation-Id"].ToString();

                Core.Entity.User user = new Core.Entity.User()
                {
                    Public = registrationCompleteRequest.PublicToken,
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
                    Id = registrationCompleteRequest.Id,
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
                    
                };

                //var clientCreateAcountInsert = _client.Update(client);
                /*if (clientCreateAcountInsert.Data == null)
                {
                    response.Data = null;
                    response.Message = clientCreateAcountInsert.Message;
                    response.State = clientCreateAcountInsert.State;
                    return BadRequest(response);
                }*/
                response.Data = new RegistrationCompleteResponse
                {
                    IdCliente = 1,
                    RegistrationComplete = true,
                    NameComplete = "Josue Gutierrez Quino"

                };
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
                //var clientCreateAcountInsert = _client.Update(client);
                /*if (clientCreateAcountInsert.Data == null)
                {
                    response.Data = null;
                    response.Message = clientCreateAcountInsert.Message;
                    response.State = clientCreateAcountInsert.State;
                    return BadRequest(response);
                }*/
                List<QuestionAswerResponse.Question.Answer> answersQ1 = new List<QuestionAswerResponse.Question.Answer>();
                answersQ1.Add(new QuestionAswerResponse.Question.Answer()
                {
                    IdAnswer = 1,
                    DetailAnswer = "Correcto",
                    State = true
                });
                answersQ1.Add(new QuestionAswerResponse.Question.Answer()
                {
                    IdAnswer = 2,
                    DetailAnswer = "Incorrecto",
                    State = false
                });

                List<QuestionAswerResponse.Question> questions = new List<QuestionAswerResponse.Question>();
                questions.Add(new QuestionAswerResponse.Question()
                {
                    IdQuestion = 1,
                    QuestionEvalute = "Indica si el siguiente enunciado en correcto o incorrecto:",
                    QuestionDetail = "El Sistema Financiero es aquel conjunto de instituciones, mercados y medios de un país determinado cuyo objetivo y finalidad principal es la de canalizar el ahorro que generan los prestamistas hacia los prestatarios.",
                    Answers = answersQ1
                });
                List<QuestionAswerResponse.Question.Answer> answersQ2 = new List<QuestionAswerResponse.Question.Answer>();
                answersQ2.Add(new QuestionAswerResponse.Question.Answer()
                {
                    IdAnswer = 1,
                    DetailAnswer = "Asociación de Supervisión del Sistema Financiero",
                    State = true
                });
                answersQ2.Add(new QuestionAswerResponse.Question.Answer()
                {
                    IdAnswer = 2,
                    DetailAnswer = "Autoridad de Supervisión de Finanzas Institucionales",
                    State = false
                });
                answersQ2.Add(new QuestionAswerResponse.Question.Answer()
                {
                    IdAnswer = 3,
                    DetailAnswer = "Autoridad de Supervisión del Sistema Financiero",
                    State = false
                });

                questions.Add(new QuestionAswerResponse.Question()
                {
                    IdQuestion = 2,
                    QuestionEvalute = "El significado de las siglas “ASFI” corresponden a:",
                    QuestionDetail = "",
                    Answers = answersQ2
                });

                QuestionAswerResponse questionAswerResponse = new QuestionAswerResponse()
                {
                    Questions = questions
                };
                response.Data = questionAswerResponse;
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
                /*DateTime dateResponse = DateTime.Now;
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
                Logger.Debug("Request: {0} Response: {1}", questionAswerRequest, response);*/
            }
        }

        [Route("AnswerQuestion")]
        [HttpPost, Authorize]
        public IActionResult AnswerQuestion([FromBody] AnswersRequest questionAswerRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(questionAswerRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<AnswersResponse>();
            string correlationId = string.Empty;
            try
            {
                //var clientCreateAcountInsert = _client.Update(client);
                /*if (clientCreateAcountInsert.Data == null)
                {
                    response.Data = null;
                    response.Message = clientCreateAcountInsert.Message;
                    response.State = clientCreateAcountInsert.State;
                    return BadRequest(response);
                }*/

                AnswersResponse questionAswerResponse = new AnswersResponse()
                {
                    Id = 1,
                    IdClient = 12,
                    IdQuestion = 20
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
                /*DateTime dateResponse = DateTime.Now;
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
                Logger.Debug("Request: {0} Response: {1}", questionAswerRequest, response);*/
            }
        }

        [AllowAnonymous]
        [Route("Login")]
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(loginRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<LoginResponse>();
            string correlationId = string.Empty;
            try
            {
                //var clientCreateAcountInsert = _client.Update(client);
                /*if (clientCreateAcountInsert.Data == null)
                {
                    response.Data = null;
                    response.Message = clientCreateAcountInsert.Message;
                    response.State = clientCreateAcountInsert.State;
                    return BadRequest(response);
                }*/
                //var client = _client.ValidateCliente(loginRequest.Email, loginRequest.Password);

                if(loginRequest.Email == "Ramiro" && loginRequest.Password == "123") 
                {
                    var validate = Models.Response.Error("NotUnauthorized");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return Unauthorized(response);
                }

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Iat,Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, "Ramiro Castillo"),
                    new Claim(ClaimTypes.NameIdentifier,"1"),
                    new Claim(ClaimTypes.Email,"castilloramiro313@gmail.com"),
                };
                var accessToken = _tokenManager.GenerateAccessToken(claims);
                var refreshToken = _tokenManager.GenerateRefreshToken();

                var refreshTokenExpiryTime = DateTime.Now.AddDays(_timeExpirationTokenRefresh);
                //_client.updateToken(RefreshTokenExpiryTime, refreshToken);
                //user.RefreshToken = refreshToken;
                //user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

                LoginResponse loginResponse = new LoginResponse()
                {
                    Token = accessToken,
                    TokenFrefresh = refreshToken
                };
                response.Data = loginResponse;
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
                /*DateTime dateResponse = DateTime.Now;
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
                Logger.Debug("Request: {0} Response: {1}", questionAswerRequest, response);*/
            }
        }

    }
}
