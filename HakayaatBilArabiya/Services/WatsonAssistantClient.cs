using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace HakayaatBilArabiya.Services
{
    public class WatsonAssistantClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _serviceUrl;
        private readonly string _projectId;
        private readonly string _modelId;
        private string _accessToken;
        private DateTime _tokenExpiration;

        public WatsonAssistantClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _apiKey = configuration["IBMWatson:ApiKey"];
            _serviceUrl = configuration["IBMWatson:ServiceUrl"];
            _projectId = configuration["IBMWatson:ProjectId"];
            _modelId = configuration["IBMWatson:ModelId"];


        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration > DateTime.UtcNow)
            {
                return _accessToken;
            }

            var tokenUrl = "https://iam.cloud.ibm.com/identity/token";
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "urn:ibm:params:oauth:grant-type:apikey"),
                new KeyValuePair<string, string>("apikey", _apiKey)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = requestBody
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error obtaining access token: {errorResponse}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);

            _accessToken = responseObject.access_token;
            int expiresIn = responseObject.expires_in;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            return _accessToken;
        }

        public async Task<string> GenerateTextAsync(string المدخل_اليدوي, string الشخصيات, string الحيوانات, string المكان, string الموسم, string الوقت)
        {
            var عبارات_البداية = new List<string>
    {
        "كان يا ما كان،",
        "في قديم الزمان،",
        "في يوم من الأيام،"
    };

            var random = new Random();
            int index = random.Next(عبارات_البداية.Count);
            string بداية_القصة = عبارات_البداية[index];

            string inputText;

            if (!string.IsNullOrWhiteSpace(المدخل_اليدوي))
            {
                inputText = $"{بداية_القصة} {المدخل_اليدوي}.\nأكمل القصة.";
            }
            else
            {
                inputText = $"{بداية_القصة} في {المكان} عاش {الشخصيات} الذين كانوا يحبون التفاعل مع {الحيوانات} خلال {الموسم} في {الوقت}.\nأكمل القصة.";
            }

            var accessToken = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestBody = new
            {
                model_id = _modelId,
                input = inputText,
                parameters = new
                {
                    decoding_method = "greedy",
                    max_new_tokens = 300,
                    repetition_penalty = 1.1,
                    temperature = 0.7,
                    top_p = 0.9,
                    top_k = 50,
                    stop_sequences = new[] { "النهاية" }
                },
                project_id = _projectId
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error from IBM Watson API: {errorResponse}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);

            if (responseObject != null && responseObject.results != null && responseObject.results[0].generated_text != null)
            {
                return responseObject.results[0].generated_text.ToString();
            }
            else
            {
                return "لم يتم توليد أي نص.";
            }
        }





    }
}
