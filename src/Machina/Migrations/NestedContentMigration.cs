using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chauffeur;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Machina.Migrations
{
    [DeliverableName("migrate-nested-content")]
    [DeliverableAlias("mnc")]
    public class NestedContentMigration : Deliverable
    {
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;

        private static readonly string _newContentTypeAlias = "Our.Umbraco.NestedContent";

        public NestedContentMigration(
            TextReader reader,
            TextWriter writer,
            IContentService contentService,
            IMediaService mediaService
        ) : base(reader, writer)
        {
            _contentService = contentService;
            _mediaService = mediaService;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            var cliInput = MigrationHelper.ParseCliArgs(args);

            var shouldPersist = cliInput.ShouldPersist;

            Console.WriteLine("Migrating NestedContent properties to UDI...");

            if (shouldPersist)
            {
                Console.WriteLine("Persisting!");
            }
            else
            {
                Console.WriteLine("Previewing, re-run with '1' as the first arg to persist.");
            }


            if (string.IsNullOrEmpty(cliInput.NestedContentDocTypePropertyAlias))
            {
                Console.WriteLine("You need specify a doctype property alias!");
                
                return DeliverableResponse.Continue;
            }

            if (string.IsNullOrEmpty(cliInput.UdiServiceName))
            {
                Console.WriteLine("You need specify a service type for the UDI (media or content)!");

                return DeliverableResponse.Continue;
            }

            Console.WriteLine($"Using {cliInput.UdiServiceName} service for UDI's!");

            var allContent = MigrationHelper.GetAllContent(_contentService).FilterBy(cliInput);

            MigrationHelper.SetBufferSize(allContent.Count);

            foreach (var content in allContent)
            {
                foreach (var property in content.Properties.Where(x => x.PropertyType.PropertyEditorAlias == _newContentTypeAlias))
                {
                    if (property.Value != null)
                    {
                        var propertyValueString = property.Value.ToString();

                        Console.WriteLine($"raw =>{propertyValueString}");

                        if (MigrationHelper.ShouldSkip(propertyValueString))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;

                            MigrationHelper.WriteOutBoilerplate(content, property);

                            Console.WriteLine("Skipping...");
                            Console.ResetColor();
                            continue;
                        }

                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;

                            var pattern = @"\""" + cliInput.NestedContentDocTypePropertyAlias + @"\"":\s?\""?(\d+,?)*\""?";

                            var matches = Regex.Matches(propertyValueString, pattern);

                            if (matches.Count > 0)
                            {
                                MigrationHelper.WriteOutBoilerplate(content, property);

                                Console.WriteLine($"Matches: {matches.Count}");

                                //a match is a single nested content json object
                                foreach (Match match in matches)
                                {
                                    Console.ForegroundColor = ConsoleColor.Magenta;

                                    Console.WriteLine($"Handling match: {match.Value}");

                                    var count = 0;

                                    foreach (Group group in match.Groups)
                                    {
                                        Console.WriteLine($"{count} => {group.Value}");
                                        count++;
                                    }

                                    //handle in case there are more than one id's to convert
                                    var csvValues = match.Groups[1].ToString().Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

                                    var udiList = new List<string>();

                                    foreach (var id in csvValues)
                                    {
                                        var referencedId = 0;

                                        Console.WriteLine($"Trying to parse: {id}");

                                        if (int.TryParse(id, out referencedId) && referencedId != default(int))
                                        {
                                            Console.WriteLine($"Getting by Id: {referencedId}");

                                            var udi = _getUdi(cliInput.UdiServiceName, referencedId);

                                            udiList.Add(udi.ToString());

                                            Console.WriteLine($"Setting {referencedId} UDI: {udi}");
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;

                                            MigrationHelper.WriteOutBoilerplate(content, property);

                                            Console.WriteLine($"Could not parse '{referencedId}'");

                                            Console.ResetColor();
                                        }
                                    }

                                    var csvString = string.Join(",", udiList);

                                    //handle the case where the double quotes are missing from the value
                                    if (match.Groups[0].ToString().Count(x => x == '"') != 4)
                                    {
                                        csvString = $"\"{csvString}\"";
                                    }

                                    var newValue = match.Groups[0].ToString()
                                        .Replace(match.Groups[1].ToString(), csvString);

                                    var finalMatchValue = propertyValueString.Replace(match.Groups[0].ToString(), newValue);
                                    Console.WriteLine($"Match Result: {finalMatchValue}");

                                    propertyValueString = finalMatchValue;

                                    property.Value = propertyValueString;

                                    Console.ResetColor();
                                }

                                Console.ForegroundColor = ConsoleColor.Cyan;

                                Console.WriteLine($"Final Result: {propertyValueString}");

                                if (shouldPersist)
                                {
                                    _contentService.Save(content);
                                }

                                Console.ResetColor();
                            }

                            Console.ResetColor();
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(ex.Message);
                            Console.ResetColor();
                        }
                    }
                }
            }

            return DeliverableResponse.Continue;
        }

        private string _getUdi(string serviceType, int id)
        {
            if (serviceType.ToLower() == "media")
            {
                return _mediaService.GetById(id).GetUdi().ToString();
            }

            if (serviceType.ToLower() == "content")
            {
                return _contentService.GetById(id).GetUdi().ToString();
            }

            return string.Empty;
        }
    }
}
