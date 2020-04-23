using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;


namespace VkBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new FileStream("C:\\Users\\0811a\\Desktop\\vkBot\\VkBot\\VkBot\\config.txt", FileMode.Open);
            var reader = new StreamReader(config);
            var str = reader.ReadToEnd().Split('\n');
            reader.Close();
            var groupId = "130703143";
            var apiKey = str[0].Split()[1];
            var appId = ulong.Parse(str[1].Split()[1]);
            var server = new Server(apiKey, groupId);
            server.Authorize();
            var workWithDocuments = new Documents(server, 
                "C:\\Users\\0811a\\Desktop\\", 
                "C:\\Users\\0811a\\Desktop\\Sbot\\");
            
            var json = string.Empty;

            while (true)
            {
                json = server.GetJsonAnswer(json);
                
                Console.WriteLine(json);
                var temp = JObject.Parse(json);
                var col = temp["updates"].ToList();
                
                foreach (var items in col)
                {
                    if (items["type"].ToString() != "message_new") 
                        continue;

                    var msg = items["object"]["message"]["text"].ToString();

                    var arrayData = msg.Split(' ');
                    try
                    {
                        switch (arrayData[0].ToLower())
                        {
                            case "get":
                                msg = GetComm(arrayData[1]);
                                break;
                            
                            case "deadlines":
                                msg = GetInform("deadlines");
                                break;
                            
                            case "submit":
                                var userJson = server.Request("users.get",
                                    new Dictionary<string, string>
                                    {
                                        ["user_ids"] = items["object"]["message"]["from_id"].ToString()
                                    });
                                var name = userJson["response"][0]["last_name"].ToString();
                                var lis = items["object"]["message"]["attachments"];
                                try
                                {
                                    workWithDocuments.ReadFiles(lis, name, arrayData[1].ToLower());
                                    msg = "Успешно";
                                }
                                catch (ArgumentException)
                                {
                                    msg = "Неверный формат файла";
                                }
                                
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
                    server.SendMessage(items["object"]["message"]["from_id"].ToString(), 
                        msg,
                        items["object"]["message"]["random_id"].ToString());
                    Thread.Sleep(1000);
                }
            }
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
    }
}
