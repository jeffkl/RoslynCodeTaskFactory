using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

namespace RoslynCodeTaskFactory.Internal
{
    internal abstract class ManagedCompiler : ToolTask
    {
        public bool? Deterministic { get; set; }

        public bool? NoConfig { get; set; }

        public bool? NoLogo { get; set; }

        public bool? Optimize { get; set; }

        public ITaskItem OutputAssembly { get; set; }

        public ITaskItem[] References { get; set; }

        public ITaskItem[] Sources { get; set; }

        public string TargetType { get; set; }

        public bool? UseSharedCompilation { get; set; }

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

            AddCommandLineCommands(commandLineBuilder);

            return commandLineBuilder.ToString();
        }

        protected override string GenerateFullPathToTool()
        {
            if (!String.IsNullOrWhiteSpace(ToolExe) && Path.IsPathRooted(ToolExe))
            {
                return ToolExe;
            }

            string pathToBuildTools = ToolLocationHelper.GetPathToBuildTools(ToolLocationHelper.CurrentToolsVersion, DotNetFrameworkArchitecture.Bitness32);

            if (pathToBuildTools != null)
            {
                string toolMSBuildLocation = Path.Combine(pathToBuildTools, "Roslyn", ToolName);

                if (File.Exists(toolMSBuildLocation))
                {
                    return toolMSBuildLocation;
                }
            }

            return null;
        }

        protected override string GenerateResponseFileCommands()
        {
            CommandLineBuilderExtension commandLineBuilder = new CommandLineBuilderExtension(quoteHyphensOnCommandLine: false, useNewLineSeparator: false);

            AddResponseFileCommands(commandLineBuilder);

            return commandLineBuilder.ToString();
        }
    }
}