using System.Buffers.Binary;
using System.Security.Cryptography;

var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
var salt = RandomNumberGenerator.GetBytes(32);
var key = Rfc2898DeriveBytes.Pbkdf2(secret, salt, 100_000, HashAlgorithmName.SHA512, 64);

var payload = new byte[13 + salt.Length + key.Length];
payload[0] = 0x01;
BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(1), 2);
BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(5), 100_000);
BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(9), (uint)salt.Length);
salt.CopyTo(payload, 13);
key.CopyTo(payload, 13 + salt.Length);

Console.WriteLine($"Secret: {secret}");
Console.WriteLine($"Hash: {Convert.ToBase64String(payload)}");
