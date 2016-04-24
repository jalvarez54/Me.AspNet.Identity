using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Me.AspNet.Identity.Models;
using JA.Helpers;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Me.AspNet.Identity.Controllers
{

    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Votre mot de passe a été changé."
                : message == ManageMessageId.SetPasswordSuccess ? "Votre mot de passe a été défini."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Votre fournisseur d'authentification à 2 facteurs a été défini."
                : message == ManageMessageId.Error ? "Une erreur s'est produite."
                : message == ManageMessageId.AddPhoneSuccess ? "Votre numéro de téléphone a été ajouté."
                : message == ManageMessageId.RemovePhoneSuccess ? "Votre numéro de téléphone a été supprimé."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Générer le jeton et l'envoyer
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Votre code de sécurité est : " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Envoyer un SMS via le fournisseur SMS afin de vérifier le numéro de téléphone
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            //Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
            ModelState.AddModelError("", "La vérification du téléphone a échoué");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            //Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "La connexion externe a été supprimée."
                : message == ManageMessageId.Error ? "Une erreur s'est produite."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Demander une redirection vers le fournisseur de connexion externe afin de lier une connexion pour l'utilisateur actuel
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        // GET: /Manage/ChangeProfile
        public ActionResult ChangeProfile(ManageMessageId? message = null)
        {
            ViewBag.StatusMessage =
                    message == ManageMessageId.ModifSuccess ? "Vos modifications ont été prises en compte."
                    : message == ManageMessageId.Error ? "Une erreur s'est produite."
                    : "";

            var user = UserManager.Users.First(u => u.UserName == User.Identity.Name);

            // Dont let user edit another user if not Admin
            if ((user.UserName != User.Identity.Name) && (!User.IsInRole("Admin")))
            {
                ModelState.AddModelError("", "You are not" + user.UserName);
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
            }
            var model = new ChangeProfileViewModel(user);
            model.Pseudo = user.Pseudo;
            // [10028] ADD: UserName (ReadOnly) in Manage/ChangeProfile
            model.UserName = user.UserName;
            // [10028]
            model.Email = user.Email;

            return View(model);
        }
        //
        // POST: /Manage/ChangeProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeProfile(ChangeProfileViewModel model)
        {

            var Db = new ApplicationDbContext();
            var user = Db.Users.First(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                // Update the user data:
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Pseudo = model.Pseudo;
                user.Email = model.Email;
                if(model.BirthDateYear != 1900)
                    user.BirthDate = new DateTime(model.BirthDateYear, model.BirthDateMonth, model.BirthDateDay);
                user.FirstYearSchool = model.FirstYearSchool;
                user.LastYearSchool = model.LastYearSchool;
                user.LastClass = model.LastClass;
                user.ActualCity = model.ActualCity;
                user.ActualCountry = model.ActualCountry;
                Db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                await Db.SaveChangesAsync();
                return RedirectToAction("ChangeProfile", new { Message = ManageMessageId.ModifSuccess });
            }
            //// Generate Years
            List<SelectListItem> years = new List<SelectListItem>();
            for (int y = 1900; y <= DateTime.Now.Year; y++)
            {
                years.Add(new SelectListItem { Value = y.ToString(), Text = y.ToString() });
            };
            model.YearFirst = (IEnumerable<SelectListItem>)years;
            model.YearLast = (IEnumerable<SelectListItem>)years;
            model.Year = (IEnumerable<SelectListItem>)years;
            //// Generate Months
            List<System.Web.Mvc.SelectListItem> months = new List<System.Web.Mvc.SelectListItem>();
            for (int y = 1; y <= 12; y++)
            {
                months.Add(new System.Web.Mvc.SelectListItem { Value = y.ToString(), Text = y.ToString() });
            };
            model.Month = (IEnumerable<System.Web.Mvc.SelectListItem>)months;
            //// Generate Days
            List<System.Web.Mvc.SelectListItem> days = new List<System.Web.Mvc.SelectListItem>();
            for (int y = 1; y <= 31; y++)
            {
                days.Add(new System.Web.Mvc.SelectListItem { Value = y.ToString(), Text = y.ToString() });
            };
            model.Day = (IEnumerable<System.Web.Mvc.SelectListItem>)days;
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Manage/ChangePhoto
        public ActionResult ChangePhoto(ManageMessageId? message = null)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ModifSuccess ? "Your photo has been changed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.NoChange ? "No change have been made."
                : message == ManageMessageId.NoExternalProvider ? "No external login provider found."
                : "";

            var user = UserManager.Users.First(u => u.UserName == User.Identity.Name);

            var model = new ChangePhotoViewModel();
            if (user.AvatarUrl == null)
            {
                model.AvatarUrl = System.IO.Path.Combine(HttpRuntime.AppDomainAppVirtualPath, @"Content/Avatars", @"BlankPhoto.jpg");
            }
            else
            {
                model.AvatarUrl = user.AvatarUrl;
            }
            model.UseGravatar = user.UseGravatar;
            model.Email = user.Email;
            //[10021]
            model.Pseudo = user.Pseudo;

            //[10019] Use provider avatar by default for external login ADD: this function in Profile Change photo.
            model.UseSocialNetworkPicture = user.UseSocialNetworkPicture;
            var owc = Request.GetOwinContext();
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            if (linkedAccounts.Count != 0)
            {
                model.ExternalProvider = linkedAccounts[0].LoginProvider;

                if (model.ExternalProvider == "Google")
                {
                    var imageJSON = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:google:image")).Value;
                    JObject o = JObject.Parse(imageJSON);
                    model.ParameterProvider = o["url"].ToString();
                }
               if (model.ExternalProvider == "Microsoft")
                {
                    var microsoftAccountId = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:microsoftaccount:id")).Value;
                    model.ParameterProvider = microsoftAccountId;
                }
                if (model.ExternalProvider == "Facebook")
                {
                    var facebookAccountId = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:facebook:id")).Value;
                    model.ParameterProvider = facebookAccountId;
                }
                if (model.ExternalProvider == "Twitter")
                {
                    var twitterScreenname = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:twitter:screenname")).Value;
                    model.ParameterProvider = twitterScreenname;
                }
                //[10026] ADD: Github for external login
                if (model.ExternalProvider == "GitHub")
                {
                    var githibId = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:github:id")).Value;
                    model.ParameterProvider = githibId;
                }
                //[10026]
            }
            //[10019]

            return View(model);
        }
        //
        // POST: /Manage/ChangePhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePhoto(ChangePhotoViewModel model)
        {
            var user = UserManager.Users.First(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                // No change
                if (model.Avatar == null && !model.IsNoPhotoChecked && model.UseGravatar == user.UseGravatar && model.UseSocialNetworkPicture == user.UseSocialNetworkPicture)
                {
                    return RedirectToAction("ChangePhoto", new { Message = ManageMessageId.NoChange });
                }
                // Delete photo if photo exist and is not a gravatar or socialnetworkPicture.
                if (user.AvatarUrl != null)
                {
                    if (!(user.AvatarUrl.Contains("http://") || user.AvatarUrl.Contains("https://")))
                    {
                        // Delete file if not the BlankPhoto.jpg and if we change for a gravatar or socialnetworkpicture or new photo
                        if (!user.AvatarUrl.Contains("BlankPhoto.jpg"))
                        {
                            if (model.IsNoPhotoChecked || model.UseGravatar || model.UseSocialNetworkPicture || model.Avatar != null)
                            {
                                string fileToDelete = Path.GetFileName(user.AvatarUrl);

                                var path = Path.Combine(Server.MapPath("~/Content/Avatars"), fileToDelete);
                                FileInfo fi = new FileInfo(path);
                                if (fi.Exists)
                                    fi.Delete();
                            }
                        }
                    }
                }
                if (model.IsNoPhotoChecked)
                {
                    //string vPath = HttpRuntime.AppDomainAppVirtualPath;
                    //var path = Path.Combine(vPath, "/Content/Avatars/BlankPhoto.jpg");
                    // [10015] PB: Migration files VS dev env
                    string vPath = Utils.AppPath();
                    var path = vPath + "/Content/Avatars/BlankPhoto.jpg";
                    // [10015]
                    model.AvatarUrl = path;
                }
                else
                    // Save on disk only uploaded picture.
                    if (model.Avatar != null)
                {
                    model.AvatarUrl = Utils.SavePhotoFileToDisk(model.Avatar, this, user.AvatarUrl, false);
                }
                if (model.UseGravatar == true)
                {
                    model.AvatarUrl = JA.Helpers.Utils.GetGravatarUrlForAddress(user.Email);
                }
                //[10019] Use provider avatar by default for external login ADD: this function in Profile Change photo.
                if (model.UseSocialNetworkPicture == true)
                {
                    var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
                    var owc = Request.GetOwinContext();

                    if (linkedAccounts.Count == 0)
                    {
                        return RedirectToAction("ChangePhoto", new { Message = ManageMessageId.NoExternalProvider });
                    }

                    model.ExternalProvider = linkedAccounts[0].LoginProvider;

                    if (model.ExternalProvider == "Google")
                    {
                        var imageJSON = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:google:image")).Value;
                        JObject o = JObject.Parse(imageJSON);
                        model.AvatarUrl = o["url"].ToString();
                    }
                    if (model.ExternalProvider == "Microsoft")
                    {
                        var microsoftAccountId = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:microsoftaccount:id")).Value;
                        model.AvatarUrl = string.Format("https://apis.live.net/v5.0/{0}/picture", microsoftAccountId);
                    }
                    if (model.ExternalProvider == "Facebook")
                    {
                        var facebookAccountId = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:facebook:id")).Value;
                        model.AvatarUrl = string.Format("http://graph.facebook.com/{0}/picture", facebookAccountId);
                    }
                    if (model.ExternalProvider == "Twitter")
                    {
                        var twitterScreenname = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:twitter:screenname")).Value;
                        model.AvatarUrl = string.Format("https://twitter.com/{0}/profile_image?size=original", twitterScreenname);
                    }
                    if (model.ExternalProvider == "GitHub")
                    {
                        var githubId = owc.Authentication.User.Claims.FirstOrDefault(c => c.Type.Equals("urn:github:id")).Value;
                        model.AvatarUrl = string.Format("https://avatars.githubusercontent.com/u/{0}?v=3", githubId);
                    }
                }
                //[10019]

                user.AvatarUrl = model.AvatarUrl;
                user.UseGravatar = model.UseGravatar;
                user.UseSocialNetworkPicture = model.UseSocialNetworkPicture;
                await UserManager.UpdateAsync(user);

                return RedirectToAction("ChangePhoto", new { Message = ManageMessageId.ModifSuccess });
            }
            model.AvatarUrl = user.AvatarUrl;

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> RemoveAccount(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("RemoveAccount")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveAccountConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Remove photo if exist before remove account
                //if Photo exist and is not a gravatar
                if ((user.AvatarUrl != null && !user.AvatarUrl.Contains("http://") && !user.AvatarUrl.Contains("https://")))
                {
                    // Delete file if not the BlankPhoto.jpg and if we change for a gravatar
                    if (!user.AvatarUrl.Contains("BlankPhoto.jpg"))
                    {
                        string fileToDelete = Path.GetFileName(user.AvatarUrl);

                        var path = Path.Combine(Server.MapPath("~/Content/Avatars"), fileToDelete);
                        FileInfo fi = new FileInfo(path);
                        if (fi.Exists)
                            fi.Delete();
                    }
                }
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }

                AuthenticationManager.SignOut();
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //[10031] ADD: webcam function for webcamjs with BUG !!!!
        [HttpPost]
        public async Task<JsonResult> Upload()
        {
            HttpPostedFileBase photo = Request.Files["webcam"];

            var user = UserManager.Users.First(u => u.UserName == User.Identity.Name);

            user.AvatarUrl = Utils.SavePhotoFileToDisk(photo, this, user.AvatarUrl, false);
            user.UseGravatar = false;
            user.UseSocialNetworkPicture = false;

            await UserManager.UpdateAsync(user);


            return Json(new { success = true });

        }
        //[10031]

        //[10031] ADD: webcam function for jquery.webcam.js
        [HttpPost]
        public async Task Capture()
        {

            var user = UserManager.Users.First(u => u.UserName == User.Identity.Name);

            var stream = Request.InputStream;
            string dump;

            using (var reader = new System.IO.StreamReader(stream))
                dump = reader.ReadToEnd();

            user.AvatarUrl = Utils.SaveBytesPhotoFileToDisk(dump, this, user.AvatarUrl, false);
            user.UseGravatar = false;
            user.UseSocialNetworkPicture = false;

            await UserManager.UpdateAsync(user);

        }
        [HttpPost]
        public JsonResult Rebind()
        {
            return Json(new { success = true });
        }
        //[10031]

        //- [10014] - ADD: Show claims feature
        // GET: /Manage/GetClaims
        public async Task<ActionResult> GetUserClaims()
        {
            //Version: Mine (show stored claims)
            var model = await UserManager.GetClaimsAsync(User.Identity.GetUserId());
            //var model = new UserClaimsViewModel() { CurrentClaims = userClaims };
            return View(model);
        }

        //- [10014] - ADD: Show claims feature
        // GET: /Manage/GetClaims
        public ActionResult GetUserIdentityClaims()
        {
            //Version: http://www.apress.com/files/extra/ASP_NET_Identity_Chapters.pdf (show identity claims)
            System.Security.Claims.ClaimsIdentity ident = HttpContext.User.Identity as System.Security.Claims.ClaimsIdentity;
            if (ident == null)
            {
                return View("Error", new string[] { "No claims available" });
            }
            else
            {
                return View(ident.Claims);
            }
        }

        #region Programmes d'assistance
        // Utilisé pour la protection XSRF lors de l'ajout de connexions externes
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            NoChange,
            NoExternalProvider,
            ModifSuccess,
            AddPhoneSuccess,
            ChangePhotoSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}