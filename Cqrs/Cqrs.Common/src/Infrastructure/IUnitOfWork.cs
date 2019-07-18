
namespace Cqrs.Infrastructure
{
    public interface IUnitOfWork
    {
        void Commit();
    }
}
