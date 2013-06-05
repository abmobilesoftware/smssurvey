using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
namespace smsSurvery.Surveryer.Utilities
{
   public static class ViewExtensionMethods
   {

      public static MvcHtmlString ImageLink(this UrlHelper helper, string imageUrl, string altText, string actionName, string controllerName, string title, object routeValues, string imgID = null)
      {
         var imgTag = new TagBuilder("img");
         imgTag.MergeAttribute("src", imageUrl);
         imgTag.MergeAttribute("alt", altText);
         imgTag.MergeAttribute("title", title);
         if (!String.IsNullOrEmpty(imgID))
         {
            imgTag.GenerateId(imgID);
         }

         var link = helper.Action(actionName, controllerName);
         TagBuilder imglink = new TagBuilder("a");
         imglink.MergeAttribute("href", link.ToString());
         imglink.InnerHtml = imgTag.ToString();
         return new MvcHtmlString(imglink.ToString()); ;
      }      

      public static MvcHtmlString UpdatedResourceLink(this UrlHelper helper, string resourceURL)
      {
         //Dragos: convention - we always look at the "source" file and not at the minified file -> we disregard the /Minified folder from the path        
         var link = helper.Content(resourceURL);
         var originalFile = resourceURL.Replace("/Minified", "");
         try
         {
            var fileLink = HttpContext.Current.Server.MapPath(originalFile);
            var file = File.GetLastWriteTime(fileLink);
            var fileTimestamp = file.ToString("yyyyMMddHHmmss");

            var strRegex = @"(.*)(\.css|\.js)$";
            RegexOptions myRegexOptions = RegexOptions.None;
            Regex myRegex = new Regex(strRegex, myRegexOptions);
            var matches = myRegex.Matches(link);
            if (matches.Count == 1)
            {
               var match = matches[0];
               link = match.Groups[1].Value + "." + fileTimestamp + match.Groups[2];
            }
         }
         catch (Exception ex)
         {
            //TODO we should log this somewhere
         }
         return new MvcHtmlString(link);
      }

      public static bool IsReleaseBuild(this HtmlHelper helper)
      {
#if DEBUG
         return false;
#else
    return true;
#endif
      }
   }
}