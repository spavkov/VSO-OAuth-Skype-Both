using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BotApplication1.Services
{
    public interface IUserProfileService
    {
        Task<UserVSOProfile> GetUserProfile(string userId);
        Task<string> GetUserAccounts(string userId);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly IUserOauthTokenService _tokenService;

        public UserProfileService(IUserOauthTokenService tokenService)
        {
            _tokenService = tokenService;
        }


        public async Task<string> GetUserAccounts(string userId)
        {
            var token = _tokenService.GetUserToken(userId);
            if (token == null)
            {
                return null;
            }

            var profile = await this.GetUserProfile(userId);
            if (profile == null)
            {
                return null;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetStringAsync(new Uri(string.Format("https://app.vssps.visualstudio.com/_apis/Accounts?memberId={0}&api-version=1.0", profile.VsoMemberId)));

                    var json = JObject.Parse(response);

                    string count = json["count"].Value<string>();
                    return string.Format("User {0} has {1} accounts", profile.DisplayName, count);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<UserVSOProfile> GetUserProfile(string userId)
        {
            var token = _tokenService.GetUserToken(userId);
            if (token == null)
            {
                return null;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetStringAsync(new Uri("https://app.vssps.visualstudio.com/_apis/profile/profiles/me?api-version=1.0"));

                    var json = JObject.Parse(response);

                    string name = json["displayName"].Value<string>();
                    string id = json["id"].Value<string>();

                    if (name == null || id == null)
                    {
                        return null;
                    }

                    return new UserVSOProfile(userId, name, id);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}