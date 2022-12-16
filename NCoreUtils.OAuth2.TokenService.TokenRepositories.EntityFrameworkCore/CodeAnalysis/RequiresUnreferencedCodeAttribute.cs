namespace System.Diagnostics.CodeAnalysis
{
    //
    // Summary:
    //     Indicates that the specified method requires dynamic access to code that is not
    //     referenced statically, for example, through System.Reflection.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    internal sealed class RequiresUnreferencedCodeAttribute : Attribute
    {
        //
        // Summary:
        //     Initializes a new instance of the System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute
        //     class with the specified message.
        //
        // Parameters:
        //   message:
        //     A message that contains information about the usage of unreferenced code.
        public RequiresUnreferencedCodeAttribute(string message)
        {
            Message = message;
        }

        //
        // Summary:
        //     Gets a message that contains information about the usage of unreferenced code.
        public string Message { get; }
        //
        // Summary:
        //     Gets or sets an optional URL that contains more information about the method,
        //     why it requires unreferenced code, and what options a consumer has to deal with
        //     it.
        public string? Url { get; set; }
    }
}