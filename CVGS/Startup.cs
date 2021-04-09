using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CVGS.Startup))]
namespace CVGS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
