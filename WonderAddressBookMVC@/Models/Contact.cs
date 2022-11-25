using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WonderAddressBookMVC_.Enums;
using RequiredAttribute = Microsoft.Build.Framework.RequiredAttribute;
namespace WonderAddressBookMVC_.Models
{
    public class Contact
    {
        public int Id { get; set; }
        [Required]
        public string? AppUserId { get; set; }

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

        [Display(Name = "Birthday")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required]
        public string? Address1 { get; set; }
        
        
        public string? Address2 { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public States State{ get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        [DataType(DataType.PostalCode)]
        public string? ZipCode { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        //image properties
        [NotMapped]
        [Display(Name = "Contact Image")]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        //Virtual navigation and creation of fk's in model to create join tables
        public virtual AppUser? AppUser { get; set; }
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();

    }
}
