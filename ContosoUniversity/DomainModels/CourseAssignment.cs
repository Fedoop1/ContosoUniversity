namespace ContosoUniversity.DomainModels
{
    public class CourseAssignment
    {
        public int CourseID { get; set; }
        public int InstructorID { get; set; }
        public virtual Course Course { get; set; }
        public virtual Instructor Instructor { get; set; }
    }
}
