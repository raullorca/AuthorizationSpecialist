using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MvcClient
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
            services.AddControllersWithViews();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
                        {
                            options.DefaultScheme = "Cookies"; // Usamos una cookie para iniciar localmente al usuario
                            options.DefaultChallengeScheme = "oidc"; // Usamos el protocolo OpenId Connect para iniciar sesion
                        })
                    .AddCookie("Cookies") // Agregamos el controlador que puede procesar cookies
                    // Configurar el controlador que realiza el protocolo OpenID Connect
                    .AddOpenIdConnect("oidc", options =>
                        {
                            options.Authority = "http://localhost:5000";  //  Indica dónde se encuentra el servicio token de confianza
                            options.RequireHttpsMetadata = false;

                            // Identificamos al cliente
                            options.ClientId = "mvc";
                            options.ClientSecret = "secret";

                            options.ResponseType = "code"; // Usamos el flujo de protocolo (code) con PKCE para conectarse al proveedor OpenID Connect

                            options.SaveTokens = true; // Se utiliza para conserver los tokens de IS en la cookie
                        });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute()
                        .RequireAuthorization(); // Deshabilita el acceso anonimo para toda la aplicación
            });
        }
    }
}
