namespace System.Diagnostics.CodeAnalysis
{
    //
    // Summary:
    //     Suppresses reporting of a specific rule violation, allowing multiple suppressions
    //     on a single code artifact.
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    internal sealed class UnconditionalSuppressMessageAttribute : Attribute
    {
        //
        // Summary:
        //     Initializes a new instance of the System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute
        //     class, specifying the category of the tool and the identifier for an analysis
        //     rule.
        //
        // Parameters:
        //   category:
        //     The category for the attribute.
        //
        //   checkId:
        //     The identifier of the analysis rule the attribute applies to.
        public UnconditionalSuppressMessageAttribute(string category, string checkId)
        {
            Category = category;
            CheckId = checkId;
        }

        //
        // Summary:
        //     Gets the category identifying the classification of the attribute.
        public string Category { get; }
        //
        // Summary:
        //     Gets the identifier of the analysis tool rule to be suppressed.
        public string CheckId { get; }
        //
        // Summary:
        //     Gets or sets the justification for suppressing the code analysis message.
        public string? Justification { get; set; }
        //
        // Summary:
        //     Gets or sets an optional argument expanding on exclusion criteria.
        public string? MessageId { get; set; }
        //
        // Summary:
        //     Gets or sets the scope of the code that is relevant for the attribute.
        public string? Scope { get; set; }
        //
        // Summary:
        //     Gets or sets a fully qualified path that represents the target of the attribute.
        public string? Target { get; set; }
    }
}