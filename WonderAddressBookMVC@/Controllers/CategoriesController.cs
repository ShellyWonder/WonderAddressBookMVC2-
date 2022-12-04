using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using WonderAddressBookMVC_.Data;
using WonderAddressBookMVC_.Models;
using WonderAddressBookMVC_.Models.ViewModels;
using WonderAddressBookMVC_.Services.Interfaces;
using WonderAddressBookMVC_.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WonderAddressBookMVC_.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailService;
        #region Constructor
        public CategoriesController(ApplicationDbContext context,
                                    UserManager<AppUser> userManager,
                                    IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }
        #endregion

        #region Get Categories (Index)
        // GET: Categories
        public async Task<IActionResult> Index(string? swalMessage = null)
        {
            ViewData["Message"] = swalMessage;

            //Get current logged in user to filter all cat. assigned to user
            string appUserId = _userManager.GetUserId(User);

            var categories = await _context.Categories.Where(c => c.AppUserId == appUserId)
                                                 .Include(c => c.AppUser)
                                                 .ToListAsync();
            return View(categories);
        }
        #endregion

        #region Get Categories Email Category
        public async Task<IActionResult> EmailCategory(int Id)
        {
            String appUserId = _userManager.GetUserId(User);
            Category? category = await _context.Categories
                                              .Include(c=> c.Contacts)
                                              .FirstOrDefaultAsync(c => c.Id == Id && c.AppUserId == appUserId);
            //get all contacts' emails in the category belonging to User;
            List<string?>? emails = category!.Contacts
                                           .Select(c => c.Email)
                                           .ToList();

            EmailData emailData = new EmailData()
            {
                GroupName = category.Name,
                EmailAddress = String.Join(";", emails),
                Subject = $"Group Message: {category.Name}"
            };
            EmailCategoryViewModel model = new EmailCategoryViewModel()
            {
                Contacts = category.Contacts.ToList(),
                EmailData = emailData
            };

            return View(model);//Returns views populated w data
        }
        #endregion

        #region Post Categories Email Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>EmailCategory(EmailCategoryViewModel ecvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(ecvm.EmailData!.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    return RedirectToAction("Index", "Categories", new { swalMessage = "Success: Email Sent" });
                }
                catch (Exception)
                {
                    return RedirectToAction("Index", "Categories", new { swalMessage = "Error: Email Send Failed" });
                    throw;
                }
            }
            return View (ecvm); 
        }

        #endregion

        #region Get Categories Create
        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }
        #endregion

        #region POST Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AppUserId,Name")] Category category)
        {
            ModelState.Remove("AppUserId");
            if (ModelState.IsValid)
            {
                string appUserId = _userManager.GetUserId(User);
                category.AppUserId = appUserId;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        #endregion

        #region Get Categories Edit
        public async Task<IActionResult> Edit(int? id)
        {
            string appUserId = _userManager.GetUserId(User);


            var category = await _context.Categories
                               .Where(c => c.Id == id && c.AppUserId == appUserId)
                               .FirstOrDefaultAsync();
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        #endregion

        #region POST: Categories Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,Name")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //ensures appUser only has access to edit his own categories
                    string appUserId = _userManager.GetUserId(User);
                    category.AppUserId = appUserId;

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        #endregion

        #region Get Categories Delete
        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        #endregion

        #region POST Categories Delete
        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Category Exists
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
        #endregion
    }
}
