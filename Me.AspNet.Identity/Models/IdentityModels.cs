using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace Me.AspNet.Identity.Models
{
    // Vous pouvez ajouter des données de profil pour l'utilisateur en ajoutant plus de propriétés à votre classe ApplicationUser ; consultez http://go.microsoft.com/fwlink/?LinkID=317594 pour en savoir davantage.
    public class ApplicationUser : IdentityUser
    {
        // New properties added to extend Application User class:

        // - [10001] ADD: public string Pseudo { get; set; } in IdentityModels
        public string Pseudo { get; set; }
        // - [10004] ADD: User properties for profile.
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public int FirstYearSchool { get; set; }
        public int LastYearSchool { get; set; }
        public string LastClass { get; set; }
        public string ActualCity { get; set; }
        public string ActualCountry { get; set; }
        public string AvatarUrl { get; set; }
        [NotMapped]
        public HttpPostedFileWrapper Avatar { get; set; }
        // - [10005] - ADD: Gravatar
        public bool UseGravatar { get; set; }
        // - [10007] - ADD: Social network picture 
        public bool UseSocialNetworkPicture { get; set; }
        public Nullable<System.DateTime> RegistrationDate { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Notez qu'authenticationType doit correspondre à l'élément défini dans CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Ajouter les revendications personnalisées de l’utilisateur ici
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}