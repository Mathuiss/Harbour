using System;
using System.Collections.Generic;
using Harbour.Models;

namespace Harbour.Services
{
    public class ProxyService
    {
        public void Serve(List<Service> services)
        {
            Console.WriteLine("Starting proxy service...");
            Console.WriteLine("Success");
        }

        public void Stop()
        {
            Console.WriteLine("Stopping proxy service...");
            Console.WriteLine("Success");
        }
    }
}