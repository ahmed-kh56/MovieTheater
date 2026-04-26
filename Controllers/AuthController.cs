using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieRatingApp.Models.Auth;
using MovieRatingApp.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieRatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MovieDbContext _context;

        public AuthController(IConfiguration configuration, MovieDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == request.UserName);
                if (user == null)
                {
                    return Unauthorized($"Invalid username or password.\n hint: its the username not found");
                }

                if (user.Password != request.Password)
                {
                    return Unauthorized($"Invalid username or password.\n hint: its the password");
                }

                var token = GenerateJwtToken(user.Id, user.Name, user.Role.ToString());


                Response.Cookies.Append("jwt_token", token);

                return Ok(new
                {
                    Token = token,
                    Expires = DateTime.Now.AddMinutes(60),
                    User = new
                    {
                        Id = user.Id,
                        Role = user.Role,
                    }
                }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("reg")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Name == request.UserName))
                {
                    return BadRequest($"username already are in our database");
                }
                var user = new User(request.UserName, request.Password);


                var token = GenerateJwtToken(user.Id, user.Name, user.Role.ToString());


                Response.Cookies.Append("jwt_token", token);
                await _context.AddAsync(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Token = token,
                    Expires = DateTime.Now.AddMinutes(60),
                    User = new
                    {
                        Id = user.Id,
                        Role = user.Role,
                    }
                }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        private string GenerateJwtToken(Guid userId, string username, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, username)
            };

            claims.Add(new Claim("UserId", userId.ToString()));

            claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"])),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> AdminEndpoint([FromQuery] int page = 0, int pageSiz = 12)
        {
            try
            {
                var users = _context.Users.Skip(page * pageSiz).Take(pageSiz).ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
