using System;

namespace NCoreUtils.OAuth2
{
    public class RijndaelTokenEncryptionConfiguration
    {
        private byte[]? _key;

        private byte[]? _iv;

        public string Key { get; set; } = string.Empty;

        public string IV { get; set; } = string.Empty;

        public byte[] KeyValue
        {
            get
            {
                if (_key is null)
                {
                    _key = Convert.FromBase64String(Key);
                }
                return _key;
            }
        }

        public byte[] IVValue
        {
            get
            {
                if (_iv is null)
                {
                    _iv = Convert.FromBase64String(IV);
                }
                return _iv;
            }
        }
    }
}