using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.DomainModels
{
    public abstract class Person
    {
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        [Column("FirstName")]
        [DisplayName("First Name")]
        [StringLength(50, ErrorMessage = "First name can't be longer than 50 characters")]
        public string FirstMidName { get; set; }

        [DisplayName("Full Name")]
        public string FullName => $"{this.FirstMidName}, {this.LastName}";
    }
}
