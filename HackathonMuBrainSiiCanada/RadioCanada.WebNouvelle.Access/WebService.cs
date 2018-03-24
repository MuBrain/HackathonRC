using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RadioCanada.WebNouvelle.Access
{

    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
    public class WebService
    {
        public static string GetCategoryFrench(string body)
        {
            string txt = string.Empty;
            string url = @"https://ussouthcentral.services.azureml.net/workspaces/cf61fcd5cd074cedab98530fbd2c10b8/services/429d9f50e5c84a75a106cdc61ad62396/execute?api-version=2.0&details=true";
            const string apiKey = "VhICAn80PmO2ZqjURlqAmq6VokXaHScnp36siCVGnLWVe88Q5Tlm/dZ/r78ELaBbB5+jt0XKUd1eRlHpbmVMBg=="; // Replace this with the API key for the web service
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + apiKey);
            request.AutomaticDecompression = DecompressionMethods.GZip;

           
           

            request.Method = "POST";
            request.ContentType = "application/json";
            


            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                //JObject json = JObject.Parse("test");
                string stringData = "{\"Inputs\": {\"input1\": {\"ColumnNames\": [\"News\",\"Category\"],\"Values\": [[\""+body+"\",\"VALUE\"]]}},\"GlobalParameters\": {}}"; //place body here

                streamWriter.Write(stringData);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)request.GetResponse();
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            

            return result;
        }

        public static bool GetArticle(int NeuroId, out string Title, out string Body, out string CategoryName,out string ImageURL)
        {
            Body = "";
            CategoryName = "";
            Title = "";
            ImageURL = "";

            try
            {
                string txt = string.Empty;
                string url = @"https://services.radio-canada.ca/hackathon/neuro/v1/news-stories/" + NeuroId.ToString() + "?client_key=bf9ac6d8-9ad8-4124-a63c-7b7bdf22a2ee";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    txt = reader.ReadToEnd();
                }

                JObject jObject = JObject.Parse(txt);
                CategoryName = jObject["themeTag"]["name"].ToString();
                Body = jObject["body"]["html"].ToString();
                Title = jObject["title"].ToString();
                try
                {
                    JObject joImageURL = (JObject)jObject["summaryMultimediaContentForDetail"];
                    JArray joImageURL2 = (JArray)joImageURL["concreteImages"];
                    var MetaImage = joImageURL2[0];
                    
                    ImageURL = MetaImage["mediaLink"]["href"].ToString();
                }
                catch
                {
                }

            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return false;
            }
            return true;
        }


    }




}
