using System;

namespace NCoreUtils.OAuth2
{
    public class AesTokenEncryptionConfiguration
    {
        private byte[]? _key;

        private byte[]? _iv;

        /// <summary>
        /// Key in BASE64 format (e.g. supplied by configuration).
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// IV in BASE64 format (e.g. supplied by configuration).
        /// </summary>
        public string IV { get; set; } = string.Empty;

        public byte[] KeyValue => _key ??= Convert.FromBase64String(Key);

        public byte[] IVValue => _iv ??= Convert.FromBase64String(IV);
    }
}