namespace RoslynCodeTaskFactory.Internal
{
    internal sealed class Vbc : ManagedCompiler
    {
        protected override string ToolName => "vbc.exe";
    }
}