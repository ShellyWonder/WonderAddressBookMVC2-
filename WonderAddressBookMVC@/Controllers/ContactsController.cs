using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WonderAddressBookMVC_.Data;
using WonderAddressBookMVC_.Services;
using WonderAddressBookMVC_.Enums;
using WonderAddressBookMVC_.Models;
using WonderAddressBookMVC_.Models.ViewModels;
using WonderAddressBookMVC_.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WonderAddressBookMVC_.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        #region Constructor
        public ContactsController(ApplicationDbContext context,
                                    UserManager<AppUser> userManager,
                                    IImageService imageService,
                                    IAddressBookService addressBookService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
        }
        #endregion

        #region Get Contacts
        // GET: Contacts
        public IActionResult Index(int categoryId)
        {
            //explicit syntax
            List<Contact> contacts = new List<Contact>();
            string appUserId = _userManager.GetUserId(User);

            //return userId & associated contacts & categories
            AppUser appUser = _context.Users
                                      .Include(c => c.Contacts)
                                      .ThenInclude(c => c.Categories)
                                      .FirstOrDefault(u => u.Id == appUserId)!;

            var categories = appUser.Categories;


            //filters contact results by category
            if (categoryId == 0)//All contacts option from filter drop down
            {

                contacts = appUser.Contacts.OrderBy(c => c.LastName)
                                            .ThenBy(c => c.FirstName)
                                            .ToList();
            }
            else // if anything other than zero
            {
                contacts = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)!
                                  .Contacts.OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                        .ToList();
            }

            // bind categories to select list
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", categoryId);

            return View(contacts);
        }
        #endregion

        #region Get Search Contacts
        public IActionResult SearchContacts(string searchString)
        {
            string appUserId = _userManager.GetUserId(User);
            var contacts = new List<Contact>();
            AppUser appUser = _context.Users
                                .Include(c => c.Contacts)
                                .ThenInclude(c => c.Categories)
                                .FirstOrDefault(u =>u.Id == appUserId)!;
            if (String.IsNullOrEmpty(searchString))
            {
                contacts = appUser.Contacts
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }
            else
            {
                contacts = appUser.Contacts.Where(c =>c.FullName!.ToLower().Contains(searchString.ToLower()))
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }
            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", 0);

            return View(nameof(Index), contacts);
        }
        #endregion

        #region Get Email Contacts
        //Get Email 


        #endregion

        #region Post Email Contacts
        //Post Email Contacts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailContact(int id)
        {
            string appUserId = _userManager.GetUserId(User);
            Contact? contact = await _context.Contacts!
                                           .Where(c => c.Id == id && c.AppUserId == appUserId)
                                           .FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }

            //instaniating new instance of EmailData object
            EmailData emailData = new EmailData()
            {
                //properties
                EmailAddress = contact.Email!,
                FirstName = contact.FirstName,
                LastName = contact.LastName
                 
            };

            //instaniating new instance of EmailData object
            EmailContactViewModel model = new EmailContactViewModel()
            {
                //properties
                Contact = contact,
                EmailData = emailData
            };
            return View(model);
        }


        #endregion

        #region Get Contact Details
        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }
        #endregion

        #region Create Get
        // GET: Contacts/Create

        //Task needed with await
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");
            return View();
        }
        #endregion

        #region Create Post
        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,CreatedDate,ImageFile, ")] Contact contact, List<int> CategoryList)
        {
            //allows for the required AppUserId to be removed from the bind
            ModelState.Remove("AppUserId");
            if (ModelState.IsValid)
            {
                //generates required AppUserID & CreatedDate
                contact.AppUserId = _userManager.GetUserId(User);
                contact.CreatedDate = DateTime.SpecifyKind(contact.BirthDate!.Value, DateTimeKind.Utc);

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                }
                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }
                _context.Add(contact);
                await _context.SaveChangesAsync();
                //loop over all the selected categories:
                foreach (int categoryId in CategoryList)
                {
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                }

                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));

        }
        #endregion

        #region Get Contacts Edit
        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }
            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserId == appUserId)
                                                 .FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _addressBookService.GetContactCategoryIdsAsync(contact.Id));

            return View(contact);
        }
        #endregion

        #region Post Contacts Edit
        // POST: Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,CreatedDate,ImageFile,ImageData,ImageType")] Contact contact,List<int> CategoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {   //converting time imported from views into proper format accepted by Db
                    contact.CreatedDate = DateTime.SpecifyKind(contact.CreatedDate, DateTimeKind.Utc);

                    if (contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);

                    }
                    //Allowing new image upload to save to Db
                    if (contact.ImageFile !=null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                    //save categories
                    //remove current categories
                    List<Category> oldCategories = (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();
                    foreach (var category in oldCategories)
                    {
                        await _addressBookService.RemoveContactFromCategoryAsync(category.Id, contact.Id);
                    }
                    //add selected categories back to db
                    foreach (int  categoryId in CategoryList)
                    {
                        await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);
            return View(contact);
        }
        #endregion

        #region Get Delete
        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }
        #endregion

        #region Post Delete
        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
            }
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region ContactExists
        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
        #endregion
    }
}
