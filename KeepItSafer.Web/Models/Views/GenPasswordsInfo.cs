using System.ComponentModel.DataAnnotations;

namespace KeepItSafer.Web.Models.Views
{
    public class GenPasswordsInfo
    {
        [Required]
        public int MinLength { get; set; }
        [Required]
        public int MaxLength { get; set; }
        public bool AllowSpecialCharacters { get; set; } = true;
        public bool AllowNumbers { get; set; } = true;
    }
}