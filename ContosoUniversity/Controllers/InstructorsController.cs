using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.DomainModels;
using ContosoUniversity.ViewModels.SchoolViewModel;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly SchoolContext context; 

        public InstructorsController(SchoolContext context) => this.context = context;

        // GET: Instructors
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();
            viewModel.Instrcuctors = await this.context.Instructors
                .Include(x => x.OfficeAssignment)
                .Include(x => x.CourseAssignments)  // can be deleted at Explicit loading
                    .ThenInclude(x => x.Course)     // can be deleted at Explicit loading
                        .ThenInclude(x => x.Enrollments)    // can be deleted at Explicit loading
                            .ThenInclude(x => x.Student)    // can be deleted at Explicit loading
                .Include(x => x.CourseAssignments)
                    .ThenInclude(x => x.Course)
                        .ThenInclude(x => x.Department)
                .AsNoTracking()
                .OrderBy(x => x.LastName)
                .ToListAsync();

            if (id != null)
            {
                ViewData["InstructorID"] = id;
                var instructor = viewModel.Instrcuctors.Single(x => x.ID == id);
                viewModel.Courses = instructor.CourseAssignments.Select(x => x.Course);
            }

            if (courseID != null)
            {
                //                      ---Explicit load---
                //var course = viewModel.Courses.Single(x => x.CourseID == courseID);
                //await context.Entry(course).Collection(x => x.Enrollments).LoadAsync();
                //foreach (var enrollment in course.Enrollments)
                //{
                //    await context.Entry(enrollment).Reference(x => x.Student).LoadAsync();
                //}
                //viewModel.Enrollments = course.Enrollments;

                ViewData["CourseID"] = courseID;
                viewModel.Enrollments = viewModel.Courses.Single(x => x.CourseID == courseID).Enrollments;
            }

            return View(viewModel);
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // GET: Instructors/Create
        public IActionResult Create()
        {
            PopulateAssignedCourseData(new Instructor() { CourseAssignments = new List<CourseAssignment>() });
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastName,FirstMidName,HireDate,OfficeAssignment")]Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.CourseAssignments = new List<CourseAssignment>();
                foreach (var course in selectedCourses)
                {
                    instructor.CourseAssignments.Add(new CourseAssignment() {InstructorID = instructor.ID, CourseID = int.Parse(course)});   
                }
            }

            if (ModelState.IsValid)
            {
                context.Add(instructor);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await context.Instructors
                .Include(x => x.CourseAssignments).ThenInclude(x => x.Course)
                .Include(x => x.OfficeAssignment)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == id);

            if (instructor == null)
            {
                return NotFound();
            }

            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // POST: Instructors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructorToUpdate = await context.Instructors
                .Include(x => x.CourseAssignments)
                .Include(x => x.OfficeAssignment)
                .FirstOrDefaultAsync(x => x.ID == id);


            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync(instructorToUpdate, "", x => x.FirstMidName, x => x.LastName,
                    x => x.HireDate, x => x.OfficeAssignment))
                {
                    if (string.IsNullOrEmpty(instructorToUpdate.OfficeAssignment?.Location))
                    {
                        instructorToUpdate.OfficeAssignment = null;
                    }

                    UpdateInstructorCourses(instructorToUpdate, selectedCourses);

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again and if the problem persists, see your system administrator");
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            UpdateInstructorCourses(instructorToUpdate, selectedCourses);
            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }

        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await context.Instructors.FindAsync(id);
            await context.Entry(instructor).Reference(x => x.OfficeAssignment).LoadAsync();
            context.Instructors.Remove(instructor);

            var departments = context.Departments.Where(x => x.InstructorID == id);
            await departments.ForEachAsync(x => x.InstructorID = null);

            
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InstructorExists(int id) => context.Instructors.Any(e => e.ID == id);

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(x => x.CourseID));
            var viewModel = new List<AssignedCourseData>();

            foreach (var course in context.Courses)
            {
                viewModel.Add(new AssignedCourseData() {CourseID = course.CourseID, Title = course.Title, Assigned = instructorCourses.Contains(course.CourseID)});
            }

            ViewData["Courses"] = viewModel;
        }

        private void UpdateInstructorCourses(Instructor instructorToUpdate, string[] selectedCourses)
        {
            if (selectedCourses is null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>(instructorToUpdate.CourseAssignments.Select(x => x.CourseID));

            foreach (var cours in context.Courses)
            {
                if (selectedCoursesHS.Contains(cours.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(cours.CourseID))
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment() {InstructorID = instructorToUpdate.ID, CourseID = cours.CourseID});
                    }
                }
                else
                {
                    if (instructorCourses.Contains(cours.CourseID))
                    {
                        context.CourseAssignments.Remove(
                            instructorToUpdate.CourseAssignments.FirstOrDefault(x => x.CourseID == cours.CourseID));
                    }
                }
            }
        }
    }
}
