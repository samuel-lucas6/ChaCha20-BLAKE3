using System;

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
    internal static class Tag
    {
        internal static byte[] Read(byte[] ciphertext)
        {
            byte[] tag = new byte[Constants.TagLength];
            Array.Copy(ciphertext, ciphertext.Length - tag.Length, tag, destinationIndex: 0, tag.Length);
            return tag;
        }

        internal static byte[] Remove(byte[] ciphertextWithTag)
        {
            byte[] ciphertext = new byte[ciphertextWithTag.Length - Constants.TagLength];
            Array.Copy(ciphertextWithTag, sourceIndex: 0, ciphertext, destinationIndex: 0, ciphertext.Length);
            return ciphertext;
        }
    }
}
