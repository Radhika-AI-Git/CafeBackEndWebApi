using System.ComponentModel.DataAnnotations;

namespace CafeBackEndWebApi.Models
{
    public class Employee
    {
        [Key]
        [RegularExpression(@"^UI[A-Za-z0-9]{7}$")]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string EmailAddress { get; set; }

        [Required, RegularExpression(@"^[89]\d{7}$")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Gender { get; set; }

        public Guid? CafeId { get; set; }
        public Cafe Cafe { get; set; }

        public DateTime? StartDate { get; set; }
    }
}