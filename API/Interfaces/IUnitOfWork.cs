using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUnitOfWork{
        IUserRepository UserRepository {get;}
        IMessageRepository MessageRepository {get;}
        ILikesRepository LikesRepository {get;}
        /**** Photo Management Challenge 9. ****/
        IPhotoRepository PhotoRepository {get;}
        Task<bool> Complete();
        bool hasChanged();
    }
}