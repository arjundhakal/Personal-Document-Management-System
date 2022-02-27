using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace PDMS.UI.Attributes
{
    public class RequireSession : TypeFilterAttribute
    {

        public RequireSession() : base(typeof(SessionActionFilterImpl))
        {

        }

        private class SessionActionFilterImpl : IActionFilter
        {
            private readonly IDatabaseRepository<Session> _sessionRepository;
            private readonly ISettingsService _settingsService;

            public SessionActionFilterImpl(IDatabaseRepository<Session> sessionRepository,
                                           ISettingsService settingsService)
            {
                _sessionRepository = sessionRepository;
                _settingsService = settingsService;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {

            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var sessionId =
                    context.HttpContext.Request.Cookies.SingleOrDefault(x => x.Key == "SessionId");

                if (string.IsNullOrEmpty(sessionId.Value) || !IsValidSession(sessionId.Value))
                    context.HttpContext.Response.Redirect("/Login");

            }

            private bool IsValidSession(string sessionId)
            {
                try
                {
                    Guid.TryParse(sessionId, out var sessionGuid);
                    var session = _sessionRepository.SingleOrDefaultAsync(x => x.SessionId == sessionGuid).Result;
                    if (session == default)
                        return false;

                    if (session.ExpireAt < DateTime.UtcNow)
                        return false;

                    ExtendSession(session);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }

            }

            private void ExtendSession(Session session)
            {
                var timeout = _settingsService.GetSettingValueByKey("SessionTimeoutInMins").Result;
                session.ExpireAt = session.ExpireAt.AddMinutes(int.Parse(timeout));
                _sessionRepository.Update(session).Wait();
            }

        }
    }
}
