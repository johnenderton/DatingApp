using Entities;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}