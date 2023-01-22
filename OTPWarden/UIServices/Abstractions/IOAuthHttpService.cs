using System.Net.Http;
using System.Threading.Tasks;

namespace OTPWarden.UIServices.Abstractions;

public interface IOAuthHttpService
{
    Task<T> PostAsync<T>(string requestUri, HttpContent content);
    Task<T> GetAsync<T>(string requestUri);
}
