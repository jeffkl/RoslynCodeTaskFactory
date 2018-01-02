using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Linq;

namespace RoslynCodeTaskFactory.Internal
{
    internal abstract class ManagedCompiler : ToolTask
    {
        private static readonly string DotnetCliPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");

        private readonly Lazy<string> _executablePath;

        protected ManagedCompiler()
        {
            _executablePath = new Lazy<string>(() =>
            {
                string pathToBuildTools = ToolLocationHelper.GetPathToBuildTools(ToolLocationHelper.CurrentToolsVersion, DotNetFrameworkArchitecture.Bitness32);

                Func<string>[] possibleLocations =
                {
                    // Standard MSBuild and legacy .NET Core
                    () => Path.Combine(pathToBuildTools, "Roslyn", ToolName),
                    // Legacy .NET Core
                    () => Path.Combine(pathToBuildTools, "Roslyn", Path.ChangeExtension(ToolName, ".dll")),
                    // .NET Core 2.0
                    () => Path.Combine(pathToBuildTools, "Roslyn", "bincore", Path.ChangeExtension(ToolName, ".dll")),
                };

                return possibleLocations.Select(possibleLocation => possibleLocation()).FirstOrDefault(File.Exists);
            }, isThreadSafe: true);
        }

        public bool? Deterministic { get; set; }

        public bool? NoConfig { get; set; }

        public bool? NoLogo { get; set; }

        public bool? Optimize { get; set; }

        public ITaskItem OutputAssembly { get; set; }

        public ITaskItem[] References { get; set; }

        public ITaskItem[] Sources { get; set; }

        public string TargetType { get; set; }

        public bool? UseSharedCompilation { get; set; }

        protected bool IsDotnetCli => !String.IsNullOrWhiteSpace(DotnetCliPath);

        protected internal virtual void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendPlusOrMinusSwitch("/deterministic", Deterministic);
            commandLine.AppendWhenTrue("/nologo", NoLogo);
            commandLine.AppendPlusOrMinusSwitch("/optimize", Optimize);
            commandLine.AppendSwitchIfNotNull("/target:", TargetType);
            commandLine.AppendSwitchIfNotNull("/out:", OutputAssembly);
            commandLine.AppendFileNamesIfNotNull(Sources, " ");
        }

        protected virtual void AddCommandLineCommands(CommandLineBuilder commandLine)
        {
            commandLine.AppendWhenTrue("/noconfig", NoConfig);
        }

        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilderExtension commandLineBuilder = new CommandLineBuilderExtension();

            if (IsDotnetCli)
            {
                commandLineBuilder.AppendFileNameIfNotNull(_executablePath.Value);

                commandLineBuilder.AppendTextUnquoted(" ");
            }

            AddCommandLineCommands(commandLineBuilder);

            return commandLineBuilder.ToString();
        }

        protected override string GenerateFullPathToTool()
        {
            if (!String.IsNullOrWhiteSpace(ToolExe) && Path.IsPathRooted(ToolExe))
            {
                return ToolExe;
            }

            if (IsDotnetCli)
            {
                return DotnetCliPath;
            }

            return _executablePath.Value;
        }

        protected override string GenerateResponseFileCommands()
        {
            CommandLineBuilderExtension commandLineBuilder = new CommandLineBuilderExtension(quoteHyphensOnCommandLine: false, useNewLineSeparator: false);

            AddResponseFileCommands(commandLineBuilder);

            return commandLineBuilder.ToString();
        }
    }
}