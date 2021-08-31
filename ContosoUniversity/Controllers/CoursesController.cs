using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.DomainModels;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SchoolContext context;

        public CoursesController(SchoolContext context) => this.context = context;

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var courses = context.Courses.Include(c => c.Department);
            return View(await courses.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            PopulateDepartmentsDropDownList();
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Credits,Title,DepartmentID")] Course course)
        {
            if (ModelState.IsValid)
            {
                context.Add(course);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await context.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseToUpdate = await context.Courses.FirstOrDefaultAsync(x => x.CourseID == id);

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync(courseToUpdate, "", c => c.Credits, c => c.DepartmentID, c => c.Title))
                {
                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again and if your problem persists, see your system administrator.");
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }

            PopulateDepartmentsDropDownList(courseToUpdate.DepartmentID);
            return View(courseToUpdate);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await context.Courses
                .FirstOrDefaultAsync(m => m.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            await context.Entry(course).Reference(x => x.Department).LoadAsync();
            return View(course);
        }

        public IActionResult UpdateCourseCredits() => View();

        [HttpPost]
        public async Task<IActionResult> UpdateCourseCredits(int? multiplier)
        {
            if (multiplier == null)
            {
                ModelState.AddModelError("", "Multiplier can't be null or any type other than number.");
                return View();
            }

            ViewData["RowsAffected"] =
                await context.Database.ExecuteSqlRawAsync("UPDATE Course " +
                                                          $"SET Credits = Credits * {multiplier}");
            return RedirectToAction(nameof(Index));
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await context.Courses.FindAsync(id);
            context.Courses.Remove(course);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id) => context.Courses.Any(e => e.CourseID == id);

        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentQuery = from department in context.Departments 
                                                        orderby  department.Name 
                                                        select department;

            ViewData["DepartmentID"] =
                new SelectList(departmentQuery.AsNoTracking(), "DepartmentID", "Name", selectedDepartment);
        }
    }
}
