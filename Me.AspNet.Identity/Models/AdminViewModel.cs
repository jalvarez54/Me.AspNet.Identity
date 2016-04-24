using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Me.AspNet.Identity.Models
{
    ///// <summary>
    ///// [10000]
    ///// </summary>
    public class AdminRoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
    }
    
    public class AdminUserViewModel
    {
        public string Id { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool EmailConfirmed { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Pseudo")]
        public string Pseudo { get; set; }


        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
    // [10006] ADD: Show claims (user and admin mode)
    public class AdminUsersClaimsViewModel
    {
        public string Id { get; set; }

        public IList<System.Security.Claims.Claim> CurrentClaims { get; set; }

    }
}