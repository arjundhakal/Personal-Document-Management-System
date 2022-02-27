using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PDMS.UI.Models
{
    public class ForgotPasswordModel

    {
        [DisplayName("Username : ")]
        [Required(ErrorMessage = "Username is required.")]
        [EmailAddress(ErrorMessage = "Please enter valid Username.")]
        public string Username { get; set; }
    }
}