using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using MindCarePlus.Shared.DTOs.Auth;
using System.Buffers.Text;
using System.Net.Http.Json;

namespace MindCarePlus.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        try
        {
            Console.WriteLine($"Tentando login para: {dto.Email}");
            Console.WriteLine($"BaseAddress: {_httpClient.BaseAddress}");

         
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);

            Console.WriteLine($"Status Code: {response.StatusCode}");

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Resposta vazia da API"
                    };
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<AuthResponseDto>(
                    responseContent,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                {
                    await SecureStorage.SetAsync("auth_token", result.Token);
                    Console.WriteLine("Token guardado com sucesso!");
                }

                return result ?? new AuthResponseDto
                {
                    Success = false,
                    Message = "Erro ao processar resposta"
                };
            }

            if (!string.IsNullOrWhiteSpace(responseContent))
            {
                try
                {
                    var errorResult = System.Text.Json.JsonSerializer.Deserialize<AuthResponseDto>(
                        responseContent,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    return errorResult ?? new AuthResponseDto
                    {
                        Success = false,
                        Message = "Credenciais inválidas"
                    };
                }
                catch
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = $"Erro: {responseContent}"
                    };
                }
            }

            return new AuthResponseDto
            {
                Success = false,
                Message = "Credenciais inválidas"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no login: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    // ... (GetTokenAsync, IsAuthenticatedAsync, LogoutAsync permanecem iguais)

    public async Task<Guid?> GetCurrentUserIdAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "sub" ||
                c.Type == "nameid" ||
                c.Type == "userId");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao extrair userId do token: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetUserRoleAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role");

            return roleClaim?.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao extrair role do token: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetUserEmailAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var emailClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Email ||
                c.Type == "email");

            return emailClaim?.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao extrair email do token: {ex.Message}");
            return null;
        }
    }

    public async Task<AuthResponseDto> RegisterPacienteAsync(RegisterPacienteDto dto)
    {
        try
        {
            
            var response = await _httpClient.PostAsJsonAsync("api/auth/register/paciente", dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>()
                    ?? new AuthResponseDto { Success = false, Message = "Erro ao processar resposta" };
            }

            var errorContent = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return errorContent ?? new AuthResponseDto
            {
                Success = false,
                Message = "Erro ao registar paciente"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponseDto> RegisterPsicologoAsync(RegisterPsicologoDto dto)
    {
        try
        {
            
            var response = await _httpClient.PostAsJsonAsync("api/auth/register/psicologo", dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>()
                    ?? new AuthResponseDto { Success = false, Message = "Erro ao processar resposta" };
            }

            var errorContent = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return errorContent ?? new AuthResponseDto
            {
                Success = false,
                Message = "Erro ao registar psicólogo"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        try
        {
            
            var response = await _httpClient.GetAsync($"api/auth/check-email/{Uri.EscapeDataString(email)}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync("auth_token");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            // 1. Obter o token atual
            var token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token))
            {
                // Já não está logado
                return true;
            }

            // 2. Chamar a API (opcional, se quiseres registar o logout no servidor)
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"/auth/logout", null);

            // 3. Limpar o token do SecureStorage (SEMPRE, mesmo se a API falhar)
            SecureStorage.Remove("auth_token");

            // 4. Limpar o header de autorização
            _httpClient.DefaultRequestHeaders.Authorization = null;

            return true;
        }
        catch (Exception ex)
        {
            // Mesmo com erro, remove o token localmente
            SecureStorage.Remove("auth_token");

            System.Diagnostics.Debug.WriteLine($"Erro no logout: {ex.Message}");
            return true; // Retorna true porque o token foi removido localmente
        }
    }
}