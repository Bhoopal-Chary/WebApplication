using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Register
    {
        [Key]
        public int EmployeeId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public virtual ICollection<EmployeeExRef> EmployeeExRefs { get; set; }
    }

}
