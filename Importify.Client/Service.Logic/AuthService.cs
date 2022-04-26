﻿using Blazored.LocalStorage;
using Importify.Client.Model;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Importify.Client.Service.Logic
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _storageService;

        public AuthService(IConfiguration configuration, ILocalStorageService storageService)
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://localhost:{configuration["port"]}/api/")
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _storageService = storageService;
        }

        public async Task<User> GetUser(string login)
        {
            string responseString;
            User user;

            var cookieContent = await _storageService.GetItemAsync<string>("access_token");

            if (cookieContent is null)
                return null;

            _httpClient.DefaultRequestHeaders.Remove("access_token");
            _httpClient.DefaultRequestHeaders.Add("access_token", cookieContent);

            var response = await _httpClient.GetAsync($"authentication/user?login={login}");

            if ((int)response.StatusCode == 401)
            {
                if (await SendRefreshToken(cookieContent) == -1)
                    return null;

                response = await _httpClient.GetAsync($"authentication/user?login={login}");

                responseString = await response.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<User>(responseString);

                return user;
            }

            responseString = await response.Content.ReadAsStringAsync();
            user = JsonConvert.DeserializeObject<User>(responseString);

            return user;
        }

        public async Task<List<User>> GetUsers()
        {
            string responseString;
            List<User> users;

            var cookieContent = await _storageService.GetItemAsync<string>("access_token");

            if (cookieContent is null)
                return null;

            _httpClient.DefaultRequestHeaders.Remove("access_token");
            _httpClient.DefaultRequestHeaders.Add("access_token", cookieContent);

            var response = await _httpClient.GetAsync($"authentication");

            if ((int)response.StatusCode == 401)
            {
                if (await SendRefreshToken(cookieContent) == -1)
                    return null;

                response = await _httpClient.GetAsync($"authentication");

                responseString = await response.Content.ReadAsStringAsync();
                users = JsonConvert.DeserializeObject<List<User>>(responseString);

                return users;
            }

            responseString = await response.Content.ReadAsStringAsync();
            users = JsonConvert.DeserializeObject<List<User>>(responseString);

            return users;
        }

        public async Task<Tokens> Login(LoginUser user)
        {
            var response = await _httpClient.PostAsJsonAsync("authentication/login", user);
            var tokensString = await response.Content.ReadAsStringAsync();
            
            var tokens = JsonConvert.DeserializeObject<Tokens>(tokensString);

            await _storageService.SetItemAsStringAsync("user", tokens.Login);
            await _storageService.SetItemAsStringAsync("role", tokens.Role);

            return tokens;
        }

        public async Task<int> Registration(RegistrationUser user)
        {
            var response = await _httpClient.PostAsJsonAsync("authentication", user);
            var responseString = await response.Content.ReadAsStringAsync();

            if (int.TryParse(responseString, out int idOfUser))
                return idOfUser;
            else
                return -1;
        }

        private async Task<int> SendRefreshToken(string cookieContent)
        {
            var tok = new Tokens()
            {
                AccessToken = cookieContent,
                RefreshToken = await _storageService.GetItemAsync<string>("refresh_token"),
            };

            var resp = await _httpClient.PostAsJsonAsync("token/refresh", tok);

            if ((int)resp.StatusCode == 404)
                return -1;

            var respString = await resp.Content.ReadAsStringAsync();
            var tokens = JsonConvert.DeserializeObject<Tokens>(respString);

            await _storageService.SetItemAsStringAsync("access_token", tokens.AccessToken);
            await _storageService.SetItemAsStringAsync("refresh_token", tokens.RefreshToken);

            cookieContent = await _storageService.GetItemAsync<string>("access_token");
            _httpClient.DefaultRequestHeaders.Remove("access_token");
            _httpClient.DefaultRequestHeaders.Add("access_token", cookieContent);

            return 0;
        }
    }
}
