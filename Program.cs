using System;

namespace Harbour
{
    class Program
    {
        static void Main(string[] args)
        {
            Parse(args);
        }

        static void Parse(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentOutOfRangeException(GenerateHelp());
            }

            string cmd = args[0].ToLower();
            string arg = args[1].ToLower();

            switch (cmd)
            {
                case "apply": break;
                case "add": break;
                case "remove": break;
                case "serve": break;
            }
        }

        static string GenerateHelp()
        {
            string help = @"
            Usage:
            harbour [cmd] [string: argument]

            Examples:
            harbour apply [/path/to/state.json]
            harbour add [/path/to/newService.json]
            harbour remove [service-name]
            harbour serve [/path/to/state/json]
            harbour serve stop
            ";

            return help;
        }
    }
}
