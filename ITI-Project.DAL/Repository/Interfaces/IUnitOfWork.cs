using System.Threading.Tasks;

namespace ITI_Project.DAL.Repository.Interfaces
{
    public interface IUnitOfWork 
    {
        ICourseRepository Courses { get; }
        ISessionRepository Sessions { get; }
        IGradeRepository Grades { get; }
        IUserRepository Users { get; }
        Task<int> CompleteAsync(); 
    }
}
