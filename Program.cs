using System;
using System.IO;
using Harbour.Models;

namespace Harbour
{
    class Program
    {
        static void Main(string[] args)
        {
            Models.Harbour harbour = null;

            try
            {
                harbour = Parse(args);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(GenerateHelp());
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(GenerateHelp());
            }

            try
            {
                harbour.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Environment.Exit(0);
        }

        static Models.Harbour Parse(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentOutOfRangeException("Harbour takes 2 arguments: ");
            }

            string cmd = args[0].ToLower();
            string arg = args[1].ToLower();
            Cmd exec;

            switch (cmd)
            {
                case "apply":
                    // File path
                    exec = Cmd.Apply;
                    arg = ValidatePath(arg);
                    break;
                case "add":
                    // File path
                    exec = Cmd.Add;
                    arg = ValidatePath(arg);
                    break;
                case "remove":
                    // Service name
                    exec = Cmd.Remove;
                    break;
                case "serve":
                    // File path or stop
                    exec = Cmd.Serve;
                    if (arg.ToLower() == "stop")
                    {
                        arg = arg.ToLower();
                    }
                    else
                    {
                        arg = ValidatePath(arg);
                    }

                    break;
                default:
                    throw new ArgumentException($"Invalid argument supplied: {cmd}");
            }

            return new Models.Harbour(exec, arg);
        }

        static string GenerateHelp()
        {
            string help = @"Usage:
harbour [cmd] [string: argument]

Examples:
harbour apply [/path/to/state.json]
harbour add [/path/to/newService.json]
harbour remove [service-name]
harbour serve [/path/to/state/json]
harbour serve stop";

            return help;
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
