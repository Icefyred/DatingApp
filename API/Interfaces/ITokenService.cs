using System.Threading.Tasks;
using API.Entities;

namespace API.Interfaces {
    public interface ITokenService {
        Task<string> CreateTroken(AppUser user);
    }
}