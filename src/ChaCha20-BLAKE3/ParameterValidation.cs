using System;

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
    internal static class ParameterValidation
    {
        internal static void Message(byte[] message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "The message cannot be null.");
            }
        }

        internal static void Ciphertext(byte[] ciphertext)
        {
            int validLength = Constants.TagLength + 1;
            if (ciphertext.Length < validLength)
            {
                throw new ArgumentOutOfRangeException(nameof(ciphertext), (ciphertext == null) ? 0 : ciphertext.Length, $"The ciphertext must be at least {validLength} bytes in length.");
            }
        }

        internal static void Nonce(byte[] nonce, int validNonceLength)
        {
            if (nonce == null || nonce.Length != validNonceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(nonce), (nonce == null) ? 0 : nonce.Length, $"The nonce must be {validNonceLength} bytes in length.");
            }
        }

        internal static void Key(byte[] key, int validKeyLength)
        {
            if (key == null || key.Length != validKeyLength)
            {
                throw new ArgumentOutOfRangeException(nameof(key), (key == null) ? 0 : key.Length, $"The key must be {validKeyLength} bytes in length.");
            }
        }

        internal static byte[] AdditionalData(byte[] additionalData)
        {
            // Additional data can be null
            return additionalData ?? (Array.Empty<byte>());
        }
    }
}
