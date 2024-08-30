using Application.DTOs.Request.Account;
using Application.DTOs.Response;
using Application.DTOs.Response.Account;
using Application.Extensions;
using Application.Utils;
using Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services
{
    public class AccountService(HttpClientService httpClientService) : IAccountService
    {
        public async Task<GeneralResponse> ChangeUserRoleAsync(ChangeUserRoleRequestDTO model)
        {
            try
            {
                var publicClient = httpClientService.GetPublicClient();
                var response = await publicClient.PostAsJsonAsync(Constant.ChangeUserRoleRoute, model);
                string error = CheckResponseStatus(response);
                if (!string.IsNullOrEmpty(error))
                    return new GeneralResponse(false, error);

                var result = await response.Content.ReadFromJsonAsync<GeneralResponse>();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GeneralResponse> CreateAccountAsync(CreateAccountDTO model)
        {
            try
            {
                var publicClient = httpClientService.GetPublicClient();
                var response = await publicClient.PostAsJsonAsync(Constant.RegisterRoute, model);
                string error = CheckResponseStatus(response);
                if (!string.IsNullOrEmpty(error))
                    return new GeneralResponse(false, error);

                var result = await response.Content.ReadFromJsonAsync<GeneralResponse>();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CreateAdmin()
        {
            try
            {
                var client = httpClientService.GetPublicClient();
                await client.PostAsync(Constant.CreateAdminRoute, null);
            }
            catch (Exception)
            {
            }
        }

        public async Task<GeneralResponse> CreateRoleAsync(CreateRoleDTO model)
        {
            try
            {
                var publicClient = httpClientService.GetPublicClient();
                var response = await publicClient.PostAsJsonAsync(Constant.CreateRoleRoute, model);
                string error = CheckResponseStatus(response);
                if (!string.IsNullOrEmpty(error))
                    return new GeneralResponse(false, error);

                var result = await response.Content.ReadFromJsonAsync<GeneralResponse>();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<GetUsersWithRolesResponseDTO>> GetUsersWithRolesAsync()
        {
            try
            {
                var privateClient = await httpClientService.GetPrivateClient();
                var response = await privateClient.GetAsync(Constant.GetUserWithRolesRoute);
                string error = CheckResponseStatus(response);
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);

                var result = await response.Content.ReadFromJsonAsync<IEnumerable<GetUsersWithRolesResponseDTO>>();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<LoginResponse> LoginAccountAsync(LoginDTO model)
        {
            try
            {
                var publicClient = httpClientService.GetPublicClient();
                var response = await publicClient.PostAsJsonAsync(Constant.LoginRoute, model);
                string error = CheckResponseStatus(response);
                if (!string.IsNullOrEmpty(error))
                    return new LoginResponse(Flag: false, Message: error);

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result;
            }
            catch (Exception ex)
            {
                return new LoginResponse(false, ex.Message);
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenDTO model)
        {
            try
            {
                var publicClient = httpClientService.GetPublicClient();
                var response = await publicClient.PostAsJsonAsync(Constant.RefreshTokenRoute, model);
                string error = CheckResponseStatus(response);
                if (!string.IsNullOrEmpty(error))
                    return new LoginResponse(Flag: false, Message: error);

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result;
            }
            catch (Exception ex)
            {
                return new LoginResponse(false, ex.Message);
            }
        }

        private static  string CheckResponseStatus(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return $"Sorry, unknow error occured: {Environment.NewLine} Status code:{response.StatusCode} {Environment.NewLine} Reason phrase:{response.ReasonPhrase}";
            else
                return null;
        }
    }
}
