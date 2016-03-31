using System;
using Sodium;

namespace UCS.Crypto
{
    public class ClashKeyPair : IDisposable
    {
        public const int KeyLength = 32;
        public const int NonceLength = 24;
        private bool _disposed;
        private readonly KeyPair _keyPair;

        public ClashKeyPair(byte[] publicKey, byte[] privateKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException("publicKey");
            if (publicKey.Length != PublicKeyBox.PublicKeyBytes)
                throw new ArgumentOutOfRangeException("publicKey", "publicKey must be 32 bytes in length.");

            if (privateKey == null)
                throw new ArgumentNullException("privateKey");
            if (privateKey.Length != PublicKeyBox.SecretKeyBytes)
                throw new ArgumentOutOfRangeException("privateKey", "publicKey must be 32 bytes in length.");

            _keyPair = new KeyPair(publicKey, privateKey);
        }

        public byte[] PrivateKey
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(null, "Cannot access CoCKeyPair object because it was disposed.");

                return _keyPair.PrivateKey;
            }
        }

        public byte[] PublicKey
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(null, "Cannot access CoCKeyPair object because it was disposed.");

                return _keyPair.PublicKey;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _keyPair.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}