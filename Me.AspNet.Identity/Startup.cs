using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Me.AspNet.Identity.Startup))]
namespace Me.AspNet.Identity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
