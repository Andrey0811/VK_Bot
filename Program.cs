using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;

namespace VkBot
{
    class Program
    {
        static void Main(string[] args)
        {
            //var path = Directory.GetCurrentDirectory() + "\\";
            var config = new FileStream(/*path +*/ "C:\\Users\\0811a\\Desktop\\vkBot\\VkBot\\VkBot\\config.txt", FileMode.Open);
            var reader = new StreamReader(config);
            var str = reader.ReadToEnd().Split('\n');
            reader.Close();
            var groupId = "130703143";
            var apiKey = str[0].Split()[1];
            var appId = ulong.Parse(str[1].Split()[1]);
            //var login = str[2].Split()[1];
            //var pass = str[3].Split()[1];
            
            var vkClient = new VkApi();
            var webClient = new WebClient();

            vkClient.Authorize(new ApiAuthParams{ AccessToken = apiKey, Settings = Settings.All});
            /*vkClient.Authorize(new ApiAuthParams
            {
                ApplicationId = appId,
                Login = login, 
                Password = pass,
                Settings = Settings.All
            });*/

            var param = new VkParameters(new Dictionary<string, string> { { "group_id", groupId } });

            var json = string.Empty;

            dynamic longPoll = JObject.Parse(vkClient.Call("groups.getLongPollServer", param).RawJson);
            
            //json = GetJsonAnswer(longPoll, json, webClient);

            while (true)
            {
                json = GetJsonAnswer(longPoll, json, webClient);
                
                Console.WriteLine(json);
                
                //var jsonMsg = json.IndexOf(":[]}") > -1 ? "" : $"{json} \n";
                
                var col = JObject.Parse(json)["updates"].ToList();
                
                foreach (var item in col)
                {
                    if (item["type"].ToString() != "message_new") continue;
                    var urlBotMsg = $"https://api.vk.com/method/messages.send?v=5.41&access_token={apiKey}&user_id=";

                    var msg = item["object"]["message"]["text"].ToString();

                    var arrayData = msg.Split(' ');
                    try
                    {
                        switch (arrayData[0].ToLower())
                        {
                            case "get"://отправлять ссылки думаю что с гугл диск
                                msg = GetComm(arrayData[1]);
                                break;
                            
                            case "deadlines":
                                msg = GetInform("deadlines");
                                break;
                            
                            case "submit"://сделать проферку на пдф
                                var temp = webClient.DownloadString(
                                    $"https://api.vk.com/method/{"users.get"}?{"user_ids"}={item["object"]["message"]["from_id"]}&access_token={apiKey}&v=5.103");
                                var userJson = JObject.Parse(temp);
                                //Console.WriteLine(temp);
                                msg = SubmitComm(arrayData[1].ToLower(), 
                                    item["object"]["message"]["attachments"][0]["doc"]["url"].ToString(), 
                                    item["object"]["message"]["attachments"][0]["doc"]["title"].ToString(),
                                    userJson["response"][0]["last_name"].ToString());
                                break;
                            
                            default:
                                msg = GetInform("help");
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        msg = "Ошибка в команде!\r\n" + GetInform("help");
                    }

                    webClient.DownloadString(
                        string.Format(urlBotMsg + "{0}&message={1}",
                            item["object"]["message"]["from_id"],
                            msg
                             
                        ));
                    Thread.Sleep(1000);
                }
            }
        }

        private static string GetJsonAnswer(dynamic request, string json, WebClient webClient)
        {
            string url = string.Format("{0}?act=a_check&key={1}&ts={2}&wait=3",
                request.response.server.ToString(),
                request.response.key.ToString(),
                json != string.Empty ? JObject.Parse(json)["ts"].ToString() : request.response.ts.ToString()
            );

            json = webClient.DownloadString(url);
            return json;
        }

        private static string GetComm(string topic)
        {
            try
            {
                switch (topic.ToLower())
                {
                    case "matan":
                        return "Успешно";
                    
                    case "algemb":
                        return "Успешно";
                    
                    case "rus":
                        return "Успешно";
                    
                    case "algema":
                        return "Успешно";
                    
                    default:
                        return GetInform("help");
                }
            }
            catch
            {
                return "Не все материалы доставлены";
            }
        }

        private static string SubmitComm(string topic, string url, string title, string name)
        {
            try
            {
                var path = "C:\\Users\\0811a\\Desktop\\";
                var ex = title.Split('.');
                switch (topic)
                {
                    case "matan":
                        DownloadFile(url, path + topic + "\\" + name + "." + ex[1]);
                        return "Успешно";
                    
                    case "algemb":
                        DownloadFile(url, path + topic + "\\" + title);
                        return "Успешно";
                    
                    case "rus":
                        DownloadFile(url, path + topic + "\\" + title);
                        return "Успешно";
                    
                    case "algema":
                        //DownloadFile(url, path + topic);
                        return SentToEmail("ananichev.dima@mail.ru");
                    
                    default:
                        return GetInform("help");
                }
            }
            catch
            {
                return "Файл не доставлен. Попробуй еще раз или напиши в личку";
            }
        }

        private static string SentToEmail(string email) 
            => $"Отправь задание на почту: {email}. Не забудь подписать файл и работу.";

        private static string GetInform(string name)
        {
            try
            {
                using var sr = 
                    new StreamReader($"C:\\Users\\0811a\\Desktop\\Sbot\\{name}.txt");
                return sr.ReadToEnd();
            }
            catch 
            {
                return "Что-то пошло не так! Напиши старосте";
            }
        }
        private static async void DownloadFile(string url, string path)
        {
            byte[] data;

            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            using var content = response.Content;
            data = await content.ReadAsByteArrayAsync();
            using var file = File.Create(path);
            file.Write(data, 0, data.Length);
        }
    }
}
