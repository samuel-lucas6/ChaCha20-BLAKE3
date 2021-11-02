[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/blob/main/LICENSE)
[![CodeQL](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/actions)

# ChaCha20-BLAKE3
Committing ChaCha20-BLAKE3, XChaCha20-BLAKE3, and XChaCha20-BLAKE3-SIV AEAD implementations using [libsodium](https://doc.libsodium.org/) and [BLAKE3](https://github.com/BLAKE3-team/BLAKE3).

## Features
This library does several things for you:
- Derives a 256-bit encryption key and 256-bit MAC key based on the (master) key.
- Supports additional data in the calculation of the authentication tag, unlike most Encrypt-then-MAC implementations.
- Appends the authentication tag to the ciphertext.
- Compares the authentication tags in constant time during decryption and only returns plaintext if they match.
- Offers access to an SIV implementation that does not take a nonce.

## Justification
ChaCha20-Poly1305 and XChaCha20-Poly1305 are not key or message committing, meaning it is possible to decrypt a ciphertext using [multiple keys](https://eprint.iacr.org/2020/1491.pdf), and if an attacker knows the key, then they can find other messages that have the [same tag](https://neilmadden.blog/2021/02/16/when-a-kem-is-not-enough/).

XSalsa20-Poly1305, AES-GCM, AES-GCM-SIV, AES-OCB, and [likely](https://crypto.stackexchange.com/questions/87779/do-ccm-and-eax-provide-key-commitment) AES-EAX have exactly the same problem. Furthermore, [no](https://www.usenix.org/system/files/sec21_slides_len.pdf) committing AEADs have been standardised. Then although [AEGIS](https://eprint.iacr.org/2013/695.pdf), which was a finalist in the [CAESAR competition](https://competitions.cr.yp.to/caesar-submissions.html), is committing and will be [available](https://github.com/jedisct1/libsodium/issues/1028) in the next libsodium release, it has [poor](https://github.com/jedisct1/libsodium/issues/951#issuecomment-620561064) performance without AES-NI instruction set support and [can](https://jedisct1.github.io/draft-aegis-aead/draft-denis-aegis-aead.html#name-security-considerations) be vulnerable to side-channel attacks, unlike ChaCha20.

There are various fixes for this problem, but they all have to be manually implemented, increase the size of the ciphertext, slow down encryption/decryption to some extent, certain approaches are vulnerable to timing attacks, and the better solutions I have seen involve the use of a non-polynomial MAC (e.g. HMAC), meaning you may as well switch to Encrypt-then-MAC, which can provide improved security guarantees, unless maximum performance or a small tag size is critical.

ChaCha20-BLAKE3 is about the fastest Encrypt-then-MAC combination possible and is even faster than ChaCha20-Poly1305 for large inputs. Moreover, the longer tag and commitment properties provide additional security, and the SIV implementation offers a [more secure](https://eprint.iacr.org/2019/1492.pdf) alternative to AES-SIV.

## Installation
1. Install the [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) and [Blake3.NET](https://www.nuget.org/packages/Blake3/) NuGet packages for your project in [Visual Studio](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio).
2. Download the latest [release](https://github.com/samuel-lucas6/ChaCha20-BLAKE3/releases).
3. Move the downloaded DLL file into your project folder.
4. Click on the ```Project``` tab and ```Add Project Reference...``` in Visual Studio.
5. Go to ```Browse```, click the ```Browse``` button, and select the downloaded DLL file.
6. Add ```using ChaCha20BLAKE3;``` to the top of each code file that will use the library.

Note that the [libsodium](https://doc.libsodium.org/) library requires the [Visual C++ Redistributable for Visual Studio 2015-2019](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) to work on Windows. If you want your program to be portable, then you must keep the relevant (x86 or x64) ```vcruntime140.dll``` file in the same folder as your executable on Windows.

## Usage
### ChaCha20-BLAKE3
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

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = Encoding.UTF8.GetBytes(version);

// Encrypt the message
byte[] ciphertext = ChaCha20_BLAKE3.Encrypt(message, nonce, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = ChaCha20_BLAKE3.Decrypt(ciphertext, nonce, key, additionalData);
```

### XChaCha20-BLAKE3
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

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = Encoding.UTF8.GetBytes(version);

// Encrypt the message
byte[] ciphertext = XChaCha20_BLAKE3.Encrypt(message, nonce, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20_BLAKE3.Decrypt(ciphertext, nonce, key, additionalData);
```

### XChaCha20-BLAKE3-SIV
⚠️**WARNING: A new key should be used for each message. Otherwise, you should include unique, random data as part of the additional data to ensure semantic security.**
```c#
const string filePath = "C:\\Users\\samuel-lucas6\\Pictures\\test.jpg";
const int keyLength = 64;
const int randomAdditionalDataLength = 32;

// The message does not have to be a file
byte[] message = File.ReadAllBytes(filePath);

// The key can be random or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(keyLength);

// The additional data can be null but is ideal for file headers, version numbers, timestamps, etc
byte[] additionalData = SodiumCore.GetRandomBytes(randomAdditionalDataLength);

// Encrypt the message
byte[] ciphertext = XChaCha20_BLAKE3_SIV.Encrypt(message, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20_BLAKE3_SIV.Decrypt(ciphertext, key, additionalData);
```

## Benchmarks
My implementations of ChaCha20-BLAKE3 are faster than ChaCha20-Poly1305 for large inputs but slower for small-medium sized messages. Implementations in other programming languages may be [faster](https://github.com/PaulGrandperrin/XChaCha8Blake3Siv).

The following benchmarks were done using [BenchmarkDotNet](https://benchmarkdotnet.org/) in a .NET 5 console application with 16 bytes of additional data.

#### 34.1 MiB JPG file
|                  Method |     Mean |    Error |   StdDev |
|------------------------ |----------|----------|----------|
| **ChaCha20-BLAKE3.Encrypt()** | **41.58 ms** | **0.466 ms** | **0.436 ms** |
| **ChaCha20-BLAKE3.Decrypt()** | **39.85 ms** | **0.181 ms** | **0.170 ms** |
| ChaCha20-Poly1305.Encrypt() | 51.99 ms | 0.034 ms | 0.027 ms |
| ChaCha20-Poly1305.Decrypt() | 52.02 ms | 0.461 ms | 0.409 ms |

#### 16.1 KiB Kryptor file
|                  Method |     Mean |    Error |   StdDev |
|------------------------ |----------|----------|----------|
| **ChaCha20-BLAKE3.Encrypt()** | **25.33 us** | **0.056 us** | **0.052 us** |
| **ChaCha20-BLAKE3.Decrypt()** | **24.66 us** | **0.055 us** | **0.052 us** |
| ChaCha20-Poly1305.Encrypt() | 17.11 us | 0.140 us | 0.124 us |
| ChaCha20-Poly1305.Decrypt() | 17.11 us | 0.080 us | 0.067 us |

#### 128 byte text file
|                  Method |     Mean |     Error |    StdDev |
|------------------------ |----------|-----------|-----------|
| **ChaCha20-BLAKE3.Encrypt()** | **1.056 us** | **0.0023 us** | **0.0020 us** |
| **ChaCha20-BLAKE3.Decrypt()** | **1.085 us** | **0.0043 us** | **0.0034 us** |
| ChaCha20-Poly1305.Encrypt() | 510.4 ns | 2.77 ns | 2.31 ns |
| ChaCha20-Poly1305.Decrypt() | 527.4 ns | 4.01 ns | 3.55 ns |
