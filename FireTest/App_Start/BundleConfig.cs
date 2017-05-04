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
                        "~/Scripts/jquery.sortable.min.js",
                        "~/Scripts/zoomify.min.js"));

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

            bundles.Add(new ScriptBundle("~/bundles/datepicker").Include(
                        "~/Scripts/bootstrap-datepicker.min.js",
                        "~/Scripts/locales/bootstrap-datepicker.ru.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/calendar").Include(
                        "~/Scripts/moment.min.js",
                        "~/Scripts/fullcalendar.min.js",
                        "~/Scripts/locale/ru.js"));

            bundles.Add(new ScriptBundle("~/bundles/statistic").Include(
                        "~/Scripts/Chart.min.js"));

            bundles.Add(new StyleBundle("~/Content/general").Include(
                      "~/Content/reset.css",
                      "~/Content/general.css",
                      "~/Content/qualification.css",
                      "~/Content/departments.css"));

            bundles.Add(new StyleBundle("~/Content/manage").Include(
                      "~/Content/manage.css"));

            bundles.Add(new StyleBundle("~/Content/battle").Include(
                      "~/Content/battle.css"));

            bundles.Add(new StyleBundle("~/Content/administrator").Include(
                      "~/Content/PagedList.css",
                      "~/Content/bootstrap-datepicker3.css",
                      "~/Content/administrator.css"));

            bundles.Add(new StyleBundle("~/Content/statistic").Include(
                      "~/Content/statistic.css"));

            bundles.Add(new StyleBundle("~/Content/login").Include(
                      "~/Content/login.css"));

            bundles.Add(new StyleBundle("~/Content/calendar").Include(
                     "~/Content/fullcalendar.min.css",
                     "~/Content/calendar.css"));            
        }
    }
}
