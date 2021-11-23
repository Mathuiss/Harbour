
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace Harbour
{
    class Program
    {
        private static Models.Harbour _harbour;

        static void Main(string[] args)
        {
            try
            {
                var apply = new Command("apply", "Apply either the running configuration or supply a new state.json file.")
                {
                    new Argument<string>("path", () => null, "Path to a new state.json file."),
                };
                apply.Handler = CommandHandler.Create<string>(HandleApply);

                var add = new Command("add", "Add a new service to the running configuration.")
                {
                    new Argument<string>("path", "Path to a new state.json file.")
                };
                add.Handler = CommandHandler.Create<string>(HandleAdd);

                var remove = new Command("remove", "Removes a service from the running configuration")
                {
                    new Argument<string>("serviceName", "The name of the service you want to remove.")
                };
                remove.Handler = CommandHandler.Create<string>(HandleRemove);

                var serve = new Command("serve", "Starts web server/service router.")
                {
                    new Argument<string>("path", () => null, "Path to a new state.json file."),
                    new Command("stop", "Stops the web server/service router")
                    {
                        Handler = CommandHandler.Create(StopServe)
                    },
                    new Option(new string[] { "--detached", "-d" }, "Add this option to start the web server/service router in the background."),
                };
                serve.Handler = CommandHandler.Create<string, bool>(HandleServe);

                var rootCommand = new RootCommand("Harbour manages your docker environment using a simple json file.")
                {
                    apply,
                    add,
                    remove,
                    serve,
                };

                rootCommand.Invoke(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Environment.Exit(0);
        }

        static void HandleApply(string path)
        {
            if (!string.IsNullOrEmpty(path))
                path = ValidatePath(path);

            _harbour = new Models.Harbour();
            _harbour.Apply(path);
        }

        static void HandleAdd(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path is a required argument for add");
            }

            path = ValidatePath(path);
            _harbour = new Models.Harbour();
            _harbour.Add(path);
        }

        static void HandleRemove(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException("Service name is a required argument for remove");
            }

            _harbour = new Models.Harbour();
            _harbour.Remove(serviceName);
        }

        static void HandleServe(string path, bool detached)
        {
            if (!string.IsNullOrEmpty(path))
                path = ValidatePath(path);

            _harbour = new Models.Harbour();

            if (detached)
            {
                _harbour.ServeBackground(path);
            }
            else
            {
                _harbour.Serve(path);
            }
        }

        static void StopServe()
        {
            _harbour = new Models.Harbour();
            _harbour.StopServe();
        }

        static string ValidatePath(string path)
        {
            if (!path.StartsWith('/'))
            {
                path = Path.Combine(Environment.CurrentDirectory, path);
            }

            if (!File.Exists(path))
                throw new ArgumentException($"Path {path} was not found.");

            return path;
        }
    }
}
