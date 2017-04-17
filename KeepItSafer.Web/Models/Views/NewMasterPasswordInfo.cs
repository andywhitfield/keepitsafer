using System.ComponentModel.DataAnnotations;

namespace KeepItSafer.Web.Models.Views
{
    public class NewMasterPasswordInfo
    {
        [Required]
        [CheckNewAndConfirmPasswordsAreTheSame("NewMasterPasswordConfirm")]
        public string NewMasterPassword { get; set; }
        [Required]
        public string NewMasterPasswordConfirm { get; set; }
    }
}