using System.Net.Http;
using System.Threading.Tasks;

namespace OTPWarden.UIServices.Abstractions;

public interface IOAuthHttpService
{
    Task<U> PostAsync<T, U>(string requestUri, T content);
    Task<T> PostAsync<T>(string requestUri, HttpContent content);
    Task<T> GetAsync<T>(string requestUri);
}
