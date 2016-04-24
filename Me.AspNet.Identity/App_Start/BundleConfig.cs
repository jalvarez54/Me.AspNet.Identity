using System.Web;
using System.Web.Optimization;

namespace Me.AspNet.Identity
{
    public class BundleConfig
    {
        // Pour plus d'informations sur le regroupement, visitez http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/metisMenu.js",
                        "~/Scripts/metisMenu.acdf.js",
                        "~/Scripts/app/Me.AspNet.Identity.js"));

            bundles.Add(new ScriptBundle("~/bundles/fancybox").Include(
                        "~/Scripts/jquery.fancybox.pack.js",
                        "~/Scripts/jquery.mousewheel-{version}.pack.js",
                        "~/Scripts/jquery.fancybox-buttons.js",
                        "~/Scripts/jquery.fancybox-thumbs.js",
                        "~/Scripts/jquery.fancybox-media.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/flexslider").Include(
                        "~/Scripts/jquery.flexslider.js"));

            bundles.Add(new ScriptBundle("~/bundles/avatar").Include(
                        "~/Scripts/app/Me.AspNet.Identity.namespace.js",
                        "~/Scripts/app/cdf54.ja.utils.js",
                        "~/Scripts/app/Me.AspNet.Identity.avatar.js"));

            bundles.Add(new ScriptBundle("~/bundles/photo").Include(
                        "~/Scripts/app/Me.AspNet.Identity.namespace.js",
                        "~/Scripts/app/cdf54.ja.utils.js",
                        "~/Scripts/app/Me.AspNet.Identity.photo.js"));

            // Utilisez la version de développement de Modernizr pour le développement et l'apprentissage. Puis, une fois
            // prêt pour la production, utilisez l'outil de génération (bluid) sur http://modernizr.com pour choisir uniquement les tests dont vous avez besoin.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/tablesorter").Include(
            "~/Scripts/tablesorter/jquery.tablesorter.combined.js"
            //"~/Scripts/tablesorter/jquery.tablesorter.min.js",
            //"~/Scripts/tablesorter/jquery.tablesorter.pager.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/inputmask").Include(
                    "~/Scripts/jquery.inputmask/jquery.inputmask.js",
                    "~/Scripts/jquery.inputmask/jquery.inputmask.date.extensions.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-datepicker.css",
                    "~/Content/jquery.fancybox.css",
                    "~/Content/flexslider.css",
                    "~/Content/font-awesome.css",
                    //"~/Content/elastislidestyle.css",
                    //"~/Content/elastislide.css",
                    "~/Content/site.css"));
        }
    }
}
