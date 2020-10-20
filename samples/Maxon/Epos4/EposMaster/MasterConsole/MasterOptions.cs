using CommandLine;

namespace EposMaster.MasterConsole
{
    public class MasterOptions
    {
        [Option('h', "host", Default = null, HelpText = "target host", Required = false)]
        public string Host { get; set; }

        [Option('l', "login", Default = null, HelpText = "login name", Required = false)]
        public string Login { get; set; }

        [Option('p', "password", Default = null, HelpText = "password", Required = false)]
        public string Password { get; set; }

        [Option('s', "stop", Default = false, HelpText = "Stop running master", Required = false)]
        public bool Stop { get; set; }
    }
}
