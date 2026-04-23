using AUX1.Entities;
using AUX1.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace AUX1.Services
{
    public class AutenticacionService : IAutenticacionService
    {
        private readonly IAutenticacionRepository _repository;
        private readonly IJwtService _jwtService;
        private readonly IPermisosService _permisosService;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<AutenticacionService> _logger;
        private readonly int _jwtExpMinutos;
        private readonly int _refreshExpMinutos;

        public AutenticacionService(
            IAutenticacionRepository repository,
            IJwtService jwtService,
            IPermisosService permisosService,
            IBitacoraService bitacoraService,
            ILogger<AutenticacionService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _jwtService = jwtService;
            _permisosService = permisosService;
            _bitacoraService = bitacoraService;
            _logger = logger;
            _jwtExpMinutos = configuration.GetValue<int>("JwtSettings:ExpiracionMinutos");
            _refreshExpMinutos = configuration.GetValue<int>("JwtSettings:RefreshExpiracionMinutos");
        }

        // POST /login
        public async Task<BusinessLogicResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Identificacion) ||
                string.IsNullOrWhiteSpace(request.Contrasena))
            {
                _ = _bitacoraService.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = "Credenciales vacías",
                        Fecha = DateTime.Now,
                        Exitoso = false
                    }),
                    request.Identificacion?.Trim()
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 401,
                    Message = "Usuario y/o contraseña incorrectos."
                };
            }

            var identificacion = request.Identificacion.Trim();

            // Obtener credenciales desde PERMISOS 
            UsuarioPermisoDto? usuario;
            try
            {
                usuario = await _permisosService.ObtenerUsuarioAsync(identificacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar usuario {Usuario} en PERMISOS.", identificacion);
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al iniciar sesión."
                };
            }

            if (usuario is null)
            {
                _ = _bitacoraService.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = "Intento de login con usuario inexistente",
                        Usuario = identificacion,
                        Fecha = DateTime.Now,
                        Exitoso = false
                    }),
                    null
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 401,
                    Message = "Usuario y/o contraseña incorrectos."
                };
            }

            if (usuario.Estado is "Bloqueado" or "Inactivo")
            {
                _ = _bitacoraService.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = $"Intento de login con usuario {usuario.Estado.ToLower()}",
                        Identificacion = usuario.Identificacion,
                        Estado = usuario.Estado,
                        IntentosLogin = usuario.IntentosLogin,
                        Fecha = DateTime.Now,
                        Exitoso = false
                    }),
                    identificacion
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 401,
                    Message = "Usuario y/o contraseña incorrectos."
                };
            }

            var contrasenaHash = ComputeMd5(request.Contrasena);
            if (contrasenaHash != usuario.Contrasena)
            {
                var intentos = usuario.IntentosLogin + 1;

                try
                {
                    // AUX1 escribe intentos y bloqueo directamente
                    if (intentos >= 3)
                    {
                        await _repository.BloquearUsuarioAsync(identificacion);

                        _ = _bitacoraService.RegistrarAsync(
                            JsonSerializer.Serialize(new
                            {
                                Motivo = "Usuario bloqueado por 3 intentos fallidos",
                                Identificacion = usuario.Identificacion,
                                Estado = "Bloqueado",
                                IntentosLogin = intentos,
                                Fecha = DateTime.Now,
                                Exitoso = false
                            }),
                            identificacion
                        );
                    }
                    else
                    {
                        await _repository.ActualizarIntentosLoginAsync(identificacion, intentos);

                        _ = _bitacoraService.RegistrarAsync(
                            JsonSerializer.Serialize(new
                            {
                                Motivo = $"Intento de login fallido ({intentos}/3)",
                                Identificacion = usuario.Identificacion,
                                Estado = usuario.Estado,
                                IntentosLogin = intentos,
                                Fecha = DateTime.Now,
                                Exitoso = false
                            }),
                            identificacion
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al actualizar intentos de login para {Usuario}.",
                        identificacion
                    );
                }

                return new BusinessLogicResponse
                {
                    StatusCode = 401,
                    Message = "Usuario y/o contraseña incorrectos."
                };
            }

            // Login exitoso
            try
            {
                var ahora = DateTime.Now;

                // Resetear intentos y desactivar refresh tokens anteriores en paralelo.
                await Task.WhenAll(
                    _repository.ActualizarIntentosLoginAsync(identificacion, 0),
                    _repository.DesactivarRefreshTokensUsuarioAsync(identificacion)
                );

                var jwtToken = _jwtService.GenerarJwtToken(identificacion, _jwtExpMinutos);
                var refreshToken = _jwtService.GenerarRefreshToken();
                var horaVencimiento = ahora.AddMinutes(_jwtExpMinutos);

                await _repository.CrearRefreshTokenAsync(new RefreshTokenEntity
                {
                    Token = refreshToken,
                    UsuarioEmail = identificacion,
                    FechaCreacion = ahora,
                    FechaExpiracion = ahora.AddMinutes(_refreshExpMinutos),
                    Activo = true
                });

                _ = _bitacoraService.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = "Login exitoso",
                        Identificacion = usuario.Identificacion,
                        Estado = usuario.Estado,
                        IntentosLogin = 0,
                        ExpiracionJwt = horaVencimiento,
                        Fecha = ahora,
                        Exitoso = true
                    }),
                    identificacion
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Login exitoso.",
                    ResponseObject = new LoginResponse
                    {
                        AccessToken = jwtToken,
                        RefreshToken = refreshToken,
                        ExpiresIn = horaVencimiento,
                        UsuarioId = identificacion
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar tokens para {Usuario}.", identificacion);

                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al iniciar sesión."
                };
            }
        }

        // POST /refresh
        public async Task<BusinessLogicResponse> RefreshAsync(RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." };

            var tokenExistente = await _repository.ObtenerRefreshTokenAsync(request.RefreshToken);

            if (tokenExistente is null || !tokenExistente.Activo || tokenExistente.FechaExpiracion < DateTime.Now)
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." };

            try
            {
                var ahora = DateTime.Now;

                // Rotación de refresh token
                await _repository.DesactivarRefreshTokenAsync(request.RefreshToken);

                var nuevoJwt = _jwtService.GenerarJwtToken(tokenExistente.UsuarioEmail, _jwtExpMinutos);
                var nuevoRefresh = _jwtService.GenerarRefreshToken();
                var horaVencimiento = ahora.AddMinutes(_jwtExpMinutos);

                await _repository.CrearRefreshTokenAsync(new RefreshTokenEntity
                {
                    Token = nuevoRefresh,
                    UsuarioEmail = tokenExistente.UsuarioEmail,
                    FechaCreacion = ahora,
                    FechaExpiracion = ahora.AddMinutes(_refreshExpMinutos),
                    Activo = true
                });

                _ = _bitacoraService.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = "Token renovado",
                        ExpiracionJwt = horaVencimiento,
                        Fecha = ahora
                    }),
                    tokenExistente.UsuarioEmail
                );

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Token renovado exitosamente.",
                    ResponseObject = new RefreshResponse
                    {
                        AccessToken = nuevoJwt,
                        RefreshToken = nuevoRefresh,
                        ExpiresIn = horaVencimiento,
                        UsuarioId = tokenExistente.UsuarioEmail
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar token para {Usuario}.", tokenExistente.UsuarioEmail);

                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al renovar el token."
                };
            }
        }

        // POST /validate
        public Task<bool> ValidateAsync(ValidateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return Task.FromResult(false);

            return Task.FromResult(_jwtService.ValidarToken(request.Token));
        }

        // POST /logout
        public async Task<BusinessLogicResponse?> LogoutAsync(string authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado." };

            var token = authorizationHeader.Replace("Bearer ", "").Trim();

            if (!_jwtService.ValidarToken(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "Token inválido." };

            var usuario = ExtraerUsuarioDelToken(token);
            if (usuario is null)
                return new BusinessLogicResponse { StatusCode = 401, Message = "Token inválido." };

            try
            {
                await _repository.DesactivarRefreshTokensUsuarioAsync(usuario);

                _ = _bitacoraService.RegistrarAsync(
                    JsonSerializer.Serialize(new
                    {
                        Motivo = "Cierre de sesión exitoso",
                        Fecha = DateTime.Now,
                        Exitoso = true
                    }),
                    usuario
                );

                return null; // → 204 No Content
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión para usuario {Usuario}.", usuario);

                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = "Ocurrió un error interno al cerrar la sesión."
                };
            }
        }

        // Helpers
        private static string ComputeMd5(string input)
        {
            var bytes = System.Security.Cryptography.MD5.HashData(
                System.Text.Encoding.UTF8.GetBytes(input)
            );
            return Convert.ToHexString(bytes).ToLower();
        }

        private static string? ExtraerUsuarioDelToken(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.Subject;
        }
    }
}