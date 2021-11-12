using API.Entities;

namespace API.Interfaces {
    public interface ITokenService {
         string CreateTroken(AppUser user);
    }
}