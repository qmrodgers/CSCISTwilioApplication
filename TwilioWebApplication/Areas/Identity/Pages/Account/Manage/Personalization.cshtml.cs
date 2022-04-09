using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TwilioWebApplication.Models;

namespace TwilioWebApplication.Areas.Identity.Pages.Account.Manage
{
    public class PersonalizationModel : PageModel
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public PersonalizationModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            
            
            
        }
        public IFormFile? FormFile { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        
        public class InputModel
        {

            [Display(Name = "Brand Name")]
            public string NewBrandName { get; set; }

            [Display(Name = "Brand Image File")]
            public IFormFile NewFormFile { get; set; }
            




        }



        private async Task LoadAsync(User user)
        {
            string brandName = user.brandName;


            Input = new InputModel()
            {
                
                NewBrandName = brandName,
                
            };


        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangePersonalizationAsync()
        {

            var user = await _userManager.GetUserAsync(User);

            if (Input.NewFormFile == null && ModelState.ErrorCount <= 1)
            {
                user.brandName = Input.NewBrandName;
                await _userManager.UpdateAsync(user);
                StatusMessage = "Successfull changed Brand Name";
                return RedirectToPage();
            }

            

            
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if(Input.NewFormFile.Length > 1000000) {
                StatusMessage = "Please upload a file smaller than 1MB";
            }



            if (true)
            {
                var userId = await _userManager.GetUserIdAsync(user);

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
                return RedirectToPage();
            }

            StatusMessage = "Your email is unchanged.";
            return RedirectToPage();
        }


    }
}
    
