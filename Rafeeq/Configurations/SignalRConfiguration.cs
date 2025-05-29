using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Rafeeq.Hubs;

namespace Rafeeq.Configurations
{
    public static class SignalRConfiguration
    {
        public static void AddSignalRServices(this IServiceCollection services)
        {
            services.AddSignalR();
        }

        public static void UseSignalREndpoints(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                // Example hub endpoint
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}
