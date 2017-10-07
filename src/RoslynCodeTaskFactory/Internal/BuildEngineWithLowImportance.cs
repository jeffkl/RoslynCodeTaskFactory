using System;
using System.Collections;
using Microsoft.Build.Framework;

namespace RoslynCodeTaskFactory.Internal
{
    /// <inheritdoc />
    /// <summary>
    /// Wraps an <see cref="T:Microsoft.Build.Framework.IBuildEngine" /> and mutates all message importance to <see cref="F:Microsoft.Build.Framework.MessageImportance.Low" />.  This allows the
    /// <see cref="T:RoslynCodeTaskFactory.CodeTaskFactory" /> to host the execution of other tasks without those tasks' output cluttering up the primary build
    /// output.  Everything here just returns the wrapped <see cref="T:Microsoft.Build.Framework.IBuildEngine" /> instance except for <see cref="M:Microsoft.Build.Framework.IBuildEngine.LogMessageEvent(Microsoft.Build.Framework.BuildMessageEventArgs)" />.
    /// </summary>
    internal class BuildEngineWithLowImportance : IBuildEngine
    {
        /// <summary>
        /// Stores the wrapped <see cref="IBuildEngine"/> object.
        /// </summary>
        private readonly IBuildEngine _buildEngine;

        /// <summary>
        /// Initializes a new instance of the BuildEngineWithLowImportance class.
        /// </summary>
        /// <param name="buildEngine">An <see cref="IBuildEngine"/> object to wrap and mutate messages to low importance.</param>
        public BuildEngineWithLowImportance(IBuildEngine buildEngine)
        {
            _buildEngine = buildEngine ?? throw new ArgumentNullException(nameof(buildEngine));
        }

        /// <inheritdoc cref="IBuildEngine.ColumnNumberOfTaskNode"/>
        public int ColumnNumberOfTaskNode => _buildEngine.ColumnNumberOfTaskNode;

        /// <inheritdoc cref="IBuildEngine.ContinueOnError"/>
        public bool ContinueOnError => _buildEngine.ContinueOnError;

        /// <inheritdoc cref="IBuildEngine.LineNumberOfTaskNode"/>
        public int LineNumberOfTaskNode => _buildEngine.LineNumberOfTaskNode;

        /// <inheritdoc cref="IBuildEngine.ProjectFileOfTaskNode"/>
        public string ProjectFileOfTaskNode => _buildEngine.ProjectFileOfTaskNode;

        /// <inheritdoc cref="IBuildEngine.BuildProjectFile"/>
        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs) => _buildEngine.BuildProjectFile(projectFileName, targetNames, globalProperties, targetOutputs);

        /// <inheritdoc cref="IBuildEngine.LogCustomEvent"/>
        public void LogCustomEvent(CustomBuildEventArgs e) => _buildEngine.LogCustomEvent(e);

        /// <inheritdoc cref="IBuildEngine.LogErrorEvent"/>
        public void LogErrorEvent(BuildErrorEventArgs e) => _buildEngine.LogErrorEvent(e);

        /// <inheritdoc />
        /// <summary>
        /// Logs a message but with the importance changed to <see cref="F:Microsoft.Build.Framework.MessageImportance.Low" />.
        /// </summary>
        /// <param name="e"></param>
        public void LogMessageEvent(BuildMessageEventArgs e) => _buildEngine.LogMessageEvent(new BuildMessageEventArgs(
            e.Subcategory,
            e.Code,
            e.File,
            e.LineNumber,
            e.ColumnNumber,
            e.EndLineNumber,
            e.EndColumnNumber,
            e.Message,
            e.HelpKeyword,
            e.SenderName,
            MessageImportance.Low, // Lowers the message importance
            e.Timestamp));

        /// <inheritdoc cref="IBuildEngine.LogWarningEvent"/>
        public void LogWarningEvent(BuildWarningEventArgs e) => _buildEngine.LogMessageEvent(new BuildMessageEventArgs(
            e.Subcategory,
            e.Code,
            e.File,
            e.LineNumber,
            e.ColumnNumber,
            e.EndLineNumber,
            e.EndColumnNumber,
            e.Message,
            e.HelpKeyword,
            e.SenderName,
            MessageImportance.Low, // Log warnings as low importance messages instead
            e.Timestamp));
    }
}