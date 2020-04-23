using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;

namespace VkBot
{
    public class Server
    {
        private readonly string version = "5.103";
        public VkApi vkClient;
        public WebClient webClient;
        private readonly string groupId;
        private readonly string apiKey;
        private VkParameters param;
        dynamic longPoll; 

        public Server(string apiKey, string groupId/*, string pathToSaveFiles, string pathToAdditionalFiles*/)
        {
            this.groupId = groupId;
            this.apiKey = apiKey;
            vkClient = new VkApi();
            webClient = new WebClient();
        }

        public void Authorize()
        {
            vkClient.Authorize(new ApiAuthParams{ AccessToken = apiKey, Settings = Settings.All});
            param = new VkParameters(new Dictionary<string, string> { { "group_id", groupId } });
            longPoll = JObject.Parse(vkClient.Call("groups.getLongPollServer", param).RawJson);
        }

        public void LogOut()
        {
            vkClient.LogOut();
        }

        public JObject Request(string comm, Dictionary<string, string> configs)
        {
            var tempStr = new StringBuilder();
            foreach (var key in configs.Keys) 
                tempStr.Append($"{key}={configs[key]}&");
            var temp = webClient.DownloadString(
                $"https://api.vk.com/method/{comm}?{tempStr}access_token={apiKey}&v={version}");
            return JObject.Parse(temp);
        }

        public void SendMessage(string userId, string message, string randomId)
        {
            Request("messages.send", new Dictionary<string, string>
            {
                ["user_id"] = userId,
                ["message"] = message,
                ["group_id"] = groupId,
                ["random_id"] = randomId
            });
        }
        
        public string GetJsonAnswer(string json)
        {
            string url = string.Format("{0}?act=a_check&key={1}&ts={2}&wait=3",
                longPoll.response.server.ToString(),
                longPoll.response.key.ToString(),
                json != string.Empty ? JObject.Parse(json)["ts"].ToString() : longPoll.response.ts.ToString()
            );

            json = webClient.DownloadString(url);
            return json;
        }
    }
}