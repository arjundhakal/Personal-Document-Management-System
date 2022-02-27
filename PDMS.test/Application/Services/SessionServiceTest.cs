using System;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PDMS.Domain.Entity;
using PDMS.Application.Repositories;
using PDMS.Application.Services;
using PDMS.Infrastructure;

namespace PDMS.Test.Application.Services
{
    public class SessionServiceTest
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _dbContex;
        private DatabaseRepository<Session> _sessionRepo;
        private string _dbName;

        public SessionServiceTest()
        {
            _dbName = Guid.NewGuid().ToString();
            _options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_dbName).Options;
            _dbContex = new ApplicationDbContext(_options);
            _sessionRepo = new DatabaseRepository<Session>(_dbContex);
        }
        [Fact]
        public async void WhenGivenValidSessionIdReturnSession()
        {
            var expectedSessionId = Guid.NewGuid();
            
            await _sessionRepo.Create(new Session()
            {
                SessionId = expectedSessionId,
                ExpireAt = DateTime.UtcNow.AddHours(1)
            });

            var sut = new SessionService(_sessionRepo);
            var actual = await sut.GetSession(expectedSessionId.ToString());
            actual.Should().NotBeNull();
            actual.SessionId.Should().Be(expectedSessionId);
        }
    }
}