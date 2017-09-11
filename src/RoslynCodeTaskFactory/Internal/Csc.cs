using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

namespace RoslynCodeTaskFactory.Internal
{
    internal sealed class Csc : ManagedCompiler
    {
        public bool? NoStandardLib { get; set; }

        protected override string ToolName => "Csc.exe";

        protected internal override void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendPlusOrMinusSwitch("/nostdlib", NoStandardLib);

            if (References != null)
            {
                foreach (ITaskItem reference in References)
                {
                    commandLine.AppendSwitchIfNotNull("/reference:", reference.ItemSpec);
                }
            }

            base.AddResponseFileCommands(commandLine);
        }
    }
}