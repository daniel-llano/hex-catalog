using FluentAssertions;
using Moq;
using HexCatalog.Application.Commands.Auth;
using HexCatalog.Application.Interfaces;
using HexCatalog.Domain.Entities;
using HexCatalog.Domain.Exceptions;
using HexCatalog.Domain.Ports;

namespace HexCatalog.UnitTests.Application.Auth;

public class RegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly RegisterHandler _sut;

    public RegisterHandlerTests()
    {
        _sut = new RegisterHandler(_userRepo.Object, _uow.Object, _jwt.Object, new FakeRefresh());
    }

    [Fact]
    public async Task Handle_NewUser_ReturnsAuthResponse()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("alice", default))
                 .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default))
                 .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(new AccessToken("mocked-jwt-token", DateTime.UtcNow.AddHours(1)));

        var cmd = new RegisterCommand("alice", "P@ssw0rd");

        var result = await _sut.Handle(cmd, default);

        result.AccessToken.Should().Be("mocked-jwt-token");
        result.Username.Should().Be("alice");
        result.Role.Should().Be("Viewer");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ThrowsConflict()
    {
        var existing = User.Create("alice", "hashedpw", "User");
        _userRepo.Setup(r => r.GetByUsernameAsync("alice", default))
                 .ReturnsAsync(existing);

        var cmd = new RegisterCommand("alice", "P@ssw0rd");

        var act = () => _sut.Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*alice*");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _sut = new LoginHandler(_userRepo.Object, _jwt.Object, new FakeRefresh());
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("secret");
        var user = User.Create("bob", hash, "Admin");
        _userRepo.Setup(r => r.GetByUsernameAsync("bob", default)).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns(new AccessToken("token-for-bob", DateTime.UtcNow.AddHours(1)));

        var cmd = new LoginCommand("bob", "secret");

        var result = await _sut.Handle(cmd, default);

        result.AccessToken.Should().Be("token-for-bob");
        result.Username.Should().Be("bob");
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorized()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("unknown", default))
                 .ReturnsAsync((User?)null);

        var act = () => _sut.Handle(new LoginCommand("unknown", "pw"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorized()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct");
        var user = User.Create("carol", hash, "User");
        _userRepo.Setup(r => r.GetByUsernameAsync("carol", default)).ReturnsAsync(user);

        var act = () => _sut.Handle(new LoginCommand("carol", "wrong"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }
}

file sealed class FakeRefresh : IRefreshTokenStore
{
    public Task<string> IssueAsync(Guid userId, CancellationToken ct = default) => Task.FromResult("refresh-" + userId.ToString("N"));
    public Task<Guid?> RedeemAsync(string token, CancellationToken ct = default) => Task.FromResult<Guid?>(null);
    public Task RevokeAsync(string token, CancellationToken ct = default) => Task.CompletedTask;
    public Task RevokeAllAsync(Guid userId, CancellationToken ct = default) => Task.CompletedTask;
}
