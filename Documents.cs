﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json.Linq;
using VkNet.Enums;

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
            try
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
                    ScaleImage(image, document.PageSize.Width - 50, document.PageSize.Height - 50);
                    //image.ScaleAbsolute(document.PageSize.Width - 50, document.PageSize.Height - 50);
                    image.Normalize();
                    document.Add(image);
                }

                document.CloseDocument();
                document.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Source);
            }
        }

        private void ScaleImage(Image image, float width, float height)
        {
            if (image.Width > width && image.Height > height)
            {
                if (image.Width >= image.Height) 
                    ScaleWidth(image, width);
                else
                    ScaleHeight(image, height);
                return;
            }

            if (image.Width >= width)
                ScaleWidth(image, width);
            else
                ScaleHeight(image, height);
        }

        private void ScaleWidth(Image image, float width)
        {
            var widthImage = image.Width;
            image.ScaleAbsoluteWidth(width);
            image.ScaleAbsoluteHeight(image.Height / widthImage * width);
        }
        
        private void ScaleHeight(Image image, float height)
        {
            var heightImage = image.Height;
            image.ScaleAbsoluteHeight(height);
            image.ScaleAbsoluteWidth(image.Width / heightImage * height);
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
                string name;
                var form = enumerable[i]["type"].ToString();
                if ("doc" == form)
                {
                    var ext = enumerable[i][form]["ext"].ToString();
                    name = nameSender + "_" + i + "." + ext;
                    if (CheckImage(ext))
                        names.Add(name);
                    DownloadFile(enumerable[i][form]["url"].ToString(), pathToSaveFiles + nameFolder + "\\" + name);

                }
                else
                {
                    var url = enumerable[i][form]["sizes"][6]["url"].ToString();
                    Console.WriteLine(enumerable[i][form]["sizes"][6]["height"] + " " + enumerable[i][form]["sizes"][6]["width"]);
                    name = nameSender + "_" + i + "." + "jpg";
                    DownloadFile(url, pathToSaveFiles + nameFolder + "\\" + name);
                    names.Add(name);
                }
            }
            if (names.Any()) ConverterToPdf(pathToSaveFiles + nameFolder + "\\", names);
        }
    }
}