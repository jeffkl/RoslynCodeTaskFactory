using Microsoft.Build.Utilities;

namespace RoslynCodeTaskFactory.Internal
{
    internal static class ExtensionMethods
    {
        public static void AppendPlusOrMinusSwitch(this CommandLineBuilder commandLineBuilder, string switchName, bool? value)
        {
            if (value != null)
            {
                commandLineBuilder.AppendSwitchUnquotedIfNotNull(switchName, value.Value ? "+" : "-");
            }
        }

        public static void AppendWhenTrue(this CommandLineBuilder commandLineBuilder, string switchName, bool? value)
        {
            if (value.HasValue && value.Value)
            {
                commandLineBuilder.AppendSwitch(switchName);
            }
        }
    }
}