using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Me.AspNet.Identity.CustomFiltersAttributes;
using System.Web;
using System.Globalization;
using System.Linq;

namespace Me.AspNet.Identity.Models
{
    //- [10006] - ADD: Social network login
    public class ChangePhotoViewModel
    {
        public string AvatarUrl { get; set; }
        [Display(Name = "Change")]
        // - [10011] - ADD: [ValidateFile] in Models/ManageViewModels for HttpPostedFileWrapper Avatar
        [ValidateFile]
        public HttpPostedFileWrapper Avatar { get; set; }
        [Display(Name = "Remove if checked")]
        public bool IsNoPhotoChecked { get; set; }
        [Display(Name = "Use my Gravatar")]
        public bool UseGravatar { get; set; }
        public string Email { get; set; }
        public string Pseudo { get; set; }
        //- [10007] - ADD: Social network picture
        // Use provider avatar by default for external login ADD: this function in Profile Change photo. 
        [Display(Name = "Use my Social Network Picture")]
        public bool UseSocialNetworkPicture { get; set; }
        public string ExternalProvider { get; set; }
        public string ParameterProvider { get; set; }
        //- [10007]
    }
    //- [10006]
    public class ChangeProfileViewModel
    {
        // - [10012] - ADD: Countries dropdownlist in in Models/AccountViewModels for EditProfileViewModel
        // GetCountries() method
        private IEnumerable<System.Web.Mvc.SelectListItem> GetCountries()
        {
            RegionInfo country = new RegionInfo(new CultureInfo("fr-FR", false).LCID);
            List<System.Web.Mvc.SelectListItem> countryNames = new List<System.Web.Mvc.SelectListItem>();

            //To get the Country Names from the CultureInfo installed in windows
            foreach (CultureInfo cul in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                country = new RegionInfo(new CultureInfo(cul.Name, false).LCID);
                countryNames.Add(new System.Web.Mvc.SelectListItem() { Text = country.DisplayName, Value = country.DisplayName });
            }
            //Assigning all Country names to IEnumerable
            IEnumerable<System.Web.Mvc.SelectListItem> nameAdded = countryNames.GroupBy(x => x.Text).Select(x => x.FirstOrDefault()).ToList<System.Web.Mvc.SelectListItem>().OrderBy(x => x.Text);
            return nameAdded;
        }
        // - [10012]
        public ChangeProfileViewModel() { }

        // Allow Initialization with an instance of ApplicationUser:
        public ChangeProfileViewModel(ApplicationUser user)
        {
            this.UserName = user.UserName;
            this.Pseudo = user.Pseudo;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Email = user.Email;
            this.BirthDateYear = user.BirthDate.HasValue ? user.BirthDate.Value.Year : 0;
            this.BirthDateMonth = user.BirthDate.HasValue ? user.BirthDate.Value.Month : 0;
            this.BirthDateDay = user.BirthDate.HasValue ? user.BirthDate.Value.Day : 0;
            this.BirthDate = user.BirthDate;
            this.FirstYearSchool = user.FirstYearSchool;
            this.LastYearSchool = user.LastYearSchool;
            this.LastClass = user.LastClass;
            this.ActualCity = user.ActualCity;
            this.ActualCountry = user.ActualCountry;
            this.AvatarUrl = user.AvatarUrl;
            //// Generate Years
            List<System.Web.Mvc.SelectListItem> years = new List<System.Web.Mvc.SelectListItem>();
            for (int y = 1900; y <= DateTime.Now.Year; y++)
            {
                years.Add(new System.Web.Mvc.SelectListItem { Value = y.ToString(), Text = y.ToString() });
            };
            //// Generate Months
            List<System.Web.Mvc.SelectListItem> months = new List<System.Web.Mvc.SelectListItem>();
            for (int y = 1; y <= 12; y++)
            {
                months.Add(new System.Web.Mvc.SelectListItem { Value = y.ToString(), Text = y.ToString() });
            };
            //// Generate Days
            List<System.Web.Mvc.SelectListItem> days = new List<System.Web.Mvc.SelectListItem>();
            for (int y = 1; y <= 31; y++)
            {
                days.Add(new System.Web.Mvc.SelectListItem { Value = y.ToString(), Text = y.ToString() });
            };
            this.YearFirst = (IEnumerable<System.Web.Mvc.SelectListItem>)years;
            this.YearLast = (IEnumerable<System.Web.Mvc.SelectListItem>)years;
            this.Year = (IEnumerable<System.Web.Mvc.SelectListItem>)years;
            this.Month = (IEnumerable<System.Web.Mvc.SelectListItem>)months;
            this.Day = (IEnumerable<System.Web.Mvc.SelectListItem>)days;
            this.RegistrationDate = user.RegistrationDate;
            // - [10012] - ADD: Countries dropdownlist in in Models/AccountViewModels for EditProfileViewModel
            this.Countries = GetCountries();
        }
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [RegularExpression(@"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,3})$", ErrorMessage = "Your email address is not in a valid format. Example of correct format: joe.example@example.org")]
        public string Email { get; set; }
        [Display(Name = "Pseudo")]
        public string Pseudo { get; set; }
        [Display(Name = "Prénom")]
        public string FirstName { get; set; }
        [Display(Name = "Nom")]
        public string LastName { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Date naissance")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public Nullable<System.DateTime> BirthDate { get; set; }
        public int BirthDateDay { get; set; }
        public int BirthDateMonth { get; set; }
        public int BirthDateYear { get; set; }
        [Display(Name = "Première année à l'école")]
        public int FirstYearSchool { get; set; }
        [Display(Name = "Dernière année à l'école")]
        public int LastYearSchool { get; set; }
        [Display(Name = "Dernière classe")]
        public string LastClass { get; set; }
        [Display(Name = "Ville actuelle")]
        public string ActualCity { get; set; }
        [Display(Name = "Pays actuel")]
        public string ActualCountry { get; set; }
        public string AvatarUrl { get; set; }
        [Display(Name = "Date enregistrement")]
        public Nullable<System.DateTime> RegistrationDate { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Day { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Month { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Year { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> YearFirst { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> YearLast { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Countries { get; set; }

        // Return a pre-populated instance of AppliationUser:
        public ApplicationUser GetUser()
        {
            var user = new ApplicationUser()
            {
                UserName = this.UserName,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                BirthDate = this.BirthDate,
                FirstYearSchool = this.FirstYearSchool,
                LastYearSchool = this.LastYearSchool,
                LastClass = this.LastClass,
                ActualCity = this.ActualCity,
                ActualCountry = this.ActualCountry,
                AvatarUrl = this.AvatarUrl,
                RegistrationDate = this.RegistrationDate,
            };
            return user;
        }

    }

    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Le {0} doit compter au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [Compare("NewPassword", ErrorMessage = "Le nouveau mot de passe et le mot de passe de confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Le {0} doit compter au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [Compare("NewPassword", ErrorMessage = "Le nouveau mot de passe et le mot de passe de confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Numéro de téléphone")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Numéro de téléphone")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}