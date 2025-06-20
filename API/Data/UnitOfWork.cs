using API.Interfaces;

namespace API.Data
{
    // A scoped service
    public class UnitOfWork(DataContext context, IUserRepository userRepository, ILikesRepository likesRepository, IMessageRepository messageRepository) : IUnitOfWork
    {
        public IUserRepository UserRepository => userRepository;

        public IMessageRepository MessageRepository => messageRepository;

        public ILikesRepository LikesRepository => likesRepository;

        public async Task<bool> Complete()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            // returns true if there are any tracker entities in the context that have been modified
            return context.ChangeTracker.HasChanges();
        }
    }
}
