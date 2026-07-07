using ClinicMS.Application.DTOs;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<Result<LoginResponseDto>> RefreshTokenAsync(string refreshToken);
    }
}