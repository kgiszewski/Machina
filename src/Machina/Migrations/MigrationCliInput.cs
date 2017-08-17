namespace Machina.Migrations
{
    public class MigrationCliInput
    {
        public bool ShouldPersist { get; set; }
        public string FilterBy { get; set; }
        public string UdiServiceName { get; set; }
        public string NestedContentDocTypePropertyAlias { get; set; }
    }
}
