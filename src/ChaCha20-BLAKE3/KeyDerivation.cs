using System;
using Blake3;

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
    internal static class KeyDerivation
    {
        internal static (byte[] encryptionKey, byte[] macKey) DeriveKeys(byte[] inputKeyingMaterial, byte[] encryptionContext, byte[] authenticationContext, byte[] nonce = null)
        {
            byte[] encryptionKey = DeriveKey(inputKeyingMaterial, encryptionContext);
            byte[] macKey = DeriveKey(inputKeyingMaterial, authenticationContext, nonce);
            return (encryptionKey, macKey);
        }

        private static byte[] DeriveKey(byte[] inputKeyingMaterial, byte[] context, byte[] salt = null)
        {
            salt ??= Array.Empty<byte>();
            using var blake3 = Hasher.NewDeriveKey(context);
            blake3.Update(salt);
            blake3.Update(inputKeyingMaterial);
            blake3.Update(BitConversion.GetBytes(salt.Length));
            blake3.Update(BitConversion.GetBytes(inputKeyingMaterial.Length));
            var key = blake3.Finalize();
            return key.AsSpanUnsafe().ToArray();
        }
    }
}