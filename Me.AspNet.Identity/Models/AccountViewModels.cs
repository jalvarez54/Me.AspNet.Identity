using Me.AspNet.Identity.CustomFiltersAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Web;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Me.AspNet.Identity.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Courrier électronique")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Mémoriser ce navigateur ?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Courrier électronique")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        //[Required]
        //[Display(Name = "Courrier électronique")]
        //[EmailAddress]
        //public string Email { get; set; }
        // [10002] ADD: Login with name or email 
        [Required]
        [Display(Name = "UserName or Email")]
        public string PseudoOrEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [Display(Name = "Mémoriser le mot de passe ?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        public Nullable<System.DateTime> RegistrationDate { get; set; }
        // Only to add BlankPhoto.jpg for the new user
        public HttpPostedFileWrapper Avatar { get; set; }
        public string AvatarUrl { get; set; }
        // ./Only to add BlankPhoto.jpg for the new user
        [Required]
        [EmailAddress]
        [Display(Name = "Courrier électronique")]
        public string Email { get; set; }
        // [10002] ADD: Login with name or email 
        [Required]
        //[RegularExpression(@"^\w{5,255}$", ErrorMessage = "Mauvais format. Le User Name ne doit pas comporter d'espaces ou de caractères accentués.")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "La chaîne {0} doit comporter au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe ")]
        [Compare("Password", ErrorMessage = "Le mot de passe et le mot de passe de confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Courrier électronique")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La chaîne {0} doit comporter au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Le nouveau mot de passe et le mot de passe de confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }

    public class RestrictedUsersListViewModel
    {
        public RestrictedUsersListViewModel(ApplicationUser user)
        {
            this.FirstName = user.FirstName;
            this.Pseudo = user.Pseudo;
            this.LastName = user.LastName;
            this.BirthDate = user.BirthDate;
            this.LastYearSchool = user.LastYearSchool;
            this.LastClass = user.LastClass;
            this.ActualCountry = user.ActualCountry;
            this.AvatarUrl = user.AvatarUrl;
        }
        public string Pseudo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public Nullable<System.DateTime> BirthDate { get; set; }
        public int LastYearSchool { get; set; }
        public string LastClass { get; set; }
        public string ActualCountry { get; set; }
        public string AvatarUrl { get; set; }
    }
}
