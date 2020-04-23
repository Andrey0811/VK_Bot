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
        private readonly string pathToSaveFiles = "C:\\Users\\0811a\\Desktop\\";
        private readonly string pathToAddFiles;
        private readonly Server server;
        
        public Documents(Server server, string pathToSaveFiles, string pathToAddFiles)
        {
            this.pathToSaveFiles = pathToSaveFiles;
            this.pathToAddFiles = pathToAddFiles;
            this.server = server;
        }

        /*public string[] OpenXmlListOfStudents(string pathToFile)
        {
            var objExcel = new Microsoft.Office.Interop.Excel.Application();
            //Открываем книгу.                                                                                                                                                        
            var objWorkBook = objExcel.Workbooks.Open(pathToFile, 0, true, 5, "", "", false, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            //Выбираем таблицу(лист).
            var objWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)objWorkBook.Sheets[1];

            // Указываем номер столбца (таблицы Excel) из которого будут считываться данные.
            var numCol = 1;
    
            var usedColumn = (Range)objWorkSheet.UsedRange.Columns[numCol];
            var myvalues = (Array) objWorkBook.Cells.Value2;
            var strArray = myvalues.OfType<object>().Select(o => o.ToString()).ToArray();

            // Выходим из программы Excel.
            objExcel.Quit();
            return strArray;
        }*/
        
        private static void ConverterToPdf(string path, IEnumerable<string> namesWithExt)
        {
            var enumerable = namesWithExt.ToList();
            var document = new Document();
            var nameFile = enumerable[0].Split('_', '.')[0] + ".pdf";
            using var stream = new FileStream(path + nameFile, FileMode.Create, FileAccess.Write, FileShare.None);
            PdfWriter.GetInstance(document, stream);
            document.Open();
            foreach (var name in enumerable)
            {
                using var imageStream = new FileStream(path + "\\" + name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var image = Image.GetInstance(imageStream);
                document.Add(image);
            }
            
            document.Close();
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
                                               || ext.ToLower() == "gif";

        public bool CheckDoc(string ext) 
            => ext.ToLower() == "pdf" || ext.ToLower() == "doc" || ext.ToLower() == "docx" || ext.ToLower() == "xls" 
               || ext.ToLower() == "xlsx" || ext.ToLower() == "ppt" || ext.ToLower() == "pptx" || ext.ToLower() == "odt";

        public void ReadFiles(IEnumerable<JToken> files, string nameSender, string nameFolder)
        {
            var enumerable = files.ToList();
            var names = new List<string>();
            for (var i = 0; i < enumerable.Count; i++)
            {
                var ext = enumerable[i]["doc"]["ext"].ToString();
                var name = nameSender + "_" + i + "." + ext;
                if (CheckImage(ext))
                    names.Add(name);
                DownloadFile(enumerable[i]["doc"]["url"].ToString(), pathToSaveFiles + nameFolder + "\\" + name);
            }
            //if (names.Any()) ConverterToPdf(pathToSaveFiles + nameFolder + "\\", names);
        }
    }
}