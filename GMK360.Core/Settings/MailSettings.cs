namespace GMK360.Core.Settings
{
    public class MailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Mail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderName { get; set; } = "GMK360 Platform";
        public bool EnableSsl { get; set; } = true;
        public bool IgnoreCertificateErrors { get; set; } = false;
    }
}
