using System.Threading.Tasks;

namespace GMK360.Web.Services.Sms
{
    public interface ISmsService
    {
        Task<string> SmsGonderAsync(string telefonNo, string mesajMetni);
    }
}
