using System.Linq;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.DomainModels;
using ContosoUniversity.Infrastructure;

namespace ContosoUniversity.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolContext context;

        public StudentsController(SchoolContext context)
        {
            this.context = context;
        }

        // GET: Student
        public async Task<IActionResult> Index(string sortOrder, string currentFilter , string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "LastName_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "EnrollmentDate" ? "EnrollmentDate_desc" : "EnrollmentDate";

            searchString ??= currentFilter;

            ViewData["CurrentFilter"] = searchString;

            var students = string.IsNullOrEmpty(searchString)
                ? context.Students
                : context.Students.Where(x =>
                    x.LastName.Contains(searchString) ||
                    x.FirstMidName.Contains(searchString));

            students = sortOrder switch
            {
                null => students.OrderBy(x => x.LastName),
                _ when sortOrder.Contains("_desc") => students.OrderByDescending(e => EF.Property<object>(e, sortOrder.Substring(0, sortOrder.IndexOf("_desc")))),
                _ => students.OrderBy(e => EF.Property<object>(e, sortOrder)),
            };

            const int pageSize = 3;
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Student/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await context.Students
                .Include(x => x.Enrollments)
                .ThenInclude(x => x.Course).AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnrollmentDate,FirstMidName,LastName")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    context.Add(student);
                    await context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again and if the problem persists - see you system administrator");
            }

            return View(student);
        }   

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentToUpdate = await context.Students.FirstOrDefaultAsync(x => x.ID == id);

            if (await TryUpdateModelAsync<Student>(studentToUpdate, "", x => x.FirstMidName, x => x.LastName,
                x => x.EnrollmentDate))
            {
                try
                {
                    await context.SaveChangesAsync();
                    RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again and if the problem persists - see you system administrator");
                }
            }

            return View(studentToUpdate);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await context.Students
                .FirstOrDefaultAsync(m => m.ID == id);

            if (student == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again and if the problem persists - see you system administrator";
            }

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await context.Students.FindAsync(id);

            if (student is null)
            {
                return NotFound();
            }

            try
            {
                context.Students.Remove(student);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {

                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private bool StudentExists(int id)
        {
            return context.Students.Any(e => e.ID == id);
        }
    }
}
