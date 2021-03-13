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
    internal static class KeyDerivation
    {
        internal static (byte[] encryptionKey, byte[] macKey) DeriveKeys(byte[] nonce, byte[] inputKeyingMaterial)
        {
            byte[] encryptionKey = DeriveKey(Constants.EncryptionContext, nonce, inputKeyingMaterial);
            byte[] macKey = DeriveKey(Constants.AuthenticationContext, nonce, inputKeyingMaterial);
            return (encryptionKey, macKey);
        }

        private static byte[] DeriveKey(byte[] context, byte[] nonce, byte[] inputKeyingMaterial)
        {
            using var blake3 = Hasher.NewDeriveKey(context);
            blake3.Update(nonce);
            blake3.Update(inputKeyingMaterial);
            var key = blake3.Finalize();
            return key.AsSpan().ToArray();
        }
    }
}
