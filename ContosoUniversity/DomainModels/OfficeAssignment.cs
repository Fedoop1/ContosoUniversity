using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.DomainModels
{
    public class OfficeAssignment
    {
        [Key]
        public int InstructorID { get; set; }

        [StringLength(50)]
        [DisplayName("Office Location")]
        public string Location { get; set; }

        public virtual Instructor Instructor { get; set; }
    }
}
