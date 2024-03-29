﻿using System;
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
    internal static class Tag
    {
        internal static byte[] Compute(byte[] additionalData, byte[] ciphertext, byte[] macKey)
        {
            var message = Arrays.Concat(additionalData, ciphertext, BitConversion.GetBytes(additionalData.Length), BitConversion.GetBytes(ciphertext.Length));
            using var blake3 = Hasher.NewKeyed(macKey);
            blake3.UpdateWithJoin(message);
            var tag = blake3.Finalize();
            return tag.AsSpanUnsafe().ToArray();
        }

        internal static byte[] Read(byte[] ciphertext, out byte[] ciphertextWithoutTag)
        {
            var tag = new byte[Constants.TagSize];
            Array.Copy(ciphertext, ciphertext.Length - tag.Length, tag, destinationIndex: 0, tag.Length);
            ciphertextWithoutTag = Remove(ciphertext);
            return tag;
        }

        private static byte[] Remove(byte[] ciphertextWithTag)
        {
            var ciphertext = new byte[ciphertextWithTag.Length - Constants.TagSize];
            Array.Copy(ciphertextWithTag, sourceIndex: 0, ciphertext, destinationIndex: 0, ciphertext.Length);
            return ciphertext;
        }

        internal static byte[] GetNonce(byte[] tag)
        {
            var nonce = new byte[Constants.XChaChaNonceSize];
            Array.Copy(tag, nonce, nonce.Length);
            return nonce;
        }
    }
}