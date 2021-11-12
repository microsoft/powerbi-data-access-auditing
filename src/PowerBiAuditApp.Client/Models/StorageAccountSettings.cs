namespace PowerBiAuditApp.Client.Models
{
    public class StorageAccountSettings
    {
        public string StorageConnectionString { get; set; }
        public string AuditPreProcessBlobStorageName { get; set; }
        public string AuditPreProcessQueueName { get; set; }

        public bool WriteFile { get; set; }
    }
}