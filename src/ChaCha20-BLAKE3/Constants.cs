using System.Text;

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
    internal static class Constants
    {
        internal const int KeyLength = 32;
        internal const int ChaChaNonceLength = 8;
        internal const int XChaChaNonceLength = 24;
        internal const int TagLength = 32;
        internal static readonly byte[] EncryptionContext = Encoding.UTF8.GetBytes("ChaCha20-BLAKE3 13/03/2021 14:09:00 ChaCha20.Encrypt");
        internal static readonly byte[] AuthenticationContext = Encoding.UTF8.GetBytes("ChaCha20-BLAKE3 13/03/2021 14:09:15 BLAKE3.KeyedHash");
    }
}
