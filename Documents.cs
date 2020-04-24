using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json.Linq;

namespace VkBot
{
    public class Documents
    {
        private readonly string pathToSaveFiles;
        private readonly string pathToAddFiles;
        //private readonly Server server;
        
        public Documents(/*Server server, */string pathToSaveFiles, string pathToAddFiles)
        {
            this.pathToSaveFiles = pathToSaveFiles;
            this.pathToAddFiles = pathToAddFiles;
            //this.server = server;
        }

        public HashSet<string> GetStudents(string nameFile)
        {
            var strArray = new HashSet<string>();
            using (var sr = new StreamReader(pathToAddFiles + nameFile))
            {
                while(!sr.EndOfStream)
                    strArray.Add(sr.ReadLine().ToLower());
            }
            return strArray;
        }

        public string GetText(string name)
        {
            try
            {
                using var sr =
                    new StreamReader(pathToAddFiles + name);
                return sr.ReadToEnd();
            }
            catch
            {
                return "Что-то пошло не так! Напиши старосте";
            }
        }
        
        private void ConverterToPdf(string path, IEnumerable<string> namesWithExt)
        {
            var enumerable = namesWithExt.ToList();
            var document = new Document();
            var nameFile = enumerable[0].Split('_', '.')[0] + ".pdf";
            using var stream = new FileStream(path + nameFile, FileMode.Create, FileAccess.Write, FileShare.None);
            PdfWriter.GetInstance(document, stream);
            document.Open();
            foreach (var name in enumerable)
            {
                using var imageStream = new FileStream(
                    path + name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var image = Image.GetInstance(imageStream);
                imageStream.Close();
                image.ScaleAbsolute(document.PageSize.Width - 50, document.PageSize.Height - 50);
                image.Normalize();
                document.Add(image);
            }
            document.CloseDocument();
            document.Close();
        }

        public void WriteTextBag(string message)
        {
            using var sr = new StreamWriter(
                pathToAddFiles + "bag.txt", true, System.Text.Encoding.Default);
            sr.WriteLine(message + "\n");
        }
        
        private static async void DownloadFile(string url, string path)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            using var content = response.Content;
            var data = await content.ReadAsByteArrayAsync();
            await using var file = File.Create(path);
            file.Write(data, 0, data.Length);
        }

        private bool CheckImage(string ext) => ext.ToLower() == "jpeg" 
                                               || ext.ToLower() == "png" 
                                               || ext.ToLower() == "gif"
                                               || ext.ToLower() == "jpg";

        public bool CheckDoc(string ext) 
            => ext.ToLower() == "pdf" || ext.ToLower() == "doc" || ext.ToLower() == "docx" || ext.ToLower() == "xls" 
               || ext.ToLower() == "xlsx" || ext.ToLower() == "ppt" || ext.ToLower() == "pptx" || ext.ToLower() == "odt";

        public void ReadFiles(IEnumerable<JToken> files, string nameSender, string nameFolder)
        {
            var enumerable = files.ToList();
            var names = new List<string>();
            for (var i = 0; i < enumerable.Count; i++)
            {
                string ext;
                string name;
                var form = enumerable[i]["type"].ToString();
                if ("doc" == form)
                {
                    ext = enumerable[i][form]["ext"].ToString();
                    name = nameSender + "_" + i + "." + ext;
                    if (CheckImage(ext))
                        names.Add(name);
                    DownloadFile(enumerable[i][form]["url"].ToString(), pathToSaveFiles + nameFolder + "\\" + name);

                }
                else
                {
                    var url = enumerable[i][form]["sizes"][0]["url"].ToString();
                    name = nameSender + "_" + i + "." + "jpg";
                    DownloadFile(url, pathToSaveFiles + nameFolder + "\\" + name);
                    names.Add(name);
                }
            }
            if (names.Any()) ConverterToPdf(pathToSaveFiles + nameFolder + "\\", names);
        }
    }
}