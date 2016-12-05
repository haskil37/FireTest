using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FireTest.Models
{
    public class SIC
    {
        public enum type
        {
            quote,
            img
        }
        public string SelectImagesCache(type type)
        {
            List<string> contentPaths = GetRelativePathsToRoot("~/Content/").ToList();
            string images = "";
            string start = "";
            string end = "";
            switch (type)
            {
                case type.quote:
                    start = "'";
                    end = "',";
                    break;
                case type.img:
                    start = "<img src=\"";
                    end = "\"/>";
                    break;
                default:
                    break;
            }
            foreach (string item in contentPaths)
            {
                images += start + item + end;
            }
            return images;

        }
        public string SelectImagesCache(string path)
        {
            List<string> contentPaths = GetRelativePathsToRoot("~/Content/").ToList();
            string images = "";

            foreach (string item in contentPaths)
            {
                images += "'" + item + "',";
            }
            return images;

        }
        private IEnumerable<string> GetRelativePathsToRoot(string virtualPath)
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".png" };
            var physicalPath = HttpContext.Current.Request.MapPath(virtualPath);

            var absolutePaths = Directory.EnumerateFiles(
                physicalPath,
                "*",
                SearchOption.AllDirectories).
                Where(x => extensions.Contains(Path.GetExtension(x)));
            return absolutePaths.Select(
                x => virtualPath.Replace("~", "") +
                x.Replace(physicalPath, "").Replace('\\', '/'));
        }
    }
}