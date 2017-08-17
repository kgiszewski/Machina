using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur;
using Umbraco.Core.Services;

namespace Machina.Migrations
{
    [DeliverableName("machina-migrate-any-property")]
    [DeliverableAlias("machina-map")]
    public class AnyPropertyMigration : Deliverable
    {
        private readonly IContentService _contentService;

        public AnyPropertyMigration(
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

            Console.WriteLine($"Migrating property '{cliInput.PropertyTypeAlias}' properties to given value...");
            Console.WriteLine($"Given value => {cliInput.Value}");
            Console.WriteLine($"Treat value as number => {cliInput.TreatValueAsNumber}");

            var allContent = MigrationHelper.GetAllContent(_contentService).FilterBy(cliInput);

            MigrationHelper.SetBufferSize(allContent.Count);

            foreach (var content in allContent)
            {
                foreach (var property in content.Properties.Where(x => x.PropertyType.PropertyEditorAlias == cliInput.PropertyEditorAlias))
                {
                    if (property.Value != null)
                    {
                        MigrationHelper.WriteOutBoilerplate(content, property);
                    }
                }
            }

            return DeliverableResponse.Continue;
        }
    }
}
