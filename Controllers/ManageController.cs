﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarterKit.Models;
using StarterKit.Models.ManageViewModels;
using StarterKit.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using StarterKit.Repository;

namespace StarterKit.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly UrlEncoder _urlEncoder;
        private readonly IImageStorageService _imageService;

        private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public ManageController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IEmailSender emailSender,
          ILogger<ManageController> logger,
          UrlEncoder urlEncoder,
          IImageStorageService imageService)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _urlEncoder = urlEncoder;
            _imageService = imageService;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByIdAsync(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Ok(new Profile {
                UserGuid = user.UserGuid,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEmailConfirmed = user.EmailConfirmed,
                ImageThumbnailUrl = user.ImageThumbnailUrl,
                ImageUrl = user.ImageUrl,
                StatusMessage = StatusMessage
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = user.Email;
            if (model.Email != email)
            {
                user.Email = model.Email;
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var firstName = user.FirstName;
            if (model.FirstName != firstName)
            {
                user.FirstName = model.FirstName;
                var setNameResult = await _userManager.UpdateAsync(user);
                if (!setNameResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting first name for user with ID '{user.Id}'.");
                }
            }

            var lastName = user.LastName;
            if (model.LastName != lastName)
            {
                user.LastName = model.LastName;
                var setNameResult = await _userManager.UpdateAsync(user);
                if (!setNameResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting last name for user with ID '{user.Id}'.");
                }
            }

            var imageUrl = Path.GetFileName(user.ImageUrl);
            if (model.ImageUrl.FileName != imageUrl)
            {
                user.ImageUrl = await Upload(model.ImageUrl);
                var setNameResult = await _userManager.UpdateAsync(user);
                if (!setNameResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting Image Url for user with ID '{user.Id}'.");
                }
            }

            var imageThumbnailUrl = Path.GetFileName(user.ImageThumbnailUrl);
            if (model.ImageThumbnailUrl.FileName != imageUrl)
            {
                user.ImageThumbnailUrl = await Upload(model.ImageThumbnailUrl);
                var setNameResult = await _userManager.UpdateAsync(user);
                if (!setNameResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting Image Url for user with ID '{user.Id}'.");
                }
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToAction(nameof(Index));
        }

        public async Task<string> Upload(IFormFile model)
        {
            if (model != null)
            {
                using (var stream = new MemoryStream())
                {
                    model.CopyTo(stream);
                    var fileBytes = stream.ToArray();
                    var op = _imageService.StoreProfile(model.FileName, fileBytes);

                    op.Wait();
                    if (!op.IsCompletedSuccessfully) throw op.Exception;

                    return await op;
                }
            }

            return "";
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var user = await _userManager.FindByIdAsync(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var userClaims = _userManager.GetClaimsAsync(user);
            if (userClaims.Result.Any(x => x.Type == ClaimTypes.Role && x.Value == "Admin"))
            {
                var users = await _userManager.GetUsersForClaimAsync(new Claim(ClaimTypes.Role, "User"));
                return Ok(users.Select(x => new Profile
                {
                    UserGuid = x.UserGuid,
                    Username = x.UserName,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    IsEmailConfirmed = x.EmailConfirmed,
                    ImageThumbnailUrl = x.ImageThumbnailUrl,
                    ImageUrl = x.ImageUrl,
                    StatusMessage = StatusMessage
                }).ToList());

            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendVerificationEmail(Profile model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value); ;
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
            var email = user.Email;
            //await _emailSender.SendEmailConfirmationAsync(email, callbackUrl, $"{user.FirstName} {user.LastName}");

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenicatorUriFormat,
                _urlEncoder.Encode("StarterKit"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        #endregion
    }
}