using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
    public class TokenController : ControllerBase
    {
        private readonly ITokenManger _tokenManager;
        private readonly IClient _client;
        private readonly ILogger _logger;
        private readonly IConsumptionHistory _consumptionHistory;
        private readonly IConfiguration _configuration;
        private readonly IUser _user;
        private readonly IUserPolicy _userPolicy;
        private readonly IRefreshToken _refreshToken;

        public TokenController(IConfiguration configuration, ILogger logger, IUser user, IUserPolicy userPolicy, IClient client, IConsumptionHistory consumptionHistory, ITokenManger tokenManager, IRefreshToken refreshToken)
        {
            _configuration = configuration;
            _logger = logger;
            _user = user;
            _userPolicy = userPolicy;
            _client = client;
            _consumptionHistory = consumptionHistory;
            _tokenManager = tokenManager;
            _refreshToken = refreshToken;
        }

        [AllowAnonymous]
        [Route("RefreshToken")]
        [HttpPost]
        public IActionResult RefreshToken([FromBody] TokenRefreshRequest tokenRefreshRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(tokenRefreshRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<TokenRefreshResponse>();
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
                    Public = tokenRefreshRequest.PublicToken,
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
                    AppUserId = tokenRefreshRequest.AppUserId,
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

                var refreshToken = Request.Cookies["refreshToken"];
                string ipAddress = "";
                if (Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    ipAddress = Request.Headers["X-Forwarded-For"];
                }
                else
                {
                    ipAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                }
                var newRefreshToken = _tokenManager.GenerateRefreshToken(ipAddress, _configuration.GetValue<double>("JwtSettings:TimeExpirationTokenRefresh"));

                var refreshTokenUpdate = _refreshToken.RefreshTokenUpdate(tokenRefreshRequest.Email,ipAddress, refreshToken, newRefreshToken.Token);
                if (refreshTokenUpdate.Data == null)
                {
                    response.Data = null;
                    response.State = refreshTokenUpdate.State;
                    response.Message = refreshTokenUpdate.Message;

                    return BadRequest(response);
                }
                var clientData = (Core.Entity.Client)refreshTokenUpdate.Data;


                newRefreshToken.CreationUser = tokenRefreshRequest.AppUserId;
                newRefreshToken.EmailClient = tokenRefreshRequest.Email;

                var refreshTokenInsertClient = _refreshToken.Insert(newRefreshToken);

                if (refreshTokenInsertClient.Data == null)
                {
                    response.Data = null;
                    response.State = refreshTokenInsertClient.State;
                    response.Message = refreshTokenInsertClient.Message;

                    return BadRequest(response);
                }
                var refreshTokenRemove = _refreshToken.RemoveOldRefreshTokens(tokenRefreshRequest.Email, _configuration.GetValue<int>("JwtSettings:RefreshTokenTTL"));

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Iat,Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, clientData.NameComplete),
                    new Claim(ClaimTypes.Email,clientData.Email),
                };
                var accessToken = _tokenManager.GenerateAccessToken(claims);

                var cookieOptions = new CookieOptions
                {
                    //HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(_configuration.GetValue<double>("JwtSettings:TimeExpirationTokenRefresh")),
                    SameSite = SameSiteMode.Lax,
                    //Secure = true,
                };
                Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

                response.Data = new TokenRefreshResponse
                {
                    Email = clientData.Email,
                    Role = clientData.Role == Core.Entity.Role.Admin ? "Admin" : "User",
                    Verify = clientData.IsVerified,
                    RegistredCompleted = clientData.CompleteRegister,
                    Token = accessToken,
                    RefreshToken = newRefreshToken.Token
                };
                response.Message = Models.Response.CommentMenssage("TokenRefreshSuccessful");
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
                    AppUserId = tokenRefreshRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(tokenRefreshRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", tokenRefreshRequest, response);
            }
        }

        [HttpPost, Authorize]
        [Route("RevokeToken")]
        public IActionResult RevokeToken([FromBody] RevokeRequest revokeRequest)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(revokeRequest));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<RevokeResponse>();
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
                    Public = revokeRequest.PublicToken,
                    UserName = revokeRequest.UserAplication,
                    Password = revokeRequest.PasswordAplication
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
                    AppUserId = revokeRequest.AppUserId,
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

                AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialToken = authHeader.Parameter;
                var responsetokenValidated = _tokenManager.GetPrincipalFromExpiredToken(credentialToken);

                if (responsetokenValidated.Data == null)
                {
                    response.Data = null;
                    response.Message = responsetokenValidated.Message;
                    response.State = responsetokenValidated.State;
                    return BadRequest(response);
                }
                var principal = (ClaimsPrincipal)responsetokenValidated.Data;
                var claimList = principal.Claims.ToList();
                var verifyEmail = claimList[2].Value;

                if (!verifyEmail.Equals(revokeRequest.Email.Trim()))
                {
                    var validate = Models.Response.Error("ClientNotSession");
                    response.Data = null;
                    response.Message = validate.Message;
                    response.State = validate.State;
                    return BadRequest(response);
                }

                var token = revokeRequest.Token ?? Request.Cookies["refreshToken"];

                string ipAddress = "";
                if (Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    ipAddress = Request.Headers["X-Forwarded-For"];
                }
                else
                {
                    ipAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                }
                var revokeToken = _refreshToken.RevokeToken(token,ipAddress);
                if (revokeToken.Data == null)
                {
                    response.Data = null;
                    response.State = revokeToken.State;
                    response.Message = revokeToken.Message;

                    return BadRequest(response);
                }

                response.Data = new RevokeResponse
                {
                    Token = revokeRequest.Token
                };
                response.Message = Models.Response.CommentMenssage("TokenRevokeSuccessful");
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
                    AppUserId = revokeRequest.AppUserId,
                    Request = Framework.Common.SerializeJson.ToObject(revokeRequest),
                    DateRequest = dateRequest,
                    Response = Framework.Common.SerializeJson.ToObject(response),
                    DateResponse = dateResponse,
                    CodeResponse = response.State
                };
                _consumptionHistory.Insert(consumptionHistory);
                Logger.Debug("Request: {0} Response: {1}", revokeRequest, response);
            }
        }
    }
}
