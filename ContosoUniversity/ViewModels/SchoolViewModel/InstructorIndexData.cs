using System.Collections.Generic;
using ContosoUniversity.DomainModels;

namespace ContosoUniversity.ViewModels.SchoolViewModel
{
    public class InstructorIndexData
    {
        public IEnumerable<Instructor> Instrcuctors { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Enrollment> Enrollments { get; set; }
    }
}
