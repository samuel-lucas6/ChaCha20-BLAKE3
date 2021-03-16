[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/samuel-lucas6/Geralt/blob/main/LICENSE)

# ChaCha20-BLAKE3
A committing AEAD implementation using [libsodium](https://doc.libsodium.org/) and [BLAKE3](https://github.com/BLAKE3-team/BLAKE3). This library supports both [ChaCha20](https://doc.libsodium.org/advanced/stream_ciphers/chacha20) and [XChaCha20](https://doc.libsodium.org/advanced/stream_ciphers/xchacha20).

## Should I use this?
*It depends*. This implementation has not been standardised like [ChaCha20-Poly1305](https://tools.ietf.org/html/rfc7539). If that is important to you, then obviously avoid using this.

However, ChaCha20-BLAKE3 is essentially Encrypt-then-MAC with keyed BLAKE3 rather than HMAC. To put things in perspective, the [Signal protocol](https://www.signal.org/docs/specifications/doubleratchet/#recommended-cryptographic-algorithms) uses Encrypt-then-MAC with AES-CBC and HMAC.

## Why should I use this?
This library does several things for you:

- Derives a unique 256-bit encryption key and 256-bit MAC key based on the (master) key and nonce.
- Appends the authentication tag to the ciphertext.
- Compares the authentication tags in constant time during decryption and only returns plaintext if they match.

## What is wrong with ChaCha20-Poly1305?
1. ChaCha20-Poly1305 is not key committing, meaning it is possible to decrypt a ciphertext using [multiple keys](https://eprint.iacr.org/2020/1491.pdf). The recommended approach for avoiding this problem (zero padding) has to be manually implemented, is potentially vulnerable to timing attacks, and will slow down decryption.
2. Poly1305 produces a 128-bit tag, which is rather short. The recommended hash length is typically 256-bit because that offers 128-bit security.

## How does ChaCha20-BLAKE3 solve these problems?
1. This implementation is key committing because it uses keyed BLAKE3 and both the encryption key and MAC key are derived from the same master key.
2. This implementation uses a 256-bit tag, which offers improved security guarantees.

## How do I use this?
1. Install the [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) and [Blake3.NET](https://www.nuget.org/packages/Blake3/) NuGet packages in [Visual Studio](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio).
2. Download the latest [release](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/releases).
3. Move the downloaded DLL file into your Visual Studio project folder.
3. Click on the ```Project``` tab and ```Add Project Reference...``` in Visual Studio.
4. Go to ```Browse```, click the ```Browse``` button, and select the downloaded DLL file.

Note that the [libsodium](https://doc.libsodium.org/) library requires the [Visual C++ Redistributable for Visual Studio 2015-2019](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) to work on Windows. If you want your program to be portable, you must keep the ```vcruntime140.dll file``` in the same folder as the executable on Windows.

### ChaCha20
⚠️**WARNING: Never reuse a nonce with the same key.**
```c#
const string filePath = "C:\\Users\\samuel-lucas6\\Pictures\\test.jpg";
const int nonceLength = 8;
const int keyLength = 32;
const string version = "application v1.0.0";

// The message does not have to be a file
byte[] message = File.ReadAllBytes(filePath);

// The nonce should be a counter that gets incremented for each message encrypted using the same key
byte[] nonce = new byte[nonceLength];

// The key can be random or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(keyLength);

// The additional data can be null or version numbers, timestamps, etc
byte[] additionalData = Encoding.UTF8.GetBytes(version);

// Encrypt the message
byte[] ciphertext = ChaCha20_BLAKE3.Encrypt(message, nonce, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = ChaCha20_BLAKE3.Decrypt(ciphertext, nonce, key, additionalData);
```

### XChaCha20
⚠️**WARNING: Never reuse a nonce with the same key.**
```c#
const string filePath = "C:\\Users\\samuel-lucas6\\Pictures\\test.jpg";
const int nonceLength = 24;
const int keyLength = 32;
const string version = "application v1.0.0";

// The message does not have to be a file
byte[] message = File.ReadAllBytes(filePath);

// The nonce can be random. Increment the nonce for each message encrypted using the same key
byte[] nonce = SodiumCore.GetRandomBytes(nonceLength);

// The key can be random or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(keyLength);

// The additional data can be null or version numbers, timestamps, etc
byte[] additionalData = Encoding.UTF8.GetBytes(version);

// Encrypt the message
byte[] ciphertext = XChaCha20_BLAKE3.Encrypt(message, nonce, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20_BLAKE3.Decrypt(ciphertext, nonce, key, additionalData);
```

## How fast is it?
ChaCha20-BLAKE3 is faster than ChaCha20-Poly1305 for large inputs but slightly slower for small-medium sized messages.

The following benchmarks were done using [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet/) in a .NET 5 console application with 16 bytes of additional data.

34.1 MiB JPG file:
|                 Function |     Mean |    Error |   StdDev |
|------------------------- |----------|----------|----------|
| **ChaCha20-BLAKE3.Encrypt** | **46.93 ms** | **0.791 ms** | **0.701 ms** |
| **ChaCha20-BLAKE3.Decrypt** | **47.97 ms** | **0.937 ms** | **1.079 ms** |
| ChaCha20-Poly1305.Encrypt | 57.25 ms | 1.113 ms | 1.143 ms |
| ChaCha20-Poly1305.Decrypt | 56.65 ms | 0.841 ms | 0.787 ms |
| ChaCha20-Poly1305.Encrypt (with padding fix) | 69.26 ms | 0.620 ms | 0.550 ms |
| ChaCha20-Poly1305.Decrypt (with padding fix) | 69.33 ms | 0.782 ms | 0.732 ms |

16.1 KiB Kryptor file:
|                 Function |     Mean |    Error |   StdDev |
|------------------------- |----------|----------|----------|
| **ChaCha20-BLAKE3.Encrypt** | **21.74 us** | **0.043 us** | **0.038 us** |
| **ChaCha20-BLAKE3.Decrypt** | **21.61 us** | **0.190 us** | **0.158 us** |
| ChaCha20-Poly1305.Encrypt | 17.49 us | 0.034 us | 0.027 us |
| ChaCha20-Poly1305.Decrypt | 17.48 us | 0.020 us | 0.016 us |
| ChaCha20-Poly1305.Encrypt (with padding fix) | 18.23 us | 0.034 us | 0.029 us |
| ChaCha20-Poly1305.Decrypt (with padding fix) | 18.46 us | 0.030 us | 0.024 us |

128 byte text file:
|                 Function |     Mean |    Error |   StdDev |
|------------------------- |----------|----------|----------|
| **ChaCha20-BLAKE3.Encrypt** | **1.109 us** | **0.0038 us** | **0.0032 us** |
| **ChaCha20-BLAKE3.Decrypt** | **1.168 us** | **0.0087 us** | **0.0082 us** |
| ChaCha20-Poly1305.Encrypt | 518.4 ns | 0.67 ns | 0.59 ns |
| ChaCha20-Poly1305.Decrypt | 542.5 ns | 0.59 ns | 0.55 ns |
| ChaCha20-Poly1305.Encrypt (with padding fix) | 651.6 ns | 4.48 ns | 4.19 ns |
| ChaCha20-Poly1305.Decrypt (with padding fix) | 695.0 ns | 0.46 ns | 0.38 ns |
