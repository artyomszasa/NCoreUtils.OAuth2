using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class IntrospectionServiceInput : CustomServiceInput
    {
        public override InputDeserializer CreateDeserializer(MethodDescriptor method)
            => IntrospectionInputDeserializer.Instance;
    }
}