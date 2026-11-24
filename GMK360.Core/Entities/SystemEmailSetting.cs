using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class SystemEmailSetting : BaseEntity
    {
        [Required, MaxLength(150)]
        public string Host { get; set; } = "mail.gmk360.com";

        public int Port { get; set; } = 587;

        [Required, MaxLength(200)]
        public string Mail { get; set; } = "iletisim@gmk360.com";

        [MaxLength(500)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(150)]
        public string SenderName { get; set; } = "GMK360 Platform";

        public bool EnableSsl { get; set; } = true;
        public bool IgnoreCertificateErrors { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime? LastTestAt { get; set; }

        [MaxLength(500)]
        public string? LastTestStatus { get; set; }
    }
}
