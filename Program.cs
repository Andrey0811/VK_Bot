using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;


namespace VkBot
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            /*try
            {*/
                var (apiKey, groupId) = GetConfig("C:\\Users\\0811a\\Desktop\\VkBot\\config.txt");
                var server = new Server(apiKey, groupId);
                server.Authorize();
                var workWithDocuments = new Documents( /*server, */
                    "C:\\Users\\0811a\\Desktop\\",
                    "C:\\Users\\0811a\\Desktop\\VkBot\\Sbot\\");
                var students = workWithDocuments.GetStudents("students.txt");

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
                        var attach = string.Empty;
                        
                        var arrayData = msg.Split(' ');
                        if (arrayData[0] == ".restart")
                            throw new Exception();
                        try
                        {
                            switch (arrayData[0].ToLower())
                            {
                                case "get":
                                    msg = "Задания тут";
                                    attach = GetComm(arrayData[1]);
                                    break;
                                
                                case "deadlines":
                                    msg = workWithDocuments.GetText("deadlines.txt");
                                    break;

                                case "submit":
                                    var userJson = server.Request("users.get",
                                        new Dictionary<string, string>
                                        {
                                            ["user_ids"] = items["object"]["message"]["from_id"].ToString()
                                        });
                                    var name = userJson["response"][0]["last_name"].ToString().ToLower();
                                    if (!students.Contains(name))
                                    {
                                        msg = "Не являешься студентом данной группы";
                                        break;
                                    }

                                    var lis = items["object"]["message"]["attachments"];
                                    if (!lis.Any())
                                    {
                                        msg = "Не прикреплена работа";
                                        break;
                                    }

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

                                case "bag":
                                    var sb = new StringBuilder();
                                    foreach (var str in arrayData.Skip(1))
                                        sb.Append(str + " ");
                                    //sb.Append("\n");
                                    workWithDocuments.WriteTextBag(sb.ToString());
                                    msg = "ОК, исправим";
                                    break;

                                default:
                                    msg = workWithDocuments.GetText("help.txt");
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            msg = "Ошибка в команде!\r\n" + workWithDocuments.GetText("help.txt");
                        }

                        server.SendMessage(items["object"]["message"]["from_id"].ToString(),
                            msg,
                            items["object"]["message"]["random_id"].ToString(),
                            attach);
                        Thread.Sleep(1000);
                    }
                }
            /*}
            catch
            {
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Environment.Exit(0);
            }*/
        }

        private static Tuple<string, string> GetConfig(string path)
        {
            var config = new FileStream(path, FileMode.Open);
            var reader = new StreamReader(config);
            var str = reader.ReadToEnd().Split('\n');
            reader.Close();
            var groupId = str[2].Split()[1];
            var apiKey = str[0].Split()[1];
            //var appId = ulong.Parse(str[1].Split()[1]);
            return Tuple.Create(apiKey, groupId);
        }

        private static string GetComm(string topic)
        {
            try
            {
                switch (topic.ToLower())
                {
                    case "matan":
                        return "https://drive.google.com/open?id=1FTuKB5JZV2s6O0EPQQZyj7XEvLeghtAY";
                    
                    case "algemb":
                        return "https://drive.google.com/open?id=16Y12jLwsPhmAaRTF3KMNWsjjPhEqc9Hx";
                    
                    case "rus":
                        return "https://drive.google.com/open?id=1-1MtlKHD_89QnUk57jPvz1qgEXEwku30";
                    
                    case "algema":
                        return "https://classroom.google.com/c/NzQ3NTU5MTQ1MTda";
                    
                    default:
                        return "help";
                }
            }
            catch
            {
                return "Не все материалы доставлены";
            }
        }

        private static string SentToEmail(string email) 
            => $"Сдавать задание на почту: {email}. Не забудь подписать файл и работу.";
    }
}

