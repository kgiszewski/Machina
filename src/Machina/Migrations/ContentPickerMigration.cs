using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Machina.Migrations
{
    [DeliverableName("migrate-content-picker")]
    [DeliverableAlias("mcp")]
    public class ContentPickerMigration : Deliverable
    {
        private readonly IContentService _contentService;

        private static readonly string _newContentTypeAlias = "Umbraco.ContentPicker2";

        public ContentPickerMigration(
            TextReader reader, 
            TextWriter writer, 
            IContentService contentService
        ) : base(reader, writer)
        {
            _contentService = contentService;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            var shouldPersist = MigrationHelper.ShouldPersist(args);

            Console.WriteLine("Migrating ContentPicker2 properties to UDI...");

            if (shouldPersist)
            {
                Console.WriteLine("Persisting!");
            }
            else
            {
                Console.WriteLine("Previewing, re-run with '1' as the first arg to persist.");
            }

            var allContent = MigrationHelper.GetAllContent(_contentService).FilterBy(args);

            MigrationHelper.SetBufferSize(allContent.Count);

            foreach (var content in allContent)
            {
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

                        var referencedId = 0;

                        if (int.TryParse(propertyValueString, out referencedId) && referencedId != default(int))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;

                            MigrationHelper.WriteOutBoilerplate(content, property);

                            var udi = _contentService.GetById(referencedId).GetUdi();

                            property.Value = udi;

                            if (shouldPersist)
                            {
                                _contentService.Save(content);
                            }

                            Console.WriteLine($"Setting UDI: {udi}");
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
                }
            }

            return DeliverableResponse.Continue;
        }
    }
}
