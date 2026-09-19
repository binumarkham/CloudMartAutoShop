using System.Net.Http.Headers;
using System.Net.Http.Json;
using CloudMartAutoShop.Pwa.Models;
using Microsoft.JSInterop;

namespace CloudMartAutoShop.Pwa.Services;

public class ApiService(
    HttpClient http,
    IJSRuntime js)
{
    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        var response =
            await http.PostAsJsonAsync(
                "api/Auth/login",
                request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResponse>();

        if (result is not null)
        {
            await js.InvokeVoidAsync(
                "localStorage.setItem",
                "authToken",
                result.Token);

            await js.InvokeVoidAsync(
                "localStorage.setItem",
                "businessName",
                result.BusinessName);

            await js.InvokeVoidAsync(
                "localStorage.setItem",
                "userName",
                result.Name);

            await js.InvokeVoidAsync(
                "localStorage.setItem",
                "userRole",
                result.Role);
        }

        return result;
    }

    public async Task Logout()
    {
        await js.InvokeVoidAsync(
            "localStorage.removeItem",
            "authToken");

        await js.InvokeVoidAsync(
            "localStorage.removeItem",
            "businessName");

        await js.InvokeVoidAsync(
            "localStorage.removeItem",
            "userName");

        await js.InvokeVoidAsync(
            "localStorage.removeItem",
            "userRole");

        http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> Get<T>(string url)
    {
        await ApplyAuthentication();

        var response =
            await http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content
            .ReadFromJsonAsync<T>();
    }

    public async Task<bool> Post<T>(
        string url,
        T data)
    {
        await ApplyAuthentication();

        var response =
            await http.PostAsJsonAsync(
                url,
                data);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Put<T>(
        string url,
        T data)
    {
        await ApplyAuthentication();

        var response =
            await http.PutAsJsonAsync(
                url,
                data);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Patch<T>(
        string url,
        T data)
    {
        await ApplyAuthentication();

        var response =
            await http.PatchAsJsonAsync(
                url,
                data);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Delete(string url)
    {
        await ApplyAuthentication();

        var response =
            await http.DeleteAsync(url);

        return response.IsSuccessStatusCode;
    }

    private async Task ApplyAuthentication()
    {
        var token =
            await js.InvokeAsync<string?>(
                "localStorage.getItem",
                "authToken");

        http.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue(
                    "Bearer",
                    token);
    }
}