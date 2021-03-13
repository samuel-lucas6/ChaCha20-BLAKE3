using System.Security.Cryptography;
using Sodium;
using Blake3;

/*
    ChaCha20-BLAKE3: A committing AEAD implementation.
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
    public static class XChaCha20_BLAKE3
    {
        /// <summary>Encrypts a message using XChaCha20-BLAKE3.</summary>
        /// <param name="message">The message to encrypt.</param>
        /// <param name="nonce">The 24 byte nonce.</param>
        /// <param name="key">The 32 byte key.</param>
        /// <param name="additionalData">Optional additional data to authenticate.</param>
        /// <remarks>Never reuse a nonce with the same key. A random nonce is recommended.</remarks>
        /// <returns>The ciphertext and tag.</returns>
        public static byte[] Encrypt(byte[] message, byte[] nonce, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Message(message);
            ParameterValidation.Nonce(nonce, Constants.XChaChaNonceLength);
            ParameterValidation.Key(key, Constants.KeyLength);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(nonce, key);
            byte[] ciphertext = StreamEncryption.EncryptXChaCha20(message, nonce, encryptionKey);
            byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, Arrays.ConvertLength(additionalData.Length), Arrays.ConvertLength(ciphertext.Length));
            using var blake3 = Hasher.NewKeyed(macKey);
            blake3.UpdateWithJoin(tagMessage);
            var tag = blake3.Finalize();
            return Arrays.Concat(ciphertext, tag.AsSpan().ToArray());
        }

        /// <summary>Decrypts a ciphertext message using XChaCha20-BLAKE3.</summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="nonce">The 24 byte nonce.</param>
        /// <param name="key">The 32 byte key.</param>
        /// <param name="additionalData">Optional additional data to authenticate.</param>
        /// <returns>The decrypted message.</returns>
        public static byte[] Decrypt(byte[] ciphertext, byte[] nonce, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Ciphertext(ciphertext);
            ParameterValidation.Nonce(nonce, Constants.XChaChaNonceLength);
            ParameterValidation.Key(key, Constants.KeyLength);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(nonce, key);
            byte[] tag = Tag.Read(ciphertext);
            ciphertext = Tag.Remove(ciphertext);
            byte[] tagMessage = Arrays.Concat(additionalData, ciphertext, Arrays.ConvertLength(additionalData.Length), Arrays.ConvertLength(ciphertext.Length));
            using var blake3 = Hasher.NewKeyed(macKey);
            blake3.UpdateWithJoin(tagMessage);
            var computedTag = blake3.Finalize();
            bool validTag = Utilities.Compare(tag, computedTag.AsSpan().ToArray());
            return !validTag ? throw new CryptographicException() : StreamEncryption.DecryptXChaCha20(ciphertext, nonce, encryptionKey);
        }
    }
}
