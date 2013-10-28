using System.Web;
using System.Web.Optimization;

namespace smsSurvery.Surveryer
{
   public class BundleConfig
   {
      // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
      public static void RegisterBundles(BundleCollection bundles)
      {
         //DA lazy MB, lazy DA - move googleFastButtons and screwdefaultbuttonsV2 to a less general bundle
         bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                     "~/Scripts/jquery-{version}.js",
                     "~/Scripts/jquery-ui-1.10.3.js",
                     "~/Scripts/bootstrap.js",
                     "~/Scripts/underscore.js",
                     "~/Scripts/backbone.js",
                     "~/Scripts/chosen.jquery.js",
                     "~/Scripts/googleFastButtons.js",
                     "~/Scripts/jquery.screwdefaultbuttonsV2.js"
                     ));
         bundles.Add(new ScriptBundle("~/bundles/mobilesurvey").Include(            
            "~/Scripts/canvas.js",
            "~/MyScripts/SurveyUtilities.js",          
            "~/MyScripts/SurveyElements.js",
            "~/MyScripts/MobileSurvey.js",
            "~/MyScripts/SurveyNumericModal.js",
            "~/MyScripts/SurveyAnswersModal.js",
            "~/MyScripts/SurveyAlertsModal.js",
            "~/MyScripts/Question.js",
            "~/MyScripts/SurveyBuilder.js",
            "~/MyScripts/MobileSurveyMain.js"
            ));
         bundles.Add(new ScriptBundle("~/bundles/myscripts").Include(
                      "~/Scripts/bootstrap-fileupload.js",
                     "~/MyScripts/SurveyUtilities.js",                     
                     "~/MyScripts/SurveyElements.js",
                     "~/MyScripts/SurveyPreview.js",
                     "~/MyScripts/SurveyAnswersModal.js",
                     "~/MyScripts/SurveyNumericModal.js",
                     "~/MyScripts/SurveyAlertsModal.js",
                     "~/MyScripts/Question.js",
                     "~/MyScripts/SurveyRatingsModal.js",
                     "~/MyScripts/SurveyBuilder.js",
                     "~/MyScripts/SurveyMain.js"
            ));

         bundles.Add(new ScriptBundle("~/bundles/devices").Include(
                  "~/MyScripts/ManageDevices.js"
            ));
         bundles.Add(new ScriptBundle("~/bundles/referencescripts").Include(
                     "~/Scripts/jquery.tagsinput.js"));                     

         bundles.Add(new ScriptBundle("~/bundles/activityReports").Include(
                     "~/MyScripts/DateHelper.js",
                     "~/MyScripts/ReportsFilterArea.js",
                     "~/MyScripts/SurveyUtilities.js",
                     "~/MyScripts/reports.js"));
         
         bundles.Add(new ScriptBundle("~/bundles/comparisonReports").Include(
                     "~/MyScripts/DateHelper.js",
                     "~/MyScripts/ReportsFilterArea.js",
                     "~/MyScripts/reportsTags.js"));

         bundles.Add(new ScriptBundle("~/bundles/locationTags").Include(
            "~/MyScripts/SurveyUtilities.js",
            "~/MyScripts/Locations.js"));


         bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                     "~/Scripts/jquery-ui-{version}.js"));

         bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                     "~/Scripts/jquery.unobtrusive*",
                     "~/Scripts/jquery.validate*"));

         // Use the development version of Modernizr to develop with and learn from. Then, when you're
         // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
         bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                     "~/Scripts/modernizr-*"));

         bundles.Add(new StyleBundle("~/Content/css").Include(
            "~/Content/site.css",
            "~/Content/bootstrap.css",
            "~/Content/bootstrap-fileupload.css"));

         bundles.Add(new StyleBundle("~/bundles/SurveyTemplate").Include(
            "~/Content/SurveyTemplate/questionSet.css",            
            "~/Content/chosen/chosen.css"
            ));
         bundles.Add(new StyleBundle("~/bundles/SurveyResult").Include(
            "~/Content/SurveyResult/SurveyResult.css"));
         bundles.Add(new StyleBundle("~/bundles/MobileSurveyCss").Include(
            "~/Content/bootstrap.css",
            "~/Content/survey.css"
            ));
         bundles.Add(new StyleBundle("~/bundles/MobileSurveyCssTablet").Include(
           "~/Content/tablet/bootstrap.css",
           "~/Content/tablet/survey.css"
           ));

         bundles.Add(new StyleBundle("~/Content/custom").Include(     
            "~/Content/jquery-ui-1.8.23.custom.css",
            "~/Content/jquery.tagsinput.css"));

         bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                     "~/Content/themes/base/jquery.ui.core.css",
                     "~/Content/themes/base/jquery.ui.resizable.css",
                     "~/Content/themes/base/jquery.ui.selectable.css",
                     "~/Content/themes/base/jquery.ui.accordion.css",
                     "~/Content/themes/base/jquery.ui.autocomplete.css",
                     "~/Content/themes/base/jquery.ui.button.css",
                     "~/Content/themes/base/jquery.ui.dialog.css",
                     "~/Content/themes/base/jquery.ui.slider.css",
                     "~/Content/themes/base/jquery.ui.tabs.css",
                     "~/Content/themes/base/jquery.ui.datepicker.css",
                     "~/Content/themes/base/jquery.ui.progressbar.css",
                     "~/Content/themes/base/jquery.ui.theme.css"));
      }
   }
}