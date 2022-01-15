[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/blob/main/LICENSE)
[![CodeQL](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/actions)

# ChaCha20-BLAKE3
Committing ChaCha20-BLAKE3, XChaCha20-BLAKE3, and XChaCha20-BLAKE3-SIV AEAD implementations using [libsodium](https://doc.libsodium.org/) and [BLAKE3](https://github.com/BLAKE3-team/BLAKE3).

## Features
This library does several things for you:
- Derives a 256-bit encryption key and 256-bit MAC key based on the key and nonce.
- Supports additional data in the calculation of the authentication tag, unlike most Encrypt-then-MAC implementations.
- Appends the authentication tag to the ciphertext.
- Compares the authentication tags in constant time during decryption and only returns plaintext if they match.
- Offers access to an SIV implementation that does not take a nonce.

## Justification
The popular AEADs in use today, such as (X)ChaCha20-Poly1305, AES-GCM, AES-GCM-SIV, XSalsa20-Poly1305, AES-OCB, and so on, are not key or message committing. This means it is possible to decrypt a ciphertext using [multiple keys](https://youtu.be/3M1jIO-jLHI) without an authentication error, which can lead to [partitioning oracle attacks](https://eprint.iacr.org/2020/1491.pdf) and [deanonymisation](https://github.com/LoupVaillant/Monocypher/issues/218#issuecomment-886997371) in certain online scenarios. Furthermore, if an attacker knows the key, then they can find other messages that have the [same tag](https://neilmadden.blog/2021/02/16/when-a-kem-is-not-enough/).

This library was created because there are currently no standardised committing AEAD schemes, adding the commitment property to a non-committing AEAD requires using a MAC, and Encrypt-then-MAC offers improved security guarantees.

Finally, (X)ChaCha20-BLAKE3 is a good combination for an Encrypt-then-MAC scheme because:
1. ChaCha20 has a [higher security margin](https://eprint.iacr.org/2019/1492.pdf) than AES, performs well on older devices, and runs in [constant time](https://cr.yp.to/chacha/chacha-20080128.pdf), [unlike](https://cr.yp.to/antiforgery/cachetiming-20050414.pdf) AES.
2. BLAKE3 is [fast](https://github.com/BLAKE3-team/BLAKE3-specs/blob/master/blake3.pdf) and evolved from BLAKE, which received a [significant amount of cryptanalysis](https://nvlpubs.nist.gov/nistpubs/ir/2012/NIST.IR.7896.pdf), even more than Keccak (the SHA3 finalist), as part of the [SHA3 competition](https://competitions.cr.yp.to/sha3.html).

## Installation
1. Install the [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) and [Blake3.NET](https://www.nuget.org/packages/Blake3/) NuGet packages for your project in [Visual Studio](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio).
2. Download the latest [release](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/releases/latest).
3. Move the downloaded `.dll` file into your project folder.
4. Click on the `Project` tab and `Add Project Reference...` in Visual Studio.
5. Go to `Browse`, click the `Browse` button, and select the downloaded `.dll` file.
6. Add `using ChaCha20Blake3;` to the top of each code file that will use the library.

Note that the [libsodium](https://doc.libsodium.org/) library requires the [Visual C++ Redistributable for Visual Studio 2015-2019](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) to work on Windows. If you want your program to be portable, then you must keep the relevant (x86 or x64) `vcruntime140.dll` file in the same folder as your executable on Windows.

## Usage
### ChaCha20-BLAKE3
⚠️**WARNING: Never reuse a nonce with the same key.**
```c#
const string filePath = "C:\\Users\\samuel-lucas6\\Pictures\\test.jpg";
const string version = "application v1.0.0";

// The message does not have to be a file
byte[] message = File.ReadAllBytes(filePath);

// The nonce should be a counter that gets incremented for each message encrypted using the same key
byte[] nonce = new byte[ChaCha20BLAKE3.NonceSize];

// The key can be random or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(ChaCha20BLAKE3.KeySize);

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = Encoding.UTF8.GetBytes(version);

// Encrypt the message
byte[] ciphertext = ChaCha20BLAKE3.Encrypt(message, nonce, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = ChaCha20BLAKE3.Decrypt(ciphertext, nonce, key, additionalData);
```

### XChaCha20-BLAKE3
⚠️**WARNING: Never reuse a nonce with the same key.**
```c#
const string filePath = "C:\\Users\\samuel-lucas6\\Pictures\\test.jpg";
const string version = "application v1.0.0";

// The message does not have to be a file
byte[] message = File.ReadAllBytes(filePath);

// The nonce can be random. Increment or randomly generate the nonce for each message encrypted using the same key
byte[] nonce = SodiumCore.GetRandomBytes(XChaCha20BLAKE3.NonceSize);

// The key can be random or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(XChaCha20BLAKE3.KeySize);

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = Encoding.UTF8.GetBytes(version);

// Encrypt the message
byte[] ciphertext = XChaCha20BLAKE3.Encrypt(message, nonce, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20BLAKE3.Decrypt(ciphertext, nonce, key, additionalData);
```

### XChaCha20-BLAKE3-SIV
⚠️**WARNING: Never reuse a key. As a precaution, you can use at least 16 bytes of unique, random data as part of the additional data to act as a nonce.**
```c#
const string filePath = "C:\\Users\\samuel-lucas6\\Pictures\\test.jpg";

// The message does not have to be a file
byte[] message = File.ReadAllBytes(filePath);

// The key can be random or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(XChaCha20BLAKE3SIV.KeySize);

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = SodiumCore.GetRandomBytes(XChaCha20BLAKE3SIV.KeySize / 2);

// Encrypt the message
byte[] ciphertext = XChaCha20BLAKE3SIV.Encrypt(message, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20BLAKE3SIV.Decrypt(ciphertext, key, additionalData);
```

## Benchmarks
TODO for v3.
