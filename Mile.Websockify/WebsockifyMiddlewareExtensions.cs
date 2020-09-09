using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace Mile.Websockify
{
    /// <summary>
    /// <see cref="IApplicationBuilder" /> extension methods to add and 
    /// configure <see cref="WebsockifyMiddleware" />.
    /// </summary>
    public static class WebsockifyMiddlewareExtensions
    {
        /// <summary>
        /// Adds the <see cref="WebsockifyMiddleware" /> to the request 
        /// pipeline.
        /// </summary>
        /// <param name="app">
        /// The <see cref="IApplicationBuilder" /> to configure.
        /// </param>
        /// <param name="pathMatch">
        /// The request path to match.
        /// </param>
        /// <param name="hostname">
        /// The target hostname.
        /// </param>
        /// <param name="port">
        /// The target port.
        /// </param>
        /// <returns>
        /// The <see cref="IApplicationBuilder" />.
        /// </returns>
        public static IApplicationBuilder UseWebsockify(
            this IApplicationBuilder app,
            PathString pathMatch,
            string hostname,
            int port)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (pathMatch == null)
            {
                throw new ArgumentNullException(nameof(pathMatch));
            }

            if (hostname == null)
            {
                throw new ArgumentNullException(nameof(hostname));
            }

            return app
                .UseWebSockets()
                .Map(
                    pathMatch,
                    a => a.UseMiddleware<WebsockifyMiddleware>(hostname, port));
        }
    }
}
