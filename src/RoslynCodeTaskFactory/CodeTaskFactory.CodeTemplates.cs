using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoslynCodeTaskFactory.Internal;

namespace RoslynCodeTaskFactory
{
    public sealed partial class CodeTaskFactory
    {
        private static readonly IDictionary<string, IDictionary<CodeTaskFactoryCodeType, string>> CodeTemplates = new Dictionary<string, IDictionary<CodeTaskFactoryCodeType, string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "CS", new Dictionary<CodeTaskFactoryCodeType, string>
                {
                    {
                        CodeTaskFactoryCodeType.Fragment, @"{0}

namespace InlineCode
{{
    public class {1} : Microsoft.Build.Utilities.Task
    {{
        public bool Success {{ get; private set; }} = true;
{2}
        public override bool Execute()
        {{
            {3}
            return Success;
        }}
    }}
}}"
                    },
                    {
                        CodeTaskFactoryCodeType.Method, @"{0}

namespace InlineCode
{{
    public class {1} : Microsoft.Build.Utilities.Task
    {{
        public bool Success {{ get; private set; }} = true;
{2}
        {3}
    }}
}}"
                    },
                }
            },
            {
                "VB", new Dictionary<CodeTaskFactoryCodeType, string>
                {
                    {
                        CodeTaskFactoryCodeType.Fragment, @"{0}

Namespace InlineCode
    Public Class {1}
        Inherits Microsoft.Build.Utilities.Task

        Public Property Success As Boolean = True
{2}
        Public Overrides Function Execute() As Boolean
            {3}
            Return Success
        End Function

    End Class
End Namespace"
                    },
                    {
                        CodeTaskFactoryCodeType.Method, @"{0}

Namespace InlineCode
    Public Class {1}
        Inherits Microsoft.Build.Utilities.Task

        Public Property Success As Boolean = True
{2}
        {3}

    End Class
End Namespace"
                    }
                }
            }
        };
    }
}
