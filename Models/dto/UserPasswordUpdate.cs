using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace core8_svelte_sybase.Models.dto
{
  public class UserPasswordUpdate
    {        
        public string Password_hash { get; set; }
        public DateTime UpdatedAt { get; set; }
    }    
}