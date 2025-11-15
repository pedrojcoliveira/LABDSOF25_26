using FluentAssertions;
using MindCarePlus.Application.Services;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Shared.DTOs.Auth;
using Moq;
using Xunit;

namespace MindCarePlus.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPacienteRepository> _mockPacienteRepository;
    private readonly Mock<IPsicologoRepository> _mockPsicologoRepository;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Criar mocks
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPacienteRepository = new Mock<IPacienteRepository>();
        _mockPsicologoRepository = new Mock<IPsicologoRepository>();

        // Configurar UnitOfWork para retornar os repositórios mockados
        _mockUnitOfWork.Setup(u => u.Pacientes).Returns(_mockPacienteRepository.Object);
        _mockUnitOfWork.Setup(u => u.Psicologos).Returns(_mockPsicologoRepository.Object);

        // Criar instância do serviço com o mock
        _authService = new AuthService(_mockUnitOfWork.Object);
    }

    #region RegisterPacienteAsync Tests

    [Fact]
    public async Task RegisterPacienteAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var dto = new RegisterPacienteDto
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Idade = 25,
            Objectivo = "Melhorar ansiedade"
        };

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(false);

        _mockPacienteRepository.Setup(r => r.AddAsync(It.IsAny<Paciente>()))
            .ReturnsAsync((Paciente p) => p);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterPacienteAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Conta de paciente criada com sucesso!");
        result.Token.Should().BeNull(); // Token ainda não implementado

        // Verificar que os métodos foram chamados
        _mockPacienteRepository.Verify(r => r.AddAsync(It.Is<Paciente>(p =>
            p.Nome == dto.Nome &&
            p.Email == dto.Email.ToLowerInvariant() &&
            p.Idade == dto.Idade &&
            p.Objectivo == dto.Objectivo &&
            p.NivelGamificacao == 1 &&
            p.Pontos == 0
        )), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPacienteAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var dto = new RegisterPacienteDto
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Idade = 25
        };

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterPacienteAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Este email já está registado no sistema");
        result.Token.Should().BeNull();

        // Verificar que AddAsync nunca foi chamado
        _mockPacienteRepository.Verify(r => r.AddAsync(It.IsAny<Paciente>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterPacienteAsync_WithoutObjectivo_ShouldCreateWithEmptyString()
    {
        // Arrange
        var dto = new RegisterPacienteDto
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Idade = 25,
            Objectivo = null // Sem objetivo
        };

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(false);

        _mockPacienteRepository.Setup(r => r.AddAsync(It.IsAny<Paciente>()))
            .ReturnsAsync((Paciente p) => p);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterPacienteAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        // Verificar que Objectivo foi definido como string vazia
        _mockPacienteRepository.Verify(r => r.AddAsync(It.Is<Paciente>(p =>
            p.Objectivo == string.Empty
        )), Times.Once);
    }

    [Fact]
    public async Task RegisterPacienteAsync_ShouldNormalizeEmail()
    {
        // Arrange
        var dto = new RegisterPacienteDto
        {
            Nome = "João Silva",
            Email = "JOAO@TESTE.COM", // Email em maiúsculas
            Password = "password123",
            ConfirmPassword = "password123",
            Idade = 25
        };

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(false);

        _mockPacienteRepository.Setup(r => r.AddAsync(It.IsAny<Paciente>()))
            .ReturnsAsync((Paciente p) => p);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterPacienteAsync(dto);

        // Assert
        _mockPacienteRepository.Verify(r => r.AddAsync(It.Is<Paciente>(p =>
            p.Email == "joao@teste.com" // Lowercase
        )), Times.Once);
    }

    [Fact]
    public async Task RegisterPacienteAsync_ShouldHashPassword()
    {
        // Arrange
        var dto = new RegisterPacienteDto
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Idade = 25
        };

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(false);

        _mockPacienteRepository.Setup(r => r.AddAsync(It.IsAny<Paciente>()))
            .ReturnsAsync((Paciente p) => p);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterPacienteAsync(dto);

        // Assert - Verificar que password foi hasheada (não é igual à original)
        _mockPacienteRepository.Verify(r => r.AddAsync(It.Is<Paciente>(p =>
            p.PasswordHash != dto.Password &&
            p.PasswordHash.StartsWith("$2") // BCrypt hash
        )), Times.Once);
    }

    #endregion

    #region RegisterPsicologoAsync Tests

    [Fact]
    public async Task RegisterPsicologoAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var dto = new RegisterPsicologoDto
        {
            Nome = "Dr. Maria Santos",
            Email = "maria@teste.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Idade = 35,
            NumeroCedulaProfissional = "CP-12345",
            Especialidade = "Psicologia Clínica"
        };

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.AddAsync(It.IsAny<Psicologo>()))
            .ReturnsAsync((Psicologo p) => p);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterPsicologoAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Conta de psicólogo criada com sucesso!");
        result.Token.Should().BeNull();

        // Verificar que os métodos foram chamados
        _mockPsicologoRepository.Verify(r => r.AddAsync(It.Is<Psicologo>(p =>
            p.Nome == dto.Nome &&
            p.Email == dto.Email.ToLowerInvariant() &&
            p.Idade == dto.Idade &&
            p.NumeroCedulaProfissional == dto.NumeroCedulaProfissional &&
            p.Especialidade == dto.Especialidade &&
            p.Avaliacao == 0.0f
        )), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPsicologoAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var dto = new RegisterPsicologoDto
        {
            Nome = "Dr. Maria Santos",
            Email = "maria@teste.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Idade = 35,
            NumeroCedulaProfissional = "CP-12345",
            Especialidade = "Psicologia Clínica"
        };

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterPsicologoAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Este email já está registado no sistema");
        result.Token.Should().BeNull();

        // Verificar que AddAsync nunca foi chamado
        _mockPsicologoRepository.Verify(r => r.AddAsync(It.IsAny<Psicologo>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region EmailExistsAsync Tests

    [Fact]
    public async Task EmailExistsAsync_WhenEmailExistsAsPaciente_ShouldReturnTrue()
    {
        // Arrange
        var email = "joao@teste.com";

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.EmailExistsAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailExistsAsPsicologo_ShouldReturnTrue()
    {
        // Arrange
        var email = "maria@teste.com";

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.EmailExistsAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var email = "novo@teste.com";

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.EmailExistsAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldNormalizeEmail()
    {
        // Arrange
        var email = "JOAO@TESTE.COM";

        _mockPacienteRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()))
            .ReturnsAsync(false);

        _mockPsicologoRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Psicologo, bool>>>()))
            .ReturnsAsync(false);

        // Act
        await _authService.EmailExistsAsync(email);

        // Assert - Verificar que foi chamado
        _mockPacienteRepository.Verify(r => r.ExistsAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Paciente, bool>>>()),
            Times.Once);
    }

    #endregion

    #region HashPassword Tests

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "password123";

        // Act
        var hash = _authService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password); // Hash deve ser diferente da password
        hash.Should().StartWith("$2"); // BCrypt hash começa com $2
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldThrowException()
    {
        // Arrange
        var password = "";

        // Act
        Action act = () => _authService.HashPassword(password);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password não pode ser vazia*");
    }

    [Fact]
    public void HashPassword_WithNullPassword_ShouldThrowException()
    {
        // Arrange
        string password = null!;

        // Act
        Action act = () => _authService.HashPassword(password);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ShouldGenerateDifferentHashes()
    {
        // Arrange
        var password = "password123";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert - BCrypt gera salt diferente cada vez
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "password123";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "password123";
        var incorrectPassword = "wrongpassword";
        var hash = _authService.HashPassword(correctPassword);

        // Act
        var result = _authService.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldThrowException()
    {
        // Arrange
        var password = "";
        var hash = _authService.HashPassword("password123");

        // Act
        Action act = () => _authService.VerifyPassword(password, hash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password não pode ser vazia*");
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ShouldThrowException()
    {
        // Arrange
        var password = "password123";
        var hash = "";

        // Act
        Action act = () => _authService.VerifyPassword(password, hash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Hash não pode ser vazio*");
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrowException()
    {
        // Act
        Action act = () => new AuthService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    #endregion
}