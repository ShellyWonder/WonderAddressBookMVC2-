using Microsoft.AspNetCore.Identity;
using WonderAddressBookMVC_.Data;
using WonderAddressBookMVC_.Models;
using Microsoft.EntityFrameworkCore;
using WonderAddressBookMVC_.Enums;
using WonderAddressBookMVC_.Services.Interfaces;

namespace WonderAddressBookMVC_.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DataService(ApplicationDbContext context, 
                           UserManager<AppUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task ManageDataAsync()
        {
            await _context.Database.MigrateAsync();

            //Custom Address Book seeding methods
            await SeedDefaultUserAsync(_userManager);
            await SeedDefaultContacts(_context);
            await SeedDefaultCategoriesAsync(_context);
        }

       

        public DateTime GetPostGresDate(DateTime datetime)
        {
            return DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
        }

        private async Task SeedDefaultUserAsync(UserManager<AppUser> userManagerSvc)
        {
            var defaultUser = new AppUser
            {
                UserName = "tonystark@mailinator.com",
                Email = "tonystark@mailinator.com",
                FirstName = "Tony",
                LastName = "Stark",
                EmailConfirmed = true,
            };
            try
            {
                var user = await userManagerSvc.FindByNameAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc&123!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ****");
                Console.WriteLine("Error Seeding Default User");
                Console.WriteLine(ex.Message);
                Console.WriteLine("**** END OF ERROR ****");
            }
        }

        private async Task SeedDefaultContacts(ApplicationDbContext dbContextSvc)
        {
            var userId = dbContextSvc.Users!.FirstOrDefault(u => u!.Email == "tonystark@mailinator.com").Id;

            var defaultContact = new Contact
            {
                AppUserId = userId,
                CreatedDate = DateTime.UtcNow,
                FirstName = "Henry",
                LastName = "McCoy",
                Address1 = "1407 Graymalkin Ln.",
                City = "Salem Center",
                PhoneNumber = "555-555-0101",
                State = States.NY,
                ZipCode = "10560",
                Email = "hankmccoy@starktower.com"
            };
            try
            {
                var contact = await dbContextSvc.Contacts.AnyAsync(c => c.Email == "hankmccoy@starktower.com" && c.AppUserId == userId);
                if (!contact)
                {
                    await dbContextSvc.AddAsync(defaultContact);
                    await dbContextSvc.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ****");
                Console.WriteLine("Error Seeding Default Contact");
                Console.WriteLine(ex.Message);
                Console.WriteLine("**** END OF ERROR ****");
            }

        }

        private async Task SeedDefaultCategoriesAsync(ApplicationDbContext dbContextSvc)
        {
            var userId = dbContextSvc.Users.FirstOrDefault(u => u.Email == "tonystark@mailinator.com")!.Id;

            var defaultCategory = new Category
            {
                AppUserId = userId,
                Name = "X-Men"
            };
            try
            {
                var category = await dbContextSvc.Categories.AnyAsync(c => c.Name == "X-Men" && c.AppUserId == userId);
                if (!category)
                {
                    await dbContextSvc.AddAsync(defaultCategory);
                    await dbContextSvc.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ****");
                Console.WriteLine("Error Seeding Default Category");
                Console.WriteLine(ex.Message);
                Console.WriteLine("**** END OF ERROR ****");
            }

        }

        private async Task DefaultCategoryAssign(IAddressBookService categorySvc, ApplicationDbContext dbContextSvc)
        {
            var user = dbContextSvc.Users
                .Include(c => c.Categories)
                .Include(c => c.Contacts)
                .FirstOrDefault(u => u.Email == "tonystark@mailinator.com");
            var contact = dbContextSvc.Contacts
                .FirstOrDefault(u => u.Email == "hankmccoy@starktower.com");

            foreach (var category in user?.Categories!)
            {
                await categorySvc.AddContactToCategoryAsync(category.Id, contact!.Id);
            }
        }
    }
}
