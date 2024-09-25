﻿using Demo_Dal.Entities;
using Demo_PL.Helpers;
using Demo_PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Demo_PL.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> usermanager;
		private readonly SignInManager<ApplicationUser> signInManager;

		public AccountController(UserManager<ApplicationUser> _manager,
                                SignInManager<ApplicationUser> _signInManager)
        {
            usermanager = _manager;
			signInManager = _signInManager;
		}
        public IActionResult SignUp()
        {
            return View(new RegisterViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterViewModel registerViewModel)
        {
            if(ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    Email = registerViewModel.EmailAddress,
                    UserName = registerViewModel.EmailAddress.Split('@')[0],
                    IsAgree = registerViewModel.IsAgree
                };

                var result = await usermanager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                    return RedirectToAction("SignIn");

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(registerViewModel);
        }
		public IActionResult SignIn()
		{
			return View(new SignInViewModel());
		}
        [HttpPost]
		public async Task<IActionResult> SignIn(SignInViewModel signInViewModel)
		{
            if (ModelState.IsValid)
            {
                var user = await usermanager.FindByEmailAsync(signInViewModel.EmailAddress);
                if (user == null)
                    ModelState.AddModelError("", "Email Doesn't Exist");
                var isCorrectPass = await usermanager.CheckPasswordAsync(user, signInViewModel.Password);
                if (isCorrectPass)
                {
                    var result = await signInManager.PasswordSignInAsync(user, signInViewModel.Password, signInViewModel.RememberMe, false);
                    if (result.Succeeded)
                        return RedirectToAction("Index", "Home");
                }

            }

			return View(signInViewModel);
		}

		public async Task<IActionResult> SignOut()
		{
            await signInManager.SignOutAsync();
			return RedirectToAction(nameof(SignIn));
		}

        public IActionResult ForgetPassword()
        {
            return View(new ForgetPasswordViewModel());
        }
        [HttpPost]
		public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
		{
            if (ModelState.IsValid)
            {
                var user = await usermanager.FindByEmailAsync(model.EmailAddress);
                if(user != null)
                {
                    var token = await usermanager.GeneratePasswordResetTokenAsync(user);

                    var resetPasswordLink = Url.Action("ResetPassword", "Account", 
                                                       new {Email = model.EmailAddress, Token = token}, Request.Scheme);
                    var email = new Email()
                    {
                        To = model.EmailAddress,
                        Subject = "Reset Password",
                        Body = resetPasswordLink
                    };
                    EmailSetting.SendEmail(email);
                    return RedirectToAction("CompleteForgetPassword");

                }

                ModelState.AddModelError("", "Invalid Email");
            }

			return View(model);
		}
        public IActionResult CompleteForgetPassword()
        {
            return View();
        }

        public IActionResult ResetPassword(string Email, string Token)
        {
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await usermanager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await usermanager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                        return RedirectToAction(nameof(SignIn));
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
            
                }

            }
            return View(model);

        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
