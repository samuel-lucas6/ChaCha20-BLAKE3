using System.Text;
using System.Security.Cryptography;
using Sodium;

/*
    ChaCha20-BLAKE3: Committing ChaCha20-BLAKE3, XChaCha20-BLAKE3, and XChaCha20-BLAKE3-SIV AEAD implementations.
    Copyright (c) 2021-2022 Samuel Lucas

    Permission is hereby granted, free of charge, to any person obtaining a copy of
    this software and associated documentation files (the "Software"), to deal in
    the Software without restriction, including without limitation the rights to
    use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
    the Software, and to permit persons to whom the Software is furnished to do so,
    subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

namespace ChaCha20Blake3
{
    public static class ChaCha20BLAKE3
    {
        public const int KeySize = Constants.KeySize;
        public const int NonceSize = Constants.ChaChaNonceSize;
        public const int TagSize = Constants.TagSize;
        private static readonly byte[] EncryptionContext = Encoding.UTF8.GetBytes("ChaCha20-BLAKE3 16/12/2021 14:08 ChaCha20.Encrypt()");
        private static readonly byte[] AuthenticationContext = Encoding.UTF8.GetBytes("ChaCha20-BLAKE3 16/12/2021 14:09 BLAKE3.KeyedHash()");
        
        public static byte[] Encrypt(byte[] message, byte[] nonce, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Message(message);
            ParameterValidation.Nonce(nonce, NonceSize);
            ParameterValidation.Key(key, KeySize);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(key, EncryptionContext, AuthenticationContext, nonce);
            byte[] ciphertext = StreamEncryption.EncryptChaCha20(message, nonce, encryptionKey);
            byte[] tag = Tag.Compute(additionalData, ciphertext, macKey);
            return Arrays.Concat(ciphertext, tag);
        }

        public static byte[] Decrypt(byte[] ciphertext, byte[] nonce, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Ciphertext(ciphertext);
            ParameterValidation.Nonce(nonce, NonceSize);
            ParameterValidation.Key(key, KeySize);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(key, EncryptionContext, AuthenticationContext, nonce);
            byte[] tag = Tag.Read(ciphertext, out byte[] ciphertextWithoutTag);
            byte[] computedTag = Tag.Compute(additionalData, ciphertextWithoutTag, macKey);
            bool validTag = Utilities.Compare(tag, computedTag);
            return !validTag ? throw new CryptographicException() : StreamEncryption.DecryptChaCha20(ciphertextWithoutTag, nonce, encryptionKey);
        }
    }
}