using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public virtual ICollection<EmployeeExRef> EmployeeExRefs { get; set; }
    }

}
