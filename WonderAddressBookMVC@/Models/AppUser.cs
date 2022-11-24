using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WonderAddressBookMVC_.Models
{
    public class AppUser : IdentityUser 
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "{0} must be at least {2} characters and no more than {1} characters.", MinimumLength = 2)]
        public string? FirstName { get; set; }
        
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "{0} must be at least {2} characters and no more than {1} characters.", MinimumLength = 2)]
        public string? LastName { get; set; }
        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }
    }
}
