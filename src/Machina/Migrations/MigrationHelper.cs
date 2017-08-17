using System;
using System.Collections.Generic;
using System.Linq;
using Chauffeur;
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

        public static List<IContent> FilterBy(this List<IContent> allContent, MigrationCliInput cliInput)
        {
            if (!string.IsNullOrEmpty(cliInput.FilterBy))
            {
                Console.WriteLine($"Filtering for doctype: {cliInput.FilterBy}");

                return allContent.Where(x => x.ContentType.Alias.ToLower() == cliInput.FilterBy.ToLower()).ToList();
            }

            return allContent;
        }

        public static MigrationCliInput ParseCliArgs(string[] args)
        {
            var argsAsDictionary = _argsAsDictionary(args);

            var input = new MigrationCliInput
            {
                ShouldPersist = argsAsDictionary.ContainsKey("-p") && argsAsDictionary["-p"] == "1"
            };

            if (argsAsDictionary.ContainsKey("-f"))
            {
                input.FilterBy = argsAsDictionary["-f"];
            }

            if (argsAsDictionary.ContainsKey("-dtpa"))
            {
                input.NestedContentDocTypePropertyAlias = argsAsDictionary["-dtpa"];
            }

            if (argsAsDictionary.ContainsKey("-udi"))
            {
                var serviceName = argsAsDictionary["-udi"];

                if (string.IsNullOrEmpty(serviceName) || (serviceName != "media" && serviceName != "content"))
                {
                    
                }
                else
                {
                    input.UdiServiceName = serviceName.ToLower();
                }
            }

            return input;
        }

        public static Dictionary<string, string> _argsAsDictionary(string[] args)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                var splitValues = arg.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);

                //only valid for <key>:<value>
                if (splitValues.Length == 2)
                {
                    if (!dictionary.ContainsKey(splitValues[0]))
                    {
                        dictionary.Add(splitValues[0], splitValues[1]);
                    }
                }
            }

            return dictionary;
        }
    }
}
