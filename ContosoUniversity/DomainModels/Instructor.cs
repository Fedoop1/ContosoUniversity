﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.DomainModels
{
    public class Instructor : Person
    {
        [DataType(DataType.Date)]
        [DisplayName("Hire Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime HireDate { get; set; }

        public virtual ICollection<CourseAssignment> CourseAssignments { get; set; }
        public virtual OfficeAssignment OfficeAssignment { get; set; }

    }
}