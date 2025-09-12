using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace core8_svelte_sybase.Entities
{
    [Table("users")]
    public class User {

        [Key]
        public int Id {get; set;}

        [Column("firstname", TypeName = "varchar(50)")]
        public string FirstName {get; set;}

        [Column("lastname", TypeName = "varchar(50)")]
        public string LastName {get; set;}

        [Column("email", TypeName = "varchar(100)")]
        public string Email { get; set; }

        [Column("mobile", TypeName = "varchar(50)")]
        public string Mobile { get; set; }

        [Column("username", TypeName = "varchar(50)")]
        public string UserName {get; set;}

        [Column("password_hash", TypeName = "text")]
        public string Password_hash {get; set;}

        [Column("roles", TypeName = "varchar(20)")]
        public string Roles { get; set; }

        [Column("isactivated")]
        public int IsActivated {get; set;}

        [Column("isblocked")]
        public int Isblocked {get; set;}

        [Column("mailtoken")]
        public int Mailtoken {get; set;}

        [Column("qrcodeurl", TypeName = "text")]
        public string Qrcodeurl {get; set;}

        [Column("profilepic", TypeName = "varchar(80)")]
        public string Profilepic {get; set;}

        [Column("secretkey", TypeName = "text")]
        public string Secretkey {get; set;}

        [Column("createdat")]
        public DateTime CreatedAt {get; set;}

        [Column("updatedat")]
        public DateTime UpdatedAt {get; set;}
    }
}