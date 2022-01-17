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
### NuGet
You can find the NuGet package [here](https://www.nuget.org/packages/ChaCha20BLAKE3). The easiest way to install this is via the NuGet Package Manager in [Visual Studio](https://visualstudio.microsoft.com/vs/), as explained [here](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio). [JetBrains Rider](https://www.jetbrains.com/rider/) also has a package manager, and instructions can be found [here](https://www.jetbrains.com/help/rider/Using_NuGet.html).

### Manual
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

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
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

// The nonce can be a counter or randomly generated using a CSPRNG
// Increment or randomly generate the nonce for each message encrypted using the same key
byte[] nonce = SodiumCore.GetRandomBytes(XChaCha20BLAKE3.NonceSize);

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
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

// The key can be randomly generated using a CSPRNG or derived using a KDF (e.g. Argon2, HKDF, etc)
byte[] key = SodiumCore.GetRandomBytes(XChaCha20BLAKE3SIV.KeySize);

// The additional data can be null, used as a nonce, and/or used for file headers, version numbers, timestamps, etc
byte[] additionalData = SodiumCore.GetRandomBytes(XChaCha20BLAKE3SIV.KeySize / 2);

// Encrypt the message
byte[] ciphertext = XChaCha20BLAKE3SIV.Encrypt(message, key, additionalData);

// Decrypt the ciphertext
byte[] plaintext = XChaCha20BLAKE3SIV.Decrypt(ciphertext, key, additionalData);
```

## Benchmarks
The following benchmarks were done using [BenchmarkDotNet](https://benchmarkdotnet.org/) in a [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) console application with 16 bytes of additional data.

In sum, ChaCha20-BLAKE3 performs similarly to ChaCha20-Poly1305 and is even faster with large inputs. Whilst it is slower with small messages, I would argue that the additional security makes up for any performance loss.

### 512 bytes
|                     Method |     Mean |   Error |  StdDev |
|:--------------------------:|:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE3.Encrypt | 1.504 us | 0.0027 us | 0.0024 us |
|      ChaCha20-BLAKE3.Decrypt | 1.530 us | 0.0067 us | 0.0060 us |
|     XChaCha20-BLAKE3.Encrypt | 1.601 us | 0.0012 us | 0.0010 us |
|     XChaCha20-BLAKE3.Decrypt | 1.610 us | 0.0068 us | 0.0060 us |
| XChaCha20-BLAKE3-SIV.Encrypt | 1.593 us | 0.0086 us | 0.0080 us |
| XChaCha20-BLAKE3-SIV.Decrypt | 1.610 us | 0.0009 us | 0.0008 us |
|  ChaCha20-Poly1305.Encrypt | 785.9 ns | 4.59 ns | 4.29 ns |
|  ChaCha20-Poly1305.Decrypt | 793.6 ns | 1.25 ns | 0.98 ns |
| XChaCha20-Poly1305.Encrypt | 865.1 ns | 1.24 ns | 0.97 ns |
| XChaCha20-Poly1305.Decrypt | 888.1 ns | 4.23 ns | 3.75 ns |

### 16 KiB
|                     Method |     Mean |    Error |   StdDev |
|:--------------------------:|:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE3.Encrypt | 25.49 us | 0.039 us | 0.032 us |
|      ChaCha20-BLAKE3.Decrypt | 24.91 us | 0.026 us | 0.024 us |
|     XChaCha20-BLAKE3.Encrypt | 25.75 us | 0.042 us | 0.039 us |
|     XChaCha20-BLAKE3.Decrypt | 24.99 us | 0.051 us | 0.048 us |
| XChaCha20-BLAKE3-SIV.Encrypt | 23.76 us | 0.030 us | 0.028 us |
| XChaCha20-BLAKE3-SIV.Decrypt | 23.88 us | 0.024 us | 0.022 us |
|  ChaCha20-Poly1305.Encrypt | 16.62 us | 0.049 us | 0.041 us |
|  ChaCha20-Poly1305.Decrypt | 16.66 us | 0.096 us | 0.090 us |
| XChaCha20-Poly1305.Encrypt | 16.76 us | 0.069 us | 0.058 us |
| XChaCha20-Poly1305.Decrypt | 16.70 us | 0.020 us | 0.017 us |

### 32 KiB
|                       Method |     Mean |    Error |   StdDev |
|:--------------------------:|:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE3.Encrypt | 40.38 us | 0.060 us | 0.056 us |
|      ChaCha20-BLAKE3.Decrypt | 39.58 us | 0.035 us | 0.033 us |
|     XChaCha20-BLAKE3.Encrypt | 40.53 us | 0.038 us | 0.032 us |
|     XChaCha20-BLAKE3.Decrypt | 39.50 us | 0.048 us | 0.045 us |
| XChaCha20-BLAKE3-SIV.Encrypt | 36.02 us | 0.673 us | 0.661 us |
| XChaCha20-BLAKE3-SIV.Decrypt | 36.91 us | 0.034 us | 0.032 us |
|  ChaCha20-Poly1305.Encrypt | 32.87 us | 0.051 us | 0.043 us |
|  ChaCha20-Poly1305.Decrypt | 32.92 us | 0.187 us | 0.175 us |
| XChaCha20-Poly1305.Encrypt | 32.85 us | 0.046 us | 0.039 us |
| XChaCha20-Poly1305.Decrypt | 33.03 us | 0.093 us | 0.087 us |

### 64 KiB
|                     Method |     Mean |    Error |   StdDev |
|:--------------------------:|:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE3.Encrypt | 63.93 us | 0.098 us | 0.091 us |
|      ChaCha20-BLAKE3.Decrypt | 62.47 us | 0.142 us | 0.133 us |
|     XChaCha20-BLAKE3.Encrypt | 64.31 us | 0.141 us | 0.131 us |
|     XChaCha20-BLAKE3.Decrypt | 62.67 us | 0.084 us | 0.078 us |
| XChaCha20-BLAKE3-SIV.Encrypt | 59.08 us | 0.053 us | 0.049 us |
| XChaCha20-BLAKE3-SIV.Decrypt | 60.09 us | 0.052 us | 0.049 us |
|  ChaCha20-Poly1305.Encrypt | 65.33 us | 0.182 us | 0.142 us |
|  ChaCha20-Poly1305.Decrypt | 65.75 us | 0.529 us | 0.494 us |
| XChaCha20-Poly1305.Encrypt | 65.43 us | 0.236 us | 0.197 us |
| XChaCha20-Poly1305.Decrypt | 65.79 us | 0.584 us | 0.546 us |

### 128 KiB
|                     Method |     Mean |   Error |  StdDev |
|:--------------------------:|:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE3.Encrypt | 208.2 us | 2.65 us | 2.48 us |
|      ChaCha20-BLAKE3.Decrypt | 196.0 us | 2.56 us | 2.39 us |
|     XChaCha20-BLAKE3.Encrypt | 208.1 us | 2.94 us | 2.60 us |
|     XChaCha20-BLAKE3.Decrypt | 197.1 us | 2.65 us | 2.35 us |
| XChaCha20-BLAKE3-SIV.Encrypt | 206.1 us | 2.82 us | 2.64 us |
| XChaCha20-BLAKE3-SIV.Decrypt | 196.4 us | 3.14 us | 2.93 us |
|  ChaCha20-Poly1305.Encrypt | 182.0 us | 1.15 us | 1.08 us |
|  ChaCha20-Poly1305.Decrypt | 180.9 us | 1.49 us | 1.39 us |
| XChaCha20-Poly1305.Encrypt | 180.9 us | 1.13 us | 1.00 us |
| XChaCha20-Poly1305.Decrypt | 181.1 us | 1.45 us | 1.29 us |

### 32 MiB
|                     Method |     Mean |    Error |   StdDev |
|:--------------------------:|:--------:|:--------:|:--------:|
|      ChaCha20-BLAKE3.Encrypt | 44.08 ms | 0.814 ms | 0.937 ms |
|      ChaCha20-BLAKE3.Decrypt | 42.88 ms | 0.736 ms | 0.653 ms |
|     XChaCha20-BLAKE3.Encrypt | 41.90 ms | 0.812 ms | 0.967 ms |
|     XChaCha20-BLAKE3.Decrypt | 42.31 ms | 0.653 ms | 0.579 ms |
| XChaCha20-BLAKE3-SIV.Encrypt | 42.35 ms | 0.821 ms | 1.008 ms |
| XChaCha20-BLAKE3-SIV.Decrypt | 42.39 ms | 0.694 ms | 0.649 ms |
|  ChaCha20-Poly1305.Encrypt | 49.07 ms | 0.289 ms | 0.271 ms |
|  ChaCha20-Poly1305.Decrypt | 48.68 ms | 0.171 ms | 0.143 ms |
| XChaCha20-Poly1305.Encrypt | 49.02 ms | 0.150 ms | 0.140 ms |
| XChaCha20-Poly1305.Decrypt | 48.77 ms | 0.195 ms | 0.173 ms |
