using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using BotApplication1.Services;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace BotApplication1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly IUserProfileService _profileService;

        public MessagesController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                
                if (activity.Text == "auth")
                {
                    var userProfile = await _profileService.GetUserProfile(activity.From.Id);
                    if (userProfile != null)
                    {
                        Activity replyToAuth =
                            activity.CreateReply(
                                string.Format(
                                    "User {0} already is granted me access, please invoke VSO command",
                                    userProfile.DisplayName));
                        await connector.Conversations.ReplyToActivityAsync(replyToAuth);
                    }
                    else
                    {
                        Activity replyToAuth =
                            activity.CreateReply(
                                string.Format(
                                    "Please visit this https://vso-oauth.azurewebsites.net/api/auth/?userId={0} to authenticate to VSO",
                                    activity.From.Id));
                        await connector.Conversations.ReplyToActivityAsync(replyToAuth);
                    }
                }
                else if (activity.Text == "accounts")
                {
                    var userProfile = await _profileService.GetUserProfile(activity.From.Id);
                    if (userProfile == null)
                    {
                        Activity replyToAuth =
                            activity.CreateReply(
                                string.Format(
                                    "Please visit this https://vso-oauth.azurewebsites.net/api/auth/?userId={0} to authenticate to VSO",
                                    activity.From.Id));
                        await connector.Conversations.ReplyToActivityAsync(replyToAuth);
                    }
                    else
                    {
                        var accounts = await this._profileService.GetUserAccounts(activity.From.Id);

                        Activity replyToAuth =
                        activity.CreateReply(accounts);
                        await connector.Conversations.ReplyToActivityAsync(replyToAuth);
                    }
                }
                else
                {
                    // calculate something for us to return
                    int length = (activity.Text ?? string.Empty).Length;

                    // return our reply to the user
                    Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}