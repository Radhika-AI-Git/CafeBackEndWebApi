using System.ComponentModel.DataAnnotations;

namespace CafeBackEndWebApi.Models
{
    public class Cafe
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public string? Logo { get; set; }

        [Required]
        public string Location { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}