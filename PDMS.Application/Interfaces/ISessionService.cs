using PDMS.Domain.Entity;

namespace PDMS.Application.Interfaces
{
    public interface ISessionService
    {
        Task<Session> GetSession(string sessionId);
        Task SetActiveOrganisationForUser(string sessionId, int organisationId);
    }
}
