using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITI_Project.BLL.Helpers;
using ITI_Project.BLL.Services.Interfaces;
using ITI_Project.BLL.Settings;
using ITI_Project.BLL.ViewModel;
using ITI_Project.DAL.Models;
using ITI_Project.DAL.Models.Enums;
using ITI_Project.DAL.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ITI_Project.BLL.Services
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<PaginationSetting> _settings;

        public SessionService(IUnitOfWork unitOfWork, IOptions<PaginationSetting> settings)
        {
            _unitOfWork = unitOfWork;
            _settings = settings;
        }

        public async Task<SessionViewModel?> GetSessionAsyncVM(int id)
        {
            return await _unitOfWork.Sessions.GetAllAsQueryable()
                .Where(e => e.SessionId == id)
                .Include(e => e.Grades)
                    .ThenInclude(e => e.Trainee)
                .Select(e => new SessionViewModel
                {
                    SessionId = e.SessionId,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    trainee = e.Grades
                        .Where(g => g.Trainee.Role == UserRole.Trainee)
                        .Select(s => new Trainee
                        {
                            UserId = s.TraineeId,
                            Email = s.Trainee.Email,
                            Name = s.Trainee.Name
                        }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<Session?> GetSessionAsync(int id)
        {
            return await _unitOfWork.Sessions.GetByIdAsync(id);
        }

        public async Task<List<Session>> GetByCourseId(int id)
        {
            return await _unitOfWork.Sessions
                .GetAllAsQueryable()
                .Where(e => e.CourseId == id)
                .ToListAsync();
        }

        public async Task<Pagination<SessionViewModel>> GetAllAsync(string? courseName, int pageIndex, int? pageSize)
        {
            var query = _unitOfWork.Sessions.GetAllAsQueryable().Where(e=> courseName.IsNullOrEmpty() || e.Course.Name.ToLower().Contains(courseName.ToLower())).Include(c => c.Course);



            var mapped = query.Select(session => new SessionViewModel
            {
                SessionId = session.SessionId,
                CourseId = session.CourseId,
                CourseName = session.Course.Name ?? "",
                StartDate = session.StartDate,
                EndDate = session.EndDate
            });
            int defaultPageSize = _settings.Value.PageSize;
            int effectivePageSize = pageSize ?? defaultPageSize;

            return await Pagination<SessionViewModel>.CreateAsync(mapped, pageIndex, effectivePageSize);
        }

        public async Task<bool> CreateSessionAsync(Session model)
        {
            var session = new Session
            {
                CourseId = model.CourseId,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            await _unitOfWork.Sessions.AddAsync(session);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> UpdateSessionAsync(Session model)
        {
            var existingSession = await _unitOfWork.Sessions.GetByIdAsync(model.SessionId);
            if (existingSession == null) return false;

            existingSession.CourseId = model.CourseId;
            existingSession.StartDate = model.StartDate;
            existingSession.EndDate = model.EndDate;

            _unitOfWork.Sessions.Update(existingSession);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteSessionAsync(int id)
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(id);
            if (session == null) return false;

            _unitOfWork.Sessions.Delete(session);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
