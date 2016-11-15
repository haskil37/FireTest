using System.Web;
using System.Web.Optimization;

namespace FireTest
{
    public class BundleConfig
    {
        //Дополнительные сведения об объединении см. по адресу: http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.unobtrusive-ajax.js",
                        "~/Scripts/jquery.sortable.min.js"));            

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquerymask").Include(
                        "~/Scripts/jquery.inputmask.bundle.min.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/preload").Include(
                        "~/Scripts/queryloader.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/audio").Include(
                        "~/Scripts/audio.js"));
            bundles.Add(new StyleBundle("~/Content/audio").Include(
                        "~/Content/audio.css"));

            bundles.Add(new ScriptBundle("~/bundles/avatarUpload").Include(
                        "~/Scripts/avatar.js",
                        "~/Scripts/jquery.form.min.js",
                        "~/Scripts/jquery.Jcrop.min.js"));

            bundles.Add(new StyleBundle("~/Content/general").Include(
                      "~/Content/reset.css",
                      "~/Content/general.css",
                      "~/Content/qualification.css",
                      "~/Content/departments.css"));

            bundles.Add(new StyleBundle("~/Content/manage").Include(
                      "~/Content/manage.css"));

            bundles.Add(new StyleBundle("~/Content/battle").Include(
                      "~/Content/battle.css"));

            bundles.Add(new StyleBundle("~/Content/login").Include(
                      "~/Content/login.css"));
        }
    }
}
