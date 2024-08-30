using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.Account
{
    public class LoginDTO
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
