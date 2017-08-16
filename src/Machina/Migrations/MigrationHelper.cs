using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Machina.Migrations
{
    public static class MigrationHelper
    {
        public static void WriteOutBoilerplate(IContent content, Property property)
        {
            Console.WriteLine($"Examining {content.Id} '{content.Name}'...");
            Console.WriteLine($"{property.Alias} => {property.Value}");
        }

        public static bool ShouldSkip(string propertyValue)
        {
            return string.IsNullOrEmpty(propertyValue) || propertyValue.Contains("umb://");
        }

        public static List<IContent> GetAllContent(IContentService contentService)
        {
            var allContent = contentService.GetRootContent().SelectMany(n => n.Descendants()).ToList();

            allContent.AddRange(contentService.GetRootContent());

            return allContent;
        }

        public static void SetBufferSize(int contentLength)
        {
            var minSize = 100;

            var proposedSize = contentLength * 5;

            if (proposedSize < minSize)
            {
                proposedSize = minSize;
            }

            Console.SetBufferSize(200, proposedSize);
        }

        public static List<IContent> FilterBy(this List<IContent> allContent, string[] args)
        {
            if (args.Length > 1)
            {
                if (args[1] == "0")
                {
                    return allContent;
                }

                Console.WriteLine($"Filtering for doctype: {args[1]}");

                return allContent.Where(x => x.ContentType.Alias.ToLower() == args[1].ToLower()).ToList();
            }

            return allContent;
        }

        public static bool ShouldPersist(string[] args)
        {
            return args.Any() && args[0] == "1";
        }
    }
}
