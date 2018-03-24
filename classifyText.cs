using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace RadioCanada
{
    public class classifyText
    {
        public ITextAnalyticsAPI Client {
            get {
                TextAnalyticsAPI clientN = new TextAnalyticsAPI() {  AzureRegion = AzureRegions.Westus, SubscriptionKey = "efc4cc9803844649a2e9af2fe0602afb" };
                return clientN;
            }
            set {

            }
        }
        public DetectedLanguage Lang { get; set; }
        public string Id { get; set; }
        public string Text { get; set; }
        public IList<string> KeyWords { get; set; }
        public Dictionary <string,double?>Sentiments { get; set; }
        public double Score { get; set; }

        public classifyText(string text,string id)
        {
            Text = text;
            Id = id;
            getLanguage();

        }

        private void getLanguage()
        {
            
            LanguageBatchResult result = Client.DetectLanguage(
                    new BatchInput(
                        new List<Input>()
                        {
                          new Input(Id, Text)
                        }));
            Lang = result.Documents[0].DetectedLanguages[0];
            getKeyWords();
        }
        private void getKeyWords()
        {
            KeyPhraseBatchResult result2 = Client.KeyPhrases(
                   new MultiLanguageBatchInput(
                       new List<MultiLanguageInput>()
                        {new MultiLanguageInput(Lang.Iso6391Name, Id,Text),}));
            KeyWords = result2.Documents[0].KeyPhrases;
            getSentiment();
        }
        private void getSentiment()
        {
            SentimentBatchResult result3 = Client.Sentiment(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput(Lang.Iso6391Name, Id, Text)
                        }));
            Sentiments = new Dictionary<string, double?>();
            Score = (double)result3.Documents[0].Score;
            foreach (var document in result3.Documents)
            {
                Sentiments.Add(document.Id, document.Score);
            }
            
        }
        public void InsertAnalyseInDB()
        {
            if (!RegistryExists(Id))
            {
                SqlConnection connection = new SqlConnection("Data Source=hackathonrc.database.windows.net;Initial Catalog=RadioCanadaNeuro;User ID=SiiCanadaAdmin;Password=S11C4n4d4Adm1n");
                SqlCommand cmd = new SqlCommand();

                //SqlDataReader reader;

                List<string> arr = new List<string>();
                //the first 3 keywords
                for (int i = 0; i < 3; i++)
                {

                    string sqlQuery = $"INSERT INTO Analyse (ArticleID,Keyword,Langue,Classification,Score) Values({Id},'{KeyWords[i].Replace("'","''")}','{Lang.Iso6391Name}',{i},{Score})";
                    arr.Add(sqlQuery);
                }
                string allQueries = string.Join(";", arr);
                cmd.CommandText = allQueries;
                cmd.Connection = connection;

                connection.Open();

                int c = cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
        private bool RegistryExists(string ArticleId) {
            SqlConnection conn = new SqlConnection("Data Source=hackathonrc.database.windows.net;Initial Catalog=RadioCanadaNeuro;User ID=SiiCanadaAdmin;Password=S11C4n4d4Adm1n");
            SqlCommand check = new SqlCommand("SELECT COUNT(Keyword) FROM [Analyse] WHERE ([ArticleID] = '"+ArticleId+"')", conn);
            conn.Open();
            int Exist = (int)check.ExecuteScalar();
            conn.Close();
            return(Exist > 0 ? true:false);
        }
    }

}