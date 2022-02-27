using System;
using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using System.Threading.Tasks;

namespace PDMS.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly IDatabaseRepository<Session> _sessionRepository;
        public SessionService(IDatabaseRepository<Session> sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }
        public Task<Session> GetSession(string sessionId)
        {
            return _sessionRepository.SingleOrDefaultAsync(x => x.SessionId.ToString() == sessionId && x.ExpireAt > DateTime.UtcNow);
        }

        public async Task SetActiveOrganisationForUser(string sessionId, int organisationId)
        {
            var session = await _sessionRepository.SingleOrDefaultAsync(_ => _.SessionId == Guid.Parse(sessionId));
            session.CurrentActiveOrganisationId = organisationId;
            await _sessionRepository.Update(session);
        }
    }
}
