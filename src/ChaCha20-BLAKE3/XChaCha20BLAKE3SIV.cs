﻿using System.Text;
using System.Security.Cryptography;
using Sodium;

/*
    ChaCha20-BLAKE3: Committing ChaCha20-BLAKE3, XChaCha20-BLAKE3, and XChaCha20-BLAKE3-SIV AEAD implementations.
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
    public static class XChaCha20BLAKE3SIV
    {
        public const int KeySize = Constants.KeySize;
        public const int TagSize = Constants.TagSize;
        private static readonly byte[] EncryptionContext = Encoding.UTF8.GetBytes("XChaCha20-BLAKE3-SIV 16/12/2021 14:54 XChaCha20.Encrypt()");
        private static readonly byte[] AuthenticationContext = Encoding.UTF8.GetBytes("XChaCha20-BLAKE3-SIV 16/12/2021 14:55 BLAKE3.KeyedHash()");

        public static byte[] Encrypt(byte[] message, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Message(message);
            ParameterValidation.Key(key, KeySize);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(key, nonce: null, EncryptionContext, AuthenticationContext);
            byte[] tag = Tag.Compute(additionalData, message, macKey);
            byte[] nonce = Tag.GetNonce(tag);
            byte[] ciphertext = StreamEncryption.EncryptXChaCha20(message, nonce, encryptionKey);
            return Arrays.Concat(ciphertext, tag);
        }

        public static byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] additionalData = null)
        {
            ParameterValidation.Ciphertext(ciphertext);
            ParameterValidation.Key(key, KeySize);
            additionalData = ParameterValidation.AdditionalData(additionalData);
            (byte[] encryptionKey, byte[] macKey) = KeyDerivation.DeriveKeys(key, nonce: null, EncryptionContext, AuthenticationContext);
            byte[] tag = Tag.Read(ciphertext);
            ciphertext = Tag.Remove(ciphertext);
            byte[] nonce = Tag.GetNonce(tag);
            byte[] message = StreamEncryption.DecryptXChaCha20(ciphertext, nonce, encryptionKey);
            byte[] computedTag = Tag.Compute(additionalData, message, macKey);
            bool validTag = Utilities.Compare(tag, computedTag);
            if (validTag) { return message; }
            CryptographicOperations.ZeroMemory(message);
            CryptographicOperations.ZeroMemory(computedTag);
            throw new CryptographicException();
        }
    }
}
