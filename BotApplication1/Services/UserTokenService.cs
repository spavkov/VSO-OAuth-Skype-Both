using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BotApplication1.Services
{
    public interface IUserOauthTokenService
    {
        bool UserExists(string userId);

        void UpdateUserToken(string userId, string token);

        string GetUserToken(string userId);
    }

    public class UserOauthTokenService : IUserOauthTokenService
    {
        private static readonly ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();

        public bool UserExists(string userId)
        {
            return users.ContainsKey(userId);
        }

        public void UpdateUserToken(string userId, string token)
        {
            var user = new User(userId, token);
            users.AddOrUpdate(userId, user, (key, oldUser) => user);
        }

        public string GetUserToken(string userId)
        {
            User user;
            if (!users.TryGetValue(userId, out user))
            {
                return null;
            }

            return user.OauthToken;
        }
    }

    public class UserVSOProfile
    {
        public UserVSOProfile(string id, string displayName, string vsoMemberId)
        {
            Id = id;
            DisplayName = displayName;
            VsoMemberId = vsoMemberId;
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string VsoMemberId { get; set; }
    }

    public class User
    {
        public User(string id, string oauthToken)
        {
            Id = id;
            OauthToken = oauthToken;
        }

        public string Id { get; set; }
        public string OauthToken { get; set; }
    }
}