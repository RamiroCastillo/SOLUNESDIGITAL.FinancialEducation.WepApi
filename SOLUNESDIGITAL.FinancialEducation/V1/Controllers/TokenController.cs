using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenManger _tokenManager;
        private readonly IClient _client;
        private readonly ILogger _logger;
        private readonly IConsumptionHistory _consumptionHistory;
        private readonly IConfiguration _configuration;


        public TokenController(IConfiguration configuration, ILogger logger, IUser user, IUserPolicy userPolicy, IClient client, IConsumptionHistory consumptionHistory, ITokenManger tokenManager)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
            _consumptionHistory = consumptionHistory;
            _tokenManager = tokenManager;
        }

        [AllowAnonymous]
        [Route("refresh")]
        [HttpPost]

        public IActionResult Refresh([FromBody] TokenRefreshRequest tokenRefresh)
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(tokenRefresh));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<TokenRefreshResponse>();
            string correlationId = string.Empty;

            if (tokenRefresh is null)
            {
                return BadRequest("Invalid client request");
            }
            string accessToken = tokenRefresh.AccessToken;
            string refreshToken = tokenRefresh.RefreshToken;
            var principal = _tokenManager.GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity.Name; //this is mapped to the Name claim by default
            //var client = _client.GetTokenRefreshData(username);
            /*if (client == null || client.RefreshToken != refreshToken || client.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid client request");
            }*/
            var newAccessToken = _tokenManager.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenManager.GenerateRefreshToken();
            //_client.updateTokenRefresh()
            response.Data = new TokenRefreshResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            response.Message = Models.Response.CommentMenssage("Completed");
            response.State = "000";
            return Ok(response);

        }
        /*[HttpPost, Authorize]
        [Route("revoke")]
        public IActionResult Revoke()
        {
            Logger.Debug("Request: {0}", Framework.Common.SerializeJson.ToObject(tokenRefresh));
            DateTime dateRequest = DateTime.Now;
            var response = new IResponse<TokenRefreshResponse>();
            string correlationId = string.Empty;


            var principal = _tokenManager.GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity.Name; //this is mapped to the Name claim by default
            //var client = _client.GetTokenRefreshData(username);
            if (user == null) return BadRequest();
            //_client.removeTokenRefresh(username)
            response.Data = new TokenRefreshResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            response.Message = Models.Response.CommentMenssage("Completed");
            response.State = "000";
            return Ok(response);
        }*/

    }
}
