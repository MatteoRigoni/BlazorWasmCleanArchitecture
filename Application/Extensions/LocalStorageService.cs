using Application.DTOs.Request.Account;
using Application.Utils;
using NetcodeHub.Packages.Extensions.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public class LocalStorageService(ILocalStorageService localStorageService)
    {
        private async Task<string> GetBrowserLocalStorage()
        {
            var tokenModel = await localStorageService.GetEncryptedItemAsStringAsync(Constant.BrowserStorageKey);
            return tokenModel;
        }

        public async Task<LocalStorageDTO> GetModelFromToken()
        {
            try
            {
                string token = await GetBrowserLocalStorage();
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(token))
                    return new LocalStorageDTO();

                return DeserializeJsonString<LocalStorageDTO>(token);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SetBrowserLocalStorage(LocalStorageDTO localStorageDTO)
        {
            try
            {
                string token = SerializeObject(localStorageDTO);
                await localStorageService.SaveAsEncryptedStringAsync(Constant.BrowserStorageKey, token);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveTokenFromBrowserLocalStorage()
            => await localStorageService.DeleteItemAsync(Constant.BrowserStorageKey);

        private static string SerializeObject<T>(T modelObject)
            => JsonSerializer.Serialize<T>(modelObject, JsonOptions());
        private static T DeserializeJsonString<T>(string jsonString)
            => JsonSerializer.Deserialize<T>(jsonString, JsonOptions());

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip
            };
        }
    }
}
