# 摘要与加解密工具

`YangEncrypt.cs` 提供 MD5 摘要、DES 文本加解密和 RSA 密钥生成及加解密的静态方法。

## 主要接口

- `MD5Encrypt.Encrypt`：字符串摘要；`AbstractFile`：计算文件或流的摘要。
- `DesEncrypt.Encrypt` / `Decrypt`：DES 文本加解密。
- `RsaEncrypt.GetKeyPair`、`Encrypt`、`Decrypt`：生成密钥对及 RSA 加解密。

## 使用

```csharp
string digest = MD5Encrypt.Encrypt("content");
KeyValuePair<string, string> keys = RsaEncrypt.GetKeyPair();
string cipher = RsaEncrypt.Encrypt("content", keys.Key);
string plain = RsaEncrypt.Decrypt(cipher, keys.Value);
```

摘要不可逆，不能用来恢复原文；MD5 与 DES 属于旧算法，不应单独用于保护敏感数据。RSA 加密和解密应使用匹配的公钥/私钥，并遵守接口对密钥格式和输入长度的要求。不要把生产私钥硬编码进客户端。
