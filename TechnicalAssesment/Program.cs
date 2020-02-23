using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TechnicalAssesment.Model;

namespace TechnicalAssesment
{
    public class Program
    {

        private static readonly HttpClient _apiClient = new HttpClient();
        private static readonly string _remoteServiceBase = $"http://test-demo.aem-enersol.com";

        private static SqlConnection conn; 
        public static async Task Main(string[] args)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + "\\localDb.mdf";
            //Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Atila\source\repos\TechnicalAssesment\TechnicalAssesment\localDb.mdf; Integrated Security = True

            conn = new SqlConnection($"Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = {path}; Integrated Security = True");

            var isSuccess = await Login();
            if (isSuccess)
            {
                await GetDataFromAPI();
            }


        }

        public static async Task<bool> Login()
        {
            try
            {
                var loginData = new LoginData()
                {
                    username = "user@aemenersol.com",
                    password = "Test@123"
                };

                var json = JsonConvert.SerializeObject(loginData, Formatting.None);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string itemUri = $"{_remoteServiceBase}/api/Account/Login";
                var httpResponseMessage = await _apiClient.PostAsync(itemUri, content);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string responseStream = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    _apiClient.SetBearerToken(responseStream.Trim('"'));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public static async Task GetDataFromAPI()
        {
            string itemUri = $"{_remoteServiceBase}/api/PlatformWell/GetPlatformWellActual";
            var httpResponseMessage = await _apiClient.GetAsync(itemUri);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var data = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<List<Platform>>(data);
                conn.Open();

                using (conn)
                {
                    foreach (var item in result)
                    {
                        SqlCommand CheckExistData = new SqlCommand($"SELECT * FROM [Platform] WHERE Id = {item.Id}", conn);
                        SqlDataReader reader = CheckExistData.ExecuteReader();

                        if (!reader.HasRows)
                        {
                            SqlCommand CmdSql = new SqlCommand("INSERT INTO [Platform] (Id, UniqueName, Latitude, Longitude, CreatedAt, UpdatedAt) VALUES (@Id, @UniqueName, @Latitude, @Longitude, @CreatedAt, @UpdatedAt)", conn);
                            //conn.Open();
                            CmdSql.Parameters.AddWithValue("@Id", item.Id);
                            CmdSql.Parameters.AddWithValue("@UniqueName", item.UniqueName);
                            CmdSql.Parameters.AddWithValue("@Latitude", item.Latitude);
                            CmdSql.Parameters.AddWithValue("@Longitude", item.Longitude);
                            CmdSql.Parameters.AddWithValue("@CreatedAt", item.CreatedAt);
                            CmdSql.Parameters.AddWithValue("@UpdatedAt", item.UpdatedAt);
                            reader.Close();
                            CmdSql.ExecuteNonQuery();
                            
                            if (item.well != null)
                            {
                                foreach (var w in item.well)
                                {
                                    SqlCommand CheckWellExistData = new SqlCommand($"SELECT * FROM [Well] WHERE Id = {w.Id} AND PlatformId = {w.PlatformId}", conn);
                                    SqlDataReader readerWell = CheckWellExistData.ExecuteReader();

                                    if (!readerWell.HasRows)
                                    {
                                        SqlCommand wellSql = new SqlCommand("INSERT INTO [Well] (Id, PlatformId, UniqueName, Latitude, Longitude, CreatedAt, UpdatedAt) VALUES (@Id, @PlatformId, @UniqueName, @Latitude, @Longitude, @CreatedAt, @UpdatedAt)", conn);
                                        //conn.Open();
                                        wellSql.Parameters.AddWithValue("@Id", w.Id);
                                        wellSql.Parameters.AddWithValue("@PlatformId", w.PlatformId);
                                        wellSql.Parameters.AddWithValue("@UniqueName", w.UniqueName);
                                        wellSql.Parameters.AddWithValue("@Latitude", w.Latitude);
                                        wellSql.Parameters.AddWithValue("@Longitude", w.Longitude);
                                        wellSql.Parameters.AddWithValue("@CreatedAt", w.CreatedAt);
                                        wellSql.Parameters.AddWithValue("@UpdatedAt", w.UpdatedAt);
                                        readerWell.Close();
                                        wellSql.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        SqlCommand updateSql = new SqlCommand($"UPDATE [Well] SET UniqueName = {item.UniqueName}, Latitude = {item.Latitude}, Longitude = {item.Longitude}, CreatedAt = {item.CreatedAt}, UpdatedAt = {item.UpdatedAt} WHERE Id = {w.Id}", conn);
                                    }
                                       
                                }

                            }
                        }
                        else
                        {
                            SqlCommand updateSql = new SqlCommand($"UPDATE [Platform] SET UniqueName = {item.UniqueName}, Latitude = {item.Latitude}, Longitude = {item.Longitude}, CreatedAt = {item.CreatedAt}, UpdatedAt = {item.UpdatedAt} WHERE Id = {item.Id}", conn);
                        }
                       
                       
                    }

                    conn.Close();
                }

            }
            else
            {

            }
        }


    }

    public class Platform
    {
        public int Id { get; set; }
        public string UniqueName { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<Well> well { get; set; }
    }

    public class Well
    {
        public int Id { get; set; }
        public int PlatformId { get; set; }
        public string UniqueName { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }


}
