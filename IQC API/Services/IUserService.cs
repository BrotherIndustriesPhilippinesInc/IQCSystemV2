using IQC_API.DTO.User;

namespace IQC_API.Services
{
    public interface IUserService
    {
        // Notice we return the DTO, not the DB Entity
        Task<UserDto?> GetUserByEmpNoAsync(string empNo);
    }
}
