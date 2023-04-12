using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NsEduCore_DAL.Models.Data;
using NsEduCore_DAL.Services.User;

namespace NsEduCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Db
            services.AddDbContext<NsDataContext>();
            
            // IoC
            services.AddScoped<NsDataContext, NsDataContext>();
            services.AddScoped<IUserService, UserService>();

            #if DEBUG
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NsEduCore", Version = "v1" });

                // 讓 Swagger 文件包含專案中的 XML 註解。
                // 要讓這個可以正確執行，請在專案 build 的選項中勾選產生 XML。
                string filePath = Path.Combine(AppContext.BaseDirectory, "NsEduCore.xml");
                c.IncludeXmlComments(filePath);
            });
            #endif

            // JWT
            string secret = Configuration.GetSection("NsEduCore:JWT:SecretKey")?.Value;

            if (secret == null)
                throw new NullReferenceException("JWT secret key not set!");

            byte[] secretKey = Encoding.ASCII.GetBytes(secret);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #if DEBUG
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NsEduCore v1"));
            #endif

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}