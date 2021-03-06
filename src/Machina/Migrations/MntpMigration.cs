﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Machina.Migrations
{
    [DeliverableName("machina-migrate-mntp-picker")]
    [DeliverableAlias("machina-mmntp")]
    public class MntpMigration : Deliverable
    {
        private readonly IContentService _contentService;

        private static readonly string _newContentTypeAlias = "Umbraco.MultiNodeTreePicker2";

        public MntpMigration(
            TextReader reader, 
            TextWriter writer, 
            IContentService contentService
        ) : base(reader, writer)
        {
            _contentService = contentService;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            var cliInput = MigrationHelper.ParseCliArgs(args);

            Console.WriteLine("Migrating MNTP properties to UDI...");

            var allContent = MigrationHelper.GetAllContent(_contentService).FilterBy(cliInput);

            MigrationHelper.SetBufferSize(allContent.Count);

            var contentCounter = 1;
            var contentTotal = allContent.Count;

            foreach (var content in allContent)
            {
                contentCounter++;

                Console.WriteLine($"{contentCounter}/{contentTotal}");

                foreach (var property in content.Properties.Where(x => x.PropertyType.PropertyEditorAlias == _newContentTypeAlias))
                {
                    if (property.Value != null)
                    {
                        var propertyValueString = property.Value.ToString();

                        if (MigrationHelper.ShouldSkip(propertyValueString))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;

                            MigrationHelper.WriteOutBoilerplate(content, property);

                            Console.WriteLine("Skipping...");
                            Console.ResetColor();
                            continue;
                        }

                        var ids = propertyValueString.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

                        if (ids.Any())
                        {
                            var udisToPersist = new List<string>();

                            foreach (var id in ids)
                            {
                                var referencedId = 0;

                                if (int.TryParse(id, out referencedId) && referencedId != default(int))
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;

                                    MigrationHelper.WriteOutBoilerplate(content, property);

                                    var udi = _contentService.GetById(referencedId).GetUdi();

                                    udisToPersist.Add(udi.ToString());

                                    Console.ResetColor();
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;

                                    MigrationHelper.WriteOutBoilerplate(content, property);

                                    Console.WriteLine($"Could not parse '{propertyValueString}'");
                                    Console.ResetColor();
                                }
                            }

                            if (udisToPersist.Any())
                            {
                                var valueToSet = string.Join(",", udisToPersist);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Setting UDI: {valueToSet}");

                                property.Value = valueToSet;

                                Console.ResetColor();
                            }

                            if (cliInput.ShouldPersist)
                            {
                                _contentService.Save(content);
                            }
                        }
                    }
                }
            }

            return DeliverableResponse.Continue;
        }
    }
}
