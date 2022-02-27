using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PDMS.UI.Models


{
    public class Registration
    {
        [DisplayName("First Name: ")]
        [Required(ErrorMessage = "Firstname is required.")]
        public string FirstName { get; set; }

        [DisplayName("Last Name: ")]
        [Required(ErrorMessage = "Lastname is required.")]
        public string LastName { get; set; }

        [DisplayName("Email Id: ")]
        [Required(ErrorMessage = "Email id is required.")]
        [EmailAddress(ErrorMessage = "Please enter valid Email Id.")]
        public string EmailId { get; set; }

        [DisplayName("Password: ")]
        [Required(ErrorMessage = "Password Field is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)\S{8,20}$", ErrorMessage = "Passwords need to meet following criterias : Min 8 characters.; One uppercase character required.; One lowercase character required.;" +
            "One symbol character required.; No whitespace in the password.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage ="Both Passwords needs to be same.")]
        [DisplayName("Confirm Password: ")]
        [Required(ErrorMessage = "ConfirmPassword Field is required.")]
        public string ConfirmPassword { get; set; }
    }
}
