using Ecom.OneCloud.Services;
using Ecom.OneCloud.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ecom.OneCloud
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddScoped<IFileUploaderService, FileUploaderService>();
            services.AddScoped<IDataTableService, DataTableService>();
            services.AddScoped<IDataTableActionsService, DataTableActionsService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddFluentEmail("noreply@onecloud.com")
                    .AddRazorRenderer()
                    .AddSendGridSender(Configuration["SendGripApiKey"]);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //TODO ERROR PAGE
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=OneCloud}/{action=UploadFile}/{id?}");
            });
        }
    }
}
