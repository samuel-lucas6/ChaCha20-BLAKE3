using System.Security.Cryptography;
using Sodium;

/*
    ChaCha20-BLAKE3: Committing ChaCha20-BLAKE3, XChaCha20-BLAKE3, and XChaCha20-BLAKE3-SIV implementations.
    Copyright (c) 2021 Samuel Lucas

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

namespace ChaCha20BLAKE3
{
    public static class XChaCha20_BLAKE3_SIV
    {
        /// <summary>Encrypts a message using XChaCha20-BLAKE3-SIV.</summary>
        /// <param name="message">The message to encrypt.</param>
        /// <param name="key">The 64 byte key.</param>
        /// <param name="additionalData">Optional additional data to authenticate.</param>
        /// <returns>The ciphertext and tag.</returns>
        public static byte[] Encrypt(byte[] message, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Message(message);
            ParameterValidation.Key(key, Constants.KeyLength * 2);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] macKey, byte[] encryptionKey) = KeyDerivation.DeriveKeysSIV(key);
            byte[] tagMessage = Arrays.Concat(additionalData, message, BitConversion.GetBytes(additionalData.Length), BitConversion.GetBytes(message.Length));
            byte[] tag = Tag.Compute(tagMessage, macKey);
            byte[] nonce = Tag.GetNonce(tag);
            byte[] ciphertext = StreamEncryption.EncryptXChaCha20(message, nonce, encryptionKey);
            return Arrays.Concat(ciphertext, tag);
        }

        /// <summary>Decrypts a ciphertext message using XChaCha20-BLAKE3-SIV.</summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="key">The 64 byte key.</param>
        /// <param name="additionalData">Optional additional data to authenticate.</param>
        /// <returns>The decrypted message.</returns>
        public static byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Ciphertext(ciphertext);
            ParameterValidation.Key(key, Constants.KeyLength * 2);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] macKey, byte[] encryptionKey) = KeyDerivation.DeriveKeysSIV(key);
            byte[] tag = Tag.Read(ciphertext);
            ciphertext = Tag.Remove(ciphertext);
            byte[] nonce = Tag.GetNonce(tag);
            byte[] message = StreamEncryption.DecryptXChaCha20(ciphertext, nonce, encryptionKey);
            byte[] tagMessage = Arrays.Concat(additionalData, message, BitConversion.GetBytes(additionalData.Length), BitConversion.GetBytes(message.Length));
            byte[] computedTag = Tag.Compute(tagMessage, macKey);
            bool validTag = Utilities.Compare(tag, computedTag);
            if (!validTag)
            {
                CryptographicOperations.ZeroMemory(message);
                CryptographicOperations.ZeroMemory(computedTag);
                throw new CryptographicException();
            }
            return message;
        }
    }
}
