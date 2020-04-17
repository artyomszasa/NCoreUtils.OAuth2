namespace NCoreUtils.OAuth2
{
    public struct EncryptTokenResult
    {
        public bool Success { get; }

        public int Size { get; }

        public EncryptTokenResult(bool success, int size)
        {
            Success = success;
            Size = size;
        }
    }
}