#nullable enable


namespace System.Diagnostics.CodeAnalysis
{
    //
    // Summary:
    //     Specifies that the method or property will ensure that the listed field and property
    //     members have non-null values when returning with the specified return value condition.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    internal sealed class MemberNotNullWhenAttribute : Attribute
    {
        //
        // Summary:
        //     Initializes the attribute with the specified return value condition and a field
        //     or property member.
        //
        // Parameters:
        //   returnValue:
        //     The return value condition. If the method returns this value, the associated
        //     parameter will not be null.
        //
        //   member:
        //     The field or property member that is promised to be non-null.
        public MemberNotNullWhenAttribute(bool returnValue, string member)
            : this(returnValue, new [] { member })
        { }
        //
        // Summary:
        //     Initializes the attribute with the specified return value condition and list
        //     of field and property members.
        //
        // Parameters:
        //   returnValue:
        //     The return value condition. If the method returns this value, the associated
        //     parameter will not be null.
        //
        //   members:
        //     The list of field and property members that are promised to be non-null.
        public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
        {
            Members = members;
            ReturnValue = returnValue;
        }

        //
        // Summary:
        //     Gets field or property member names.
        public string[] Members { get; }
        //
        // Summary:
        //     Gets the return value condition.
        public bool ReturnValue { get; }
    }
}