using AUX1.Entities;

namespace AUX1.Services
{
    public interface IAutenticacionService
    {
        Task<BusinessLogicResponse> LoginAsync(LoginRequest request);
        Task<BusinessLogicResponse> RefreshAsync(RefreshRequest request);

        Task<bool> ValidateAsync(ValidateRequest request);

        Task<BusinessLogicResponse?> LogoutAsync(string authorizationHeader);
    }
}