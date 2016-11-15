using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FireTest.Startup))]
namespace FireTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
