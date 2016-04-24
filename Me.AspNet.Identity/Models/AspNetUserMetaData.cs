using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Me.AspNet.Identity.Models
{
    [MetadataType(typeof(AspNetUser.AspNetUserMetaData))]
    public partial class AspNetUser
    {
        internal sealed class AspNetUserMetaData
        {
            // Metadata classes are not meant to be instantiated.
            private AspNetUserMetaData()
            {
            }
            [DataType(DataType.Date)]
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
            [Display(Name = "Date anniversaire")]
            public Nullable<System.DateTime> BirthDate { get; set; }
        }

    }

}