using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using OTPWarden.Controllers.V1.Contract.Responses;
using OTPWarden.UIServices.Abstractions;

namespace OTPWarden.UIServices;

public sealed class OAuthHttpService : IOAuthHttpService
{
    #region Fields

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private readonly AppSettings _appSettings;
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;

    #endregion

    #region Constructor

    public OAuthHttpService(IHttpClientFactory httpClientFactory, ProtectedLocalStorage protectedLocalStorage,
        AppSettings appSettings, NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _protectedLocalStorage = protectedLocalStorage ?? throw new ArgumentNullException(nameof(protectedLocalStorage));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    #endregion

    #region Public Methods

    public async Task<U> PostAsync<T, U>(string requestUri, T content)
    {
        return await PostAsync<U>(requestUri, new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json"));
    }

    public async Task<T> PostAsync<T>(string requestUri, HttpContent content)
    {
        T response = default;
        HttpClient httpClient = _httpClientFactory.CreateClient("default");

        await RefreshTokenIfNeeded(httpClient);

        ProtectedBrowserStorageResult<string> accessToken = await _protectedLocalStorage.GetAsync<string>("AccessToken");

        if (accessToken.Success)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(await _jsRuntime.InvokeAsync<string>("getUserAgent"));
            HttpResponseMessage responseMessage = await httpClient.PostAsync(requestUri, content);

            if (responseMessage.IsSuccessStatusCode)
            {
                response = await responseMessage.Content.ReadFromJsonAsync<T>();
            }
            else if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("/login", true);
            }
        }
        else
        {
            _navigationManager.NavigateTo("/login", true);
        }

        return response;
    }

    public async Task<T> GetAsync<T>(string requestUri)
    {
        T response = default;
        HttpClient httpClient = _httpClientFactory.CreateClient("default");

        await RefreshTokenIfNeeded(httpClient);

        ProtectedBrowserStorageResult<string> accessToken = await _protectedLocalStorage.GetAsync<string>("AccessToken");

        if (accessToken.Success)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(await _jsRuntime.InvokeAsync<string>("getUserAgent"));
            HttpResponseMessage responseMessage = await httpClient.GetAsync(requestUri);

            if (responseMessage.IsSuccessStatusCode)
            {
                response = await responseMessage.Content.ReadFromJsonAsync<T>();
            }
            else if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("/login", true);
            }
        }
        else
        {
            _navigationManager.NavigateTo("/login", true);
        }

        return response;
    }

    #endregion

    #region Private Methods

    private async Task RefreshTokenIfNeeded(HttpClient httpClient)
    {
        JwtSecurityTokenHandler accessTokenHandler = new JwtSecurityTokenHandler();
        ProtectedBrowserStorageResult<string> accessTokenStorageResult = await _protectedLocalStorage.GetAsync<string>("AccessToken");

        if (accessTokenStorageResult.Success && accessTokenHandler.CanReadToken(accessTokenStorageResult.Value))
        {
            JwtSecurityToken accessToken = accessTokenHandler.ReadJwtToken(accessTokenStorageResult.Value);

            if (accessToken.ValidTo <= DateTime.UtcNow + TimeSpan.FromMinutes(1))
            {
                ProtectedBrowserStorageResult<string> refreshTokenStorageResult = await _protectedLocalStorage.GetAsync<string>("RefreshToken");

                if (refreshTokenStorageResult.Success)
                {
                    List<KeyValuePair<string, string>> request = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                        new KeyValuePair<string, string>("refresh_token", refreshTokenStorageResult.Value)
                    };
                    httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(await _jsRuntime.InvokeAsync<string>("getUserAgent"));
                    HttpResponseMessage responseMessage = await httpClient.PostAsync($"{_appSettings.Host}/api/v1/auth/authenticate", new FormUrlEncodedContent(request));

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        AuthenticateUserApiResponse response = await responseMessage.Content.ReadFromJsonAsync<AuthenticateUserApiResponse>();

                        if (response != null)
                        {
                            await _protectedLocalStorage.SetAsync("AccessToken", response.AccessToken);
                            await _protectedLocalStorage.SetAsync("RefreshToken", response.RefreshToken);
                        }
                    }
                }
            }
        }
    }

    #endregion
}
