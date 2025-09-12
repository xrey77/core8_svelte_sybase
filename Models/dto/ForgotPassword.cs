namespace core8_svelte_sybase.Models.dto
{
    public class ForgotPassword {        
        public int Mailtoken {get; set;}
        public string Password_hash {get; set;}
    }
}