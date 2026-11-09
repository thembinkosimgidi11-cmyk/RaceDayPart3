using System.Net.Http.Json;
using System.Text.Json;
namespace RaceDayMVC.Services;
public class RaceDayApiClient {
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    public RaceDayApiClient(IHttpClientFactory factory) => _http = factory.CreateClient("RaceDayApi");
    public async Task<T?> Get<T>(string url) {
        var r = await _http.GetAsync(url); if (!r.IsSuccessStatusCode) return default;
        return await r.Content.ReadFromJsonAsync<T>(_json);
    }
    public async Task<(bool Ok,string Message,T? Data)> Post<T>(string url, object body) {
        var r=await _http.PostAsJsonAsync(url,body); var msg=await r.Content.ReadAsStringAsync();
        T? data=default; try { data=JsonSerializer.Deserialize<T>(msg,_json); } catch {}
        return (r.IsSuccessStatusCode, string.IsNullOrWhiteSpace(msg)?r.ReasonPhrase??"":msg,data);
    }
    public async Task<(bool Ok,string Message)> Post(string url, object body) {
        var r=await _http.PostAsJsonAsync(url,body); return (r.IsSuccessStatusCode,await r.Content.ReadAsStringAsync());
    }
    public async Task<(bool Ok,string Message)> Put(string url, object body) {
        var r=await _http.PutAsJsonAsync(url,body); return (r.IsSuccessStatusCode,await r.Content.ReadAsStringAsync());
    }
    public async Task<(bool Ok,string Message)> Delete(string url) {
        var r=await _http.DeleteAsync(url); return (r.IsSuccessStatusCode,await r.Content.ReadAsStringAsync());
    }
}
