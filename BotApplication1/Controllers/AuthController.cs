using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using BotApplication1.Model;
using BotApplication1.Services;
using Newtonsoft.Json;

namespace BotApplication1.Controllers
{
    public class AuthController : ApiController
    {
        private readonly IUserOauthTokenService _tokenService;
        private readonly IUserProfileService _profileService;

        public AuthController(IUserOauthTokenService tokenService, IUserProfileService profileService)
        {
            _tokenService = tokenService;
            _profileService = profileService;
        }

        [HttpGet]
        public HttpResponseMessage Index(string userId)
        {
            var response = Request.CreateResponse(HttpStatusCode.Found);
            response.Headers.Location = new Uri(GenerateAuthorizeUrl(userId));
            return response;
        }

        private string GenerateAuthorizeUrl(string userId)
        {
            var uriBuilder = new UriBuilder(ConfigurationManager.AppSettings["OAuth.AuthUrl"]);
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query ?? String.Empty);

            queryParams["client_id"] = ConfigurationManager.AppSettings["OAuth.AppId"];
            queryParams["response_type"] = "Assertion";
            queryParams["state"] = userId;
            queryParams["scope"] = ConfigurationManager.AppSettings["OAuth.Scope"];
            queryParams["redirect_uri"] = ConfigurationManager.AppSettings["OAuth.CallbackUrl"];

            uriBuilder.Query = queryParams.ToString();

            return uriBuilder.ToString();
        }

        private String PerformTokenRequest(String postData, out TokenModel token)
        {
            var error = String.Empty;
            var strResponseData = String.Empty;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(
                ConfigurationManager.AppSettings["OAuth.TokenUrl"]
                );

            webRequest.Method = "POST";
            webRequest.ContentLength = postData.Length;
            webRequest.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter swRequestWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                swRequestWriter.Write(postData);
            }

            try
            {
                HttpWebResponse hwrWebResponse = (HttpWebResponse)webRequest.GetResponse();

                if (hwrWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader srResponseReader = new StreamReader(hwrWebResponse.GetResponseStream()))
                    {
                        strResponseData = srResponseReader.ReadToEnd();
                    }

                    token = JsonConvert.DeserializeObject<TokenModel>(strResponseData);
                    return null;
                }
            }
            catch (WebException wex)
            {
                error = "Request Issue: " + wex.Message;
            }
            catch (Exception ex)
            {
                error = "Issue: " + ex.Message;
            }

            token = new TokenModel();
            return error;
        }

        public string GenerateRequestPostData(string code)
        {
            return string.Format("client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={1}&redirect_uri={2}",
                HttpUtility.UrlEncode(ConfigurationManager.AppSettings["OAuth.AppSecret"]),
                HttpUtility.UrlEncode(code),
                ConfigurationManager.AppSettings["OAuth.CallbackUrl"]
                );
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Callback(string code, string state)
        {
            TokenModel token = new TokenModel();
            String error = null;

            var response = new HttpResponseMessage();

            if (!String.IsNullOrEmpty(code))
            {
                error = PerformTokenRequest(GenerateRequestPostData(code), out token);
                if (String.IsNullOrEmpty(error))
                {
                    this._tokenService.UpdateUserToken(state, token.accessToken);
                    var userName = await _profileService.GetUserProfile(state);

                    response.Content = new StringContent(string.Format(@"<html><body>code:{0} state:{1}   user name: {2}</body></html>", code, state, userName));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                    return response;
                }
            }

            response.Content = new StringContent(string.Format(@"<html><body>error</body></html>"));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}