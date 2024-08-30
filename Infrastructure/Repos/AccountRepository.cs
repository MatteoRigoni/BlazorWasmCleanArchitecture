using Application.Contracts;
using Application.DTOs.Request.Account;
using Application.DTOs.Response;
using Application.DTOs.Response.Account;
using Domain.Entities.Authentication;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Application.Utils;
using Constant = Application.Utils.Constant;
using Infrastructure.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos
{
    public class AccountRepository
        (RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext appDbContext) : IAccount
    {
        private async Task<ApplicationUser> FindUserByEmailAsync(string email)
            => await userManager.FindByEmailAsync(email);
        private async Task<IdentityRole> FindRoleByNameAsync(string roleName)
            => await roleManager.FindByNameAsync(roleName);
        
        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private async Task<string> GenerateToken(ApplicationUser user)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var userClaims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, (await userManager.GetRolesAsync(user)).FirstOrDefault().ToString()),
                    new Claim("FullName", user.Name)
                };

                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: userClaims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: credentials
                    );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<GeneralResponse> SaveRefreshToken(string userId, string token)
        {
            try
            {
                var user = await appDbContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
                if (user == null)
                    appDbContext.RefreshTokens.Add(new RefreshToken() { UserId = userId, Token = token });
                else
                    user.Token = token;

                await appDbContext.SaveChangesAsync();
                return new GeneralResponse(true, null!);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, ex.Message);
            }
        }

        private async Task<GeneralResponse> AssignUserToRole(ApplicationUser user, IdentityRole role)
        {
            if (user is null || role is null) return new GeneralResponse(false, "Model state cannot be empty");
            if (await FindRoleByNameAsync(role.Name!) == null)
                await CreateRoleAsync(role.Adapt(new CreateRoleDTO()));

            IdentityResult result = await userManager.AddToRoleAsync(user, role.Name!);
            string error = CheckResponse(result);
            if (!string.IsNullOrEmpty(error))
                return new GeneralResponse(false, error);
            else
                return new GeneralResponse(true, $"{user.Name} assigned to {role.Name} role");
        }

        private static string CheckResponse(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(_ => _.Description);
                return string.Join(Environment.NewLine, errors);
            }
            return null!;
        }

        public async Task<GeneralResponse> ChangeUserRoleAsync(ChangeUserRoleRequestDTO model)
        {
            if (await FindRoleByNameAsync(model.RoleName) is null) return new GeneralResponse(false, "Role not found");
            if (await FindUserByEmailAsync(model.UserEmail) is null) return new GeneralResponse(false, "User not found");

            var user = await FindUserByEmailAsync(model.UserEmail);
            var previousRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            var removeOldRole = await userManager.RemoveFromRoleAsync(user, previousRole);
            var error = CheckResponse(removeOldRole);
            if (!string.IsNullOrEmpty(error))
                return new GeneralResponse(false, error);

            var result = await userManager.AddToRoleAsync(user, model.RoleName);
            var response = CheckResponse(result);
            if (!string.IsNullOrEmpty(error))
                return new GeneralResponse(false, response);
            else
                return new GeneralResponse(true, "Role changed");
        }

        public async Task<GeneralResponse> CreateAccountAsync(CreateAccountDTO model)
        {
            try
            {
                if (await FindUserByEmailAsync(model.EmailAddress) != null)
                    return new GeneralResponse(false, "User already exists!");

                var user = new ApplicationUser()
                {
                    Name = model.Name,
                    UserName = model.EmailAddress,
                    Email = model.EmailAddress,
                    PasswordHash = model.Password
                };
                var result = await userManager.CreateAsync(user, model.Password);
                string error = CheckResponse(result);
                if (!string.IsNullOrEmpty(error))
                    return new GeneralResponse(false, error);

                var (flag, message) = await AssignUserToRole(user, new IdentityRole() { Name = model.Role });
                return new GeneralResponse(flag, message);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, ex.Message);
            }
        }

        public async Task CreateAdmin()
        {
            try
            {
                if ((await FindRoleByNameAsync(Constant.Role.Admin)) != null) return;
                var admin = new CreateAccountDTO()
                {
                    Name = "Admin",
                    Password = "Admin123!",
                    EmailAddress = "boss@admin.com",
                    Role = Constant.Role.Admin
                };
                await CreateAccountAsync(admin);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<GeneralResponse> CreateRoleAsync(CreateRoleDTO model)
        {
            try
            {
                if ((await FindRoleByNameAsync(model.Name)) == null)
                {
                    var response = await roleManager.CreateAsync(new IdentityRole(model.Name));
                    var error = CheckResponse(response);
                    if (!string.IsNullOrEmpty(error))
                        throw new Exception(error);
                    else
                        return new GeneralResponse(true, $"{model.Name} created");
                }
                return new GeneralResponse(false, $"{model.Name} already exists");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, ex.Message);
            }
        }

        public async Task<IEnumerable<GetRoleDTO>> GetRolesAsync()
         => (await roleManager.Roles.ToListAsync()).Adapt<IEnumerable<GetRoleDTO>>();

        public async Task<IEnumerable<GetUsersWithRolesResponseDTO>> GetUsersWithRolesAsync()
        {
            var allUsers = await userManager.Users.ToListAsync();
            if (allUsers is null) return null;

            var List = new List<GetUsersWithRolesResponseDTO>();
            foreach (var user in allUsers)
            {
                var getUserRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
                var getRoleInfo = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == getUserRole.ToLower());
                List.Add(new GetUsersWithRolesResponseDTO()
                {
                    Name = user.Name,
                    Email = user.Email,
                    RoleId = getRoleInfo.Id,
                    RoleName = getRoleInfo.Name
                });
            }
            return List;
        }

        public async Task<LoginResponse> LoginAccountAsync(LoginDTO model)
        {
            try
            {
                var user = await FindUserByEmailAsync(model.EmailAddress);
                if (user == null)
                    return new LoginResponse(false, "User not found");

                SignInResult result;
                try
                {
                    result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                }
                catch (Exception)
                {
                    return new LoginResponse(false, "Invalid credentials");
                }
                if (!result.Succeeded)
                    return new LoginResponse(false, "Invalid credentials");

                string jwtToken = await GenerateToken(user);
                string refreshToken = GenerateRefreshToken();
                if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(refreshToken))
                    return new LoginResponse(false, "Error during login, please contact administrator");
                else
                {
                    var saveResult = await SaveRefreshToken(user.Id, refreshToken);
                    if (saveResult.Flag)
                        return new LoginResponse(true, $"{user.Name} succesfully loggen in", jwtToken, refreshToken);
                    else
                        return new LoginResponse(false, "Error during login, please contact administrator");
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse(false, ex.Message);
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenDTO model)
        {
            var token = await appDbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == model.Token);
            if (token == null) return new LoginResponse();

            var user = await userManager.FindByIdAsync(token.UserId);
            string newToken = await GenerateToken(user);
            string newRefreshToken = GenerateRefreshToken();
            var saveResult = await SaveRefreshToken(user.Id, newRefreshToken);
            if (saveResult.Flag)
                return new LoginResponse(true, $"{user.Name} succesfully re-logged in", newToken, newRefreshToken);
            else
                return new LoginResponse(false, null);
        }
    }
}
