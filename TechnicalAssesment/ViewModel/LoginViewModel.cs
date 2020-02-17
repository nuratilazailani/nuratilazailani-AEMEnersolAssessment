using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TechnicalAssesment.Model;

namespace TechnicalAssesment.ViewModel
{
    public class LoginViewModel
    {
        private readonly HttpClient _apiClient;
        private readonly string _remoteServiceBase;

        public LoginViewModel(HttpClient httpClient)
        {
            _apiClient = httpClient;
            _remoteServiceBase = $"http://test-demo.aem-enersol.com";
        }

        public async void Login()
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
                    
                }
                else
                {
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}
