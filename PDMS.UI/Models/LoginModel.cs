using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PDMS.UI.Models
{
    public class LoginModel

    {
        [DisplayName("Email Id: ")]
        [Required(ErrorMessage = "Email id is required.")]
        public string EmailId { get; set; }

        [DisplayName("Password: ")]
        [Required(ErrorMessage = "Password Field is required.")]
        public string Password { get; set; }
    }
}