---
description: Avoid using broken hash algorithms
ms.custom: PSSA v1.21.0
ms.date: 05/31/2022
ms.topic: reference
title: AvoidUsingBrokenHashAlgorithms
---
# AvoidUsingBrokenHashAlgorithms

**Severity Level: Medium**

## Description

Avoid using the broken algorithms MD5 or SHA-1.

## How

Replace broken algorithms with secure alternatives. MD5 and SHA-1 should be replaced with SHA256,
SHA384, SHA512, or other safer algorithms when possible, with MD5 and SHA-1 only being utilized by
necessity for backwards compatibility.

## Example 1

### Wrong

```powershell
Get-FileHash foo.txt -Algorithm MD5
```

### Correct

```powershell
Get-FileHash foo.txt -Algorithm SHA256
```

## Example 2

### Wrong

```powershell
Get-FileHash foo.txt -Algorithm SHA1
```

### Correct

```powershell
Get-FileHash foo.txt
```
