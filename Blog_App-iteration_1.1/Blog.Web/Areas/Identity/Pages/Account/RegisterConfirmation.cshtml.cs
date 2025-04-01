// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Blog.Infrastructure.Entities;
using Blog.Core.Constants;

namespace Blog.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public RegisterConfirmationModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public string Email { get; set; }
        
        public string StatusMessage { get; set; }

        public bool DisplayConfirmAccountLink { get; set; } = false;
        
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            ReturnUrl = returnUrl;
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            StatusMessage = EmailConstants.EmailConfirmationSent;
            
            return Page();
        }
    }
} 