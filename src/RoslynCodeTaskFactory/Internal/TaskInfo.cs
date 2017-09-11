using System;
using System.Collections.Generic;

namespace RoslynCodeTaskFactory.Internal
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the information parsed from a code task declaration.
    /// </summary>
    internal sealed class TaskInfo : IEquatable<TaskInfo>
    {
        /// <summary>
        /// Gets or sets the code language of the task.
        /// </summary>
        public string CodeLanguage { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CodeTaskFactoryCodeType"/> of the task.
        /// </summary>
        public CodeTaskFactoryCodeType CodeType { get; set; }

        /// <summary>
        /// Gets or sets the name of the task.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a <see cref="ISet{String}"/> of namespaces to use.
        /// </summary>
        public ISet<string> Namespaces { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets an <see cref="ISet{String}"/> of assembly references.
        /// </summary>
        public ISet<string> References { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the source code of the assembly.
        /// </summary>
        public string SourceCode { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Determines if the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">A <see cref="T:RoslynCodeTaskFactory.Internal.TaskInfo" /> object to compare to this instance.</param>
        /// <returns><code>true</code> if the specified object is equivalent to the current object, otherwise <code>false</code>.</returns>
        public bool Equals(TaskInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return References.Equals(other.References) && String.Equals(SourceCode, other.SourceCode, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc cref="Equals(Object)"/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as TaskInfo);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}