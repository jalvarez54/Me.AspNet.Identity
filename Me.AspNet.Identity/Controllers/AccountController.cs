using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Me.AspNet.Identity.Models;
using System.Collections.Generic;
using System.IO;
using JA.Helpers;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Facebook;

namespace Me.AspNet.Identity.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // [10002] ADD: Login with name or email 
            var username = model.PseudoOrEmail;
            if (model.PseudoOrEmail.Contains("@"))
            {
                // Search UserName for that Email
                var userForEmail = await UserManager.FindByEmailAsync(model.PseudoOrEmail);
                if (userForEmail != null)
                {
                    username = userForEmail.UserName;
                }
                if(!userForEmail.EmailConfirmed)
                {
                    return View("NotRegistered");
                }
            }

            // Ceci ne comptabilise pas les échecs de connexion pour le verrouillage du compte
            // Pour que les échecs de mot de passe déclenchent le verrouillage du compte, utilisez shouldLockout: true
            // [10002] ADD: Login with name or email 
            var result = await SignInManager.PasswordSignInAsync(username, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Tentative de connexion non valide.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Nécessiter que l'utilisateur soit déjà connecté via un nom d'utilisateur/mot de passe ou une connexte externe
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Le code suivant protège des attaques par force brute contre les codes à 2 facteurs. 
            // Si un utilisateur entre des codes incorrects pendant un certain intervalle, le compte de cet utilisateur 
            // est alors verrouillé pendant une durée spécifiée. 
            // Vous pouvez configurer les paramètres de verrouillage du compte dans IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Code non valide.");
                    return View(model);
            }
        }
        public async Task<ActionResult> RestrictedUsersList()
        {
            //var Db = new ApplicationDbContext();
            ////var users = Db.Users.Where(u => u.EmailConfirmed == true);
            //var users = Db.Users;
            //var model = new List<RestrictedUsersListViewModel>();
            //foreach (var user in users)
            //{
            //    var u = new RestrictedUsersListViewModel(user);
            //    model.Add(u);
            //}
            //ViewBag.CountMembers = model.Count();
            //return View(model);
            var model = await UserManager.Users.OrderByDescending(m => m.RegistrationDate).ToListAsync();
            ViewBag.CountMembers = model.Count();

            return View(model);

        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // [10002] ADD: Login with name or email 
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Pseudo = model.UserName };
                user.RegistrationDate = DateTime.Now;
                // Add BlankPhoto.jpg for the new user
                user.AvatarUrl = Utils.SavePhotoFileToDisk(null, this, null, model.Avatar == null ? true : false);
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //- [10009] - ADD: Email confirmation in AccountController/Register
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // Pour plus d'informations sur l'activation de la confirmation du compte et la réinitialisation du mot de passe, consultez http://go.microsoft.com/fwlink/?LinkID=320771
                    // Envoyer un message électronique avec ce lien
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirmez votre compte", "Confirmez votre compte en cliquant <a href=\"" + callbackUrl + "\">ici</a>");

                    // Add user to role Member
                    result = await UserManager.AddToRoleAsync(user.Id, "Member");
                    result = await UserManager.AddToRoleAsync(user.Id, "CanEdit");

                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, code = code },
                        protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id,
                        "ACDF: Confirm your account",
                         //"Please confirm your account by clicking this link: <a href=\""
                         //+ callbackUrl + "\">link</a>"
                         "<h3> Bonjour, " + user.UserName + "</h3> " 
                         + "<p>Merci de nous rejoindre, confirmez votre compte en cliquant sur ce <a href=\"" + callbackUrl + "\">lien</a></p>"
                         + "<p>Ou copier et coller le lien ci-dessous dans le navigateur</p>"
                         + "<br />"
                         + callbackUrl
                        );

                    ViewBag.Link = callbackUrl;
                    return View("DisplayEmail");

                    //return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // - [10009] - ADD: Email confirmation in AccountController/Register
                //var user = await UserManager.FindByNameAsync(model.Email);
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Ne révélez pas que l'utilisateur n'existe pas ou qu'il n'est pas confirmé
                    return View("ForgotPasswordConfirmation");
                }

                // Pour plus d'informations sur l'activation de la confirmation du compte et la réinitialisation du mot de passe, consultez http://go.microsoft.com/fwlink/?LinkID=320771
                // Envoyer un message électronique avec ce lien
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Réinitialiser le mot de passe", "Réinitialisez votre mot de passe en cliquant <a href=\"" + callbackUrl + "\">ici</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id,
                    "ACDF: Reset Password",
                    //"Please reset your password by clicking here: <a href=\""
                    //+ callbackUrl + "\">link</a>"
                    "<h3> Bonjour, " + user.UserName + "</h3> "
                    + "<p>Réinitialiser votre mot de passe en cliquant ici : <a href=\"" + callbackUrl + "\">lien</a></p>"
                         + "<p>Ou copier et coller le lien ci-dessous dans le navigateur</p>"
                         + "<br />"
                         + callbackUrl
                    );

                ViewBag.Link = callbackUrl;
                return View("ForgotPasswordConfirmation");

            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // - [10009] - ADD: Email confirmation in AccountController/Register
            //var user = await UserManager.FindByNameAsync(model.Email);
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Ne révélez pas que l'utilisateur n'existe pas
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Demandez une redirection vers le fournisseur de connexions externe
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Générer le jeton et l'envoyer
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // - [10020] - BUG: With Facebook external login V2.4 cannot retreive email, public_profile
            if (loginInfo.Login.LoginProvider == "Facebook")
            {
                var identity = AuthenticationManager.GetExternalIdentity(DefaultAuthenticationTypes.ExternalCookie);
                var access_token = identity.FindFirstValue("urn:facebook:access_token");
                var fb = new FacebookClient(access_token);
                dynamic myInfo = fb.Get("/me?fields=email"); // specify the email field
                loginInfo.Email = myInfo.email;
            }

            // Connecter cet utilisateur à ce fournisseur de connexion externe si l'utilisateur possède déjà une connexion
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                   return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Si l'utilisateur n'a pas de compte, invitez alors celui-ci à créer un compte
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Obtenez des informations sur l’utilisateur auprès du fournisseur de connexions externe
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                // - [10007] - ADD: Social network picture
                ViewBag.LoginProvider = info.Login.LoginProvider;
                //if (model.Email == info.Email || info.Login.LoginProvider == "Twitter" || info.Login.LoginProvider == "Facebook")
                //{

                    var user = new ApplicationUser { Pseudo = info.DefaultUserName, UserName = info.DefaultUserName, Email = model.Email };
                    user.RegistrationDate = DateTime.Now;
                    user.AvatarUrl = "~/Content/Avatars/BlankPhoto.jpg";
                    if (info.Login.LoginProvider == "Google")
                    {
                        var imageJSON = info.ExternalIdentity.Claims.FirstOrDefault(c => c.Type.Equals("urn:google:image")).Value;
                        JObject o = JObject.Parse(imageJSON);
                        user.AvatarUrl = o["url"].ToString();
                        user.UseSocialNetworkPicture = true;
                    }
                    if (info.Login.LoginProvider == "Microsoft")
                    {
                        var microsoftAccountId = info.ExternalIdentity.Claims.FirstOrDefault(c => c.Type.Equals("urn:microsoftaccount:id")).Value;
                        user.AvatarUrl = string.Format("https://apis.live.net/v5.0/{0}/picture", microsoftAccountId);
                        user.UseSocialNetworkPicture = true;
                    }
                    if (info.Login.LoginProvider == "Facebook")
                    {
                        var facebookAccountId = info.ExternalIdentity.Claims.FirstOrDefault(c => c.Type.Equals("urn:facebook:id")).Value;
                        user.AvatarUrl = string.Format("http://graph.facebook.com/{0}/picture", facebookAccountId);
                        user.UseSocialNetworkPicture = true;
                    }
                    if (info.Login.LoginProvider == "Twitter")
                    {
                        var twitterScreenname = info.ExternalIdentity.Claims.FirstOrDefault(c => c.Type.Equals("urn:twitter:screenname")).Value;
                        user.AvatarUrl = string.Format("https://twitter.com/{0}/profile_image?size=original", twitterScreenname);
                        user.UseSocialNetworkPicture = true;
                    }
                    if (info.Login.LoginProvider == "GitHub")
                    {
                        var githubId = info.ExternalIdentity.Claims.FirstOrDefault(c => c.Type.Equals("urn:github:id")).Value;
                        user.AvatarUrl = string.Format("https://avatars.githubusercontent.com/u/{0}?v=3", githubId);
                        user.UseSocialNetworkPicture = true;
                    }

                    var result = await UserManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await UserManager.AddLoginAsync(user.Id, info.Login);
                        if (result.Succeeded)
                        {
                        // Add user to role Member
                        await UserManager.AddToRoleAsync(user.Id, "Member");
                        await UserManager.AddToRoleAsync(user.Id, "CanEdit");

                        await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);

                            const string ignoreClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";
                            foreach (var c in info.ExternalIdentity.Claims.Where(c => !c.Type.StartsWith(ignoreClaim)))
                            {
                                if (user.Claims.All(t => t.ClaimType != c.Type))
                                    await UserManager.AddClaimAsync(user.Id, c);
                            }
                            return RedirectToLocal(returnUrl);
                        }

                    }
                    AddErrors(result);
                //}
                //else
                //{
                //    var ir = new IdentityResult("Bad Email");
                //    AddErrors(ir);
                //}
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            // - [10016] - BUG: ASP.Net MVC 5 w/identity 2.2.0 Log off not working
            //AuthenticationManager.SignOut();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Applications auxiliaires

        // Utilisé(e) pour la protection XSRF lors de l'ajout de connexions externes
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("ChangeProfile", "Manage");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        // - [10004] ADD: User properties for profile.
        public enum EditMessageID
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
            Error,
        }
        #endregion
    }
}