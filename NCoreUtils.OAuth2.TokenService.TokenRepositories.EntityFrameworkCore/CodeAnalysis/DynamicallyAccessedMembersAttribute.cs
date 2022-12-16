namespace System.Diagnostics.CodeAnalysis
{
    //
    // Summary:
    //     Indicates that certain members on a specified System.Type are accessed dynamically,
    //     for example, through System.Reflection.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
    internal sealed class DynamicallyAccessedMembersAttribute : Attribute
    {
        //
        // Summary:
        //     Initializes a new instance of the System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute
        //     class with the specified member types.
        //
        // Parameters:
        //   memberTypes:
        //     The types of the dynamically accessed members.
        public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
        {
            MemberTypes = memberTypes;
        }

        //
        // Summary:
        //     Gets the System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes that
        //     specifies the type of dynamically accessed members.
        public DynamicallyAccessedMemberTypes MemberTypes { get; }
    }
}