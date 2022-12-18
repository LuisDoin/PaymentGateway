namespace CKOBankSimulator.Services.Interfaces
{
    public interface ICachingService
    {
        Task SetAsync(string key, string value);
        Task<string> GetAsync(string key);
        Task<bool> Contains(string key);
    }
}
