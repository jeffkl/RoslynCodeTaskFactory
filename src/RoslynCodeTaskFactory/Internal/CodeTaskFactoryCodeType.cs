namespace RoslynCodeTaskFactory.Internal
{
    /// <summary>
    /// Represents the kind of code contained in the code task definition.
    /// </summary>
    internal enum CodeTaskFactoryCodeType
    {
        /// <summary>
        /// The code is a fragment and should be included within a method.
        /// </summary>
        Fragment,

        /// <summary>
        /// The code is a method and should be included within a class.
        /// </summary>
        Method,

        /// <summary>
        /// The code is a whole class and no modifications should be made to it.
        /// </summary>
        Class,
    }
}