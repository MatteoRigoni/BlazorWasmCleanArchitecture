using Application.Services;
using Application.Utils;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    /// <summary>
    /// Authentication handler for API interaction from UI
    /// </summary>
    public class CustomHttpHandler(LocalStorageService localStorageService,
        HttpClientService httpClientService,
        NavigationManager navigationManager, 
        IAccountService accountService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                bool loginUrl = request.RequestUri.AbsoluteUri.Contains(Constant.LoginRoute);
                bool registerUrl = request.RequestUri.AbsoluteUri.Contains(Constant.RegisterRoute);
                bool refreshTokenUrl = request.RequestUri.AbsoluteUri.Contains(Constant.RefreshTokenRoute);
                bool adminCreateUrl = request.RequestUri.AbsoluteUri.Contains(Constant.CreateAdminRoute);
                if (loginUrl || registerUrl || refreshTokenUrl || adminCreateUrl)
                    return await base.SendAsync(request, cancellationToken);

                var result = await base.SendAsync(request, cancellationToken);
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Get token...
                    var tokenModel = await localStorageService.GetModelFromToken();
                    if (tokenModel == null) return result;

                    // Call for refresh...
                    var newJwtToken = await GetRefreshToken(tokenModel.Refresh);
                    if (string.IsNullOrEmpty(newJwtToken)) return result;

                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(Constant.HttpClientHeaderScheme, newJwtToken);
                    return await base.SendAsync(request, cancellationToken);
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<string> GetRefreshToken(string refreshToken)
        {
            try
            {
                var response = await accountService.RefreshTokenAsync(new DTOs.Request.Account.RefreshTokenDTO() { Token = refreshToken });
                if (response == null || response.Token == null)
                {
                    await ClearBrowserStorage();
                    NavigateToLogin();
                    return null;
                }
                await localStorageService.RemoveTokenFromBrowserLocalStorage();
                await localStorageService.SetBrowserLocalStorage(new DTOs.Request.Account.LocalStorageDTO() { Refresh = response.RefreshTOken, Token = response.Token });
                return response.Token;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void NavigateToLogin() => navigationManager.NavigateTo(navigationManager.BaseUri, true, true);
        private async Task ClearBrowserStorage() => await localStorageService.RemoveTokenFromBrowserLocalStorage();
    }
}
