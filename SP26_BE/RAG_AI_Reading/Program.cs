using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; 
using Repository; 
using Service;    
using System.Text;

namespace RAG_AI_Reading
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // 1. Tự động đăng ký tất cả Repository
            builder.Services.Scan(scan => scan
                .FromAssembliesOf(typeof(Repository.UserRepository)) // Chỉ cần trỏ vào 1 class bất kỳ trong Project Repository
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository"))) // Chỉ lấy các file tên kết thúc bằng "Repository"
                .AsSelf() // Đăng ký chính nó (UserRepository -> UserRepository)
                .WithScopedLifetime()); // Vòng đời Scoped (Mỗi request 1 lần)

            // 2. Tự động đăng ký tất cả Service
            builder.Services.Scan(scan => scan
                .FromAssembliesOf(typeof(Service.AuthService)) // Trỏ vào 1 class bất kỳ trong Project Service
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service"))) // Chỉ lấy các file tên kết thúc bằng "Service"
                .AsSelf()
                .WithScopedLifetime());


            // 3. Configure JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            // Kiểm tra null để tránh lỗi crash lúc khởi động nếu quên cấu hình appsettings.json
            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Thiếu cấu hình 'Jwt:Key' trong appsettings.json");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Headers["Authorization"].ToString();
                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            // 3. Configure Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RAG Story AI API", Version = "v1" });

                // Cấu hình nút "Authorize" (ổ khóa) trên Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Nhập JWT Token vào đây (Ví dụ: eyJhbGciOiJIUz...)",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    b => b.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            
            // Kích hoạt CORS trước Auth
            app.UseCors("AllowAll"); 

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}