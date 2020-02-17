using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechnicalAssesment.Model;
using TechnicalAssesment.ViewModel;

namespace TechnicalAssesment
{
    public class Program
    {
        private static readonly HttpClient _apiClient = new HttpClient();
        private static readonly string _remoteServiceBase = $"http://test-demo.aem-enersol.com";
        public static async Task Main(string[] args)
        {
            var isSuccess = await Login();
            if (isSuccess)
            {
                 await PostData();
                //string val;
                //Console.Write("Enter Integer: ");
                //val = Console.ReadLine();

                //Console.WriteLine(val);
            }
        }

        public static async Task<bool> Login()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
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
                    string responseStream = await httpResponseMessage.Content.ReadAsStringAsync();
                    _apiClient.SetBearerToken(responseStream);
                    //var data = await httpResponseMessage.Content();
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

        public static async Task PostData()
        {
            string itemUri = $"{_remoteServiceBase}/api/PlatformWell/GetPlatformWellActual";
            var httpResponseMessage = await _apiClient.GetAsync(itemUri);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
            }
            else
            {
            }
        }

    }
}
