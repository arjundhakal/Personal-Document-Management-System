using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PDMS.UI.Models
{
    public class ResetPasswordModel

    {
        [Required(ErrorMessage = "Username Field is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Return Token Field is required.")]
        public string ReturnToken { get; set; }
        [DisplayName("New Password: ")]
        [Required(ErrorMessage = "New Password Field is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)\S{8,20}$", ErrorMessage = "Passwords need to meet following criterias : Min 8 characters.; One uppercase character required.; One lowercase character required.;" +
            "One symbol character required.; No whitespace in the password.")]
        public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "Both Passwords needs to be same.")]
        [DisplayName("Confirm New Password: ")]
        [Required(ErrorMessage = "Confirm New Password Field is required.")]
        public string ConfirmNewPassword { get; set; }
    }
}