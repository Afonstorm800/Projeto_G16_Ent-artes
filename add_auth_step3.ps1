#!/bin/bash

set -e

echo "=== Adding Authentication (Step 3) to Ent'Artes ==="

# 1. Add NuGet packages
cd EntArtes.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
cd ..

# 2. Create necessary folders
mkdir -p EntArtes.API/Services
mkdir -p EntArtes.API/DTOs
mkdir -p EntArtes.API/Controllers

# 3. Create IAuthService.cs
cat > EntArtes.API/Services/IAuthService.cs << 'EOF'
using EntArtes.Core.Entities;

namespace EntArtes.API.Services;

public interface IAuthService
{
    string GenerateJwtToken(Utilizador user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
EOF

# 4. Create AuthService.cs
cat > EntArtes.API/Services/AuthService.cs << 'EOF'
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EntArtes.Core.Entities;

namespace EntArtes.API.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateJwtToken(Utilizador user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT Key missing"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Tipo.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
EOF

# 5. Create LoginDto.cs
cat > EntArtes.API/DTOs/LoginDto.cs << 'EOF'
using System.ComponentModel.DataAnnotations;

namespace EntArtes.API.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
EOF

# 6. Create RegisterDto.cs
cat > EntArtes.API/DTOs/RegisterDto.cs << 'EOF'
using System.ComponentModel.DataAnnotations;
using EntArtes.Core.Entities;

namespace EntArtes.API.DTOs;

public class RegisterDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public TipoUtilizador Tipo { get; set; }
}
EOF

# 7. Create AuthController.cs
cat > EntArtes.API/Controllers/AuthController.cs << 'EOF'
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EntArtes.API.DTOs;
using EntArtes.API.Services;
using EntArtes.Core.Entities;
using EntArtes.Infrastructure.Data;

namespace EntArtes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _auth;

    public AuthController(AppDbContext context, IAuthService auth)
    {
        _context = context;
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // Check if user already exists
        if (await _context.Utilizadores.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email already registered" });

        var user = new Utilizador
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = _auth.HashPassword(dto.Password),
            Tipo = dto.Tipo
        };

        _context.Utilizadores.Add(user);
        await _context.SaveChangesAsync();

        var token = _auth.GenerateJwtToken(user);
        return Ok(new { token, user.Tipo, user.Nome });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !_auth.VerifyPassword(dto.Password, user.SenhaHash))
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _auth.GenerateJwtToken(user);
        return Ok(new { token, user.Tipo, user.Nome });
    }
}
EOF

# 8. Update Program.cs with JWT configuration
# We'll insert the authentication block before app.Run()
# Check if already added; if not, inject after the DbContext line.
PROGRAM_FILE="EntArtes.API/Program.cs"

# Backup original
cp "$PROGRAM_FILE" "$PROGRAM_FILE.bak"

# Check if JWT already configured
if ! grep -q "AddAuthentication" "$PROGRAM_FILE"; then
    # Insert using statements and auth code
    # First, ensure using statements are present (add if missing)
    if ! grep -q "Microsoft.AspNetCore.Authentication.JwtBearer" "$PROGRAM_FILE"; then
        sed -i '1i using Microsoft.AspNetCore.Authentication.JwtBearer;\nusing Microsoft.IdentityModel.Tokens;\nusing System.Text;' "$PROGRAM_FILE"
    fi

    # Find the line after builder.Services.AddDbContext and add JWT config
    # We'll use a more robust approach: insert before builder.Build()
    awk '
    /var app = builder.Build\(\);/ {
        print "// JWT Authentication";
        print "var jwtKey = builder.Configuration[\"Jwt:Key\"] ?? throw new InvalidOperationException(\"JWT Key not configured\");";
        print "var key = Encoding.ASCII.GetBytes(jwtKey);";
        print "";
        print "builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)";
        print "    .AddJwtBearer(options =>";
        print "    {";
        print "        options.TokenValidationParameters = new TokenValidationParameters";
        print "        {";
        print "            ValidateIssuerSigningKey = true,";
        print "            IssuerSigningKey = new SymmetricSecurityKey(key),";
        print "            ValidateIssuer = false,";
        print "            ValidateAudience = false";
        print "        };";
        print "    });";
        print "";
        print "builder.Services.AddScoped<IAuthService, AuthService>();";
        print "";
        print "var app = builder.Build();";
        next;
    }
    { print }
    ' "$PROGRAM_FILE" > "$PROGRAM_FILE.tmp" && mv "$PROGRAM_FILE.tmp" "$PROGRAM_FILE"

    # Add UseAuthentication and UseAuthorization before UseHttpsRedirection
    sed -i '/app.UseHttpsRedirection();/i app.UseAuthentication();\napp.UseAuthorization();' "$PROGRAM_FILE"
else
    echo "JWT already configured in Program.cs, skipping."
fi

# 9. Update appsettings.json with Jwt section if missing
SETTINGS_FILE="EntArtes.API/appsettings.json"
if ! grep -q '"Jwt"' "$SETTINGS_FILE"; then
    # Generate a random 64-character key using openssl if available, else a fixed placeholder
    if command -v openssl &> /dev/null; then
        JWT_KEY=$(openssl rand -base64 48 | tr -d '\n')
    else
        JWT_KEY="your-super-secret-key-at-least-32-characters-long-change-in-production"
    fi

    # Insert Jwt section before "AllowedHosts"
    sed -i "/\"AllowedHosts\"/i \  \"Jwt\": {\n    \"Key\": \"$JWT_KEY\"\n  }," "$SETTINGS_FILE"
    echo "Jwt section added to appsettings.json"
else
    echo "Jwt section already present in appsettings.json"
fi

echo "=== Authentication setup completed ==="
echo "Run 'dotnet run' inside EntArtes.API to test the /api/auth/register and /api/auth/login endpoints."
