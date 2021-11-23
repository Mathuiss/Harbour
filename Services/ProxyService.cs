using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harbour.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Harbour.Services
{
    public class ProxyService
    {
        private HttpClient _httpClient;
        private Dictionary<string, string> _endpointMap;
        public List<Service> Services { get; set; }

        public ProxyService() { }

        public ProxyService(List<Service> services)
        {
            Services = services;
            _httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });
            _endpointMap = new Dictionary<string, string>();

            foreach (Service service in Services)
            {
                foreach (Container container in service.Containers)
                {
                    if (container.Endpoint != null && container.HttpPort != null)
                    {
                        _endpointMap.Add(container.Endpoint, container.HttpPort);
                    }
                }
            }
        }

        public void StartServerBackground()
        {
            if (Services == null)
            {
                throw new InvalidOperationException("Unable to start server. Services is null.");
            }

            string executablePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", "");

            var startInfo = new ProcessStartInfo(executablePath, "serve")
            {
                RedirectStandardOutput = true
            };

            Process.Start(startInfo);
        }

        public void StartServer()
        {
            if (Services == null)
            {
                throw new InvalidOperationException("Unable to start server. Services is null.");
            }

            WebHost.CreateDefaultBuilder()
            .ConfigureKestrel(c =>
            {
                c.ListenAnyIP(80);
            })
            .UseKestrel()
            .Configure(app =>
            {
                app.Run(async http =>
                {
                    await HandleRequest(http);
                });
            })
            .Build()
            .Run();
        }

        public void Stop()
        {
            Process[] processes = Process.GetProcessesByName("harbour");

            for (int i = 0; i < processes.Length; i++)
            {
                Console.WriteLine(processes[i].ProcessName);
                Console.WriteLine(processes[i].Id);
                processes[i].Kill();
            }
        }

        async Task HandleRequest(HttpContext http)
        {
            // Determine endpoint
            foreach (string key in _endpointMap.Keys)
            {
                // if (http.Request.Path.ToString().StartsWith(key))
                if (http.Request.Path.StartsWithSegments(new PathString(key)))
                {
                    using (var proxyRequest = new HttpRequestMessage())
                    {
                        // Add method
                        proxyRequest.Method = new HttpMethod(http.Request.Method);

                        SetProxyUrl(http, proxyRequest, _endpointMap[key]);

                        SetProxyContentAndHeaders(http, proxyRequest);

                        // Wait for response
                        HttpResponseMessage responseMessage = await _httpClient.SendAsync(proxyRequest);

                        // Set response status code
                        http.Response.StatusCode = (int)responseMessage.StatusCode;

                        SetResponseHeaders(http, responseMessage);

                        // Copy request body
                        await responseMessage.Content.CopyToAsync(http.Response.Body);
                        return;
                    }
                }
            }

            string html = "<html><ul>";

            foreach (string key in _endpointMap.Keys)
            {
                html = string.Concat(html, $"<li><a href=\"{key}\">{key}</a></li>");
            }

            html = string.Concat(html, "</ul></html>");

            // Code is reached if no path was found
            await http.Response.WriteAsync(html);
        }

        void SetProxyUrl(HttpContext http, HttpRequestMessage proxyRequest, string endpoint)
        {
            // Add host
            string proxyUrl = $"http://localhost:{endpoint}";

            // Add path
            proxyUrl = string.Concat(proxyUrl, http.Request.Path);

            // Add query
            if (http.Request.QueryString.HasValue)
                proxyUrl = string.Concat(proxyUrl, http.Request.QueryString.ToUriComponent());

            // Prase URI and add to proxy request
            proxyRequest.RequestUri = new Uri(proxyUrl);
        }

        void SetProxyContentAndHeaders(HttpContext http, HttpRequestMessage proxyRequest)
        {
            // Make sure buffering is enabled
            // http.Request.EnableBuffering();

            // Add request body
            proxyRequest.Content = new StreamContent(http.Request.Body);

            // Add headers
            foreach (var header in http.Request.Headers)
            {
                if (header.Key.ToLower().StartsWith("content"))
                {
                    proxyRequest.Content.Headers.Add(header.Key, header.Value.ToArray());
                }
                else
                {
                    proxyRequest.Headers.Add(header.Key, header.Value.ToArray());
                }
            }

            // http.Request.Body.Position = 0; // Reset stream just in case
        }

        void SetResponseHeaders(HttpContext http, HttpResponseMessage responseMessage)
        {
            // Set response headers
            foreach (var header in responseMessage.Headers)
            {
                // Make sure the client knows that the proxy returns the entire message at once
                if (header.Key.ToLower() != "transfer-encoding")
                    http.Response.Headers[header.Key] = header.Value.ToArray();
            }

            // Set content headers
            foreach (var header in responseMessage.Content.Headers)
            {
                http.Response.Headers[header.Key] = header.Value.ToArray();
            }
        }
    }
}