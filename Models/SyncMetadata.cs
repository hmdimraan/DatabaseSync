namespace DatabaseSync.Models
{
    public class SyncMetadata
    {
        public string TableName { get; set; }
            = string.Empty;

        public string HashValue { get; set; }
            = string.Empty;

        public DateTime LastSyncedTime{  get; set; }
    }
}