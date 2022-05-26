﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Client.Helpers;
using Client.Interfaces;

namespace Client.Crypt
{
    public class AESCrypt : ISymmCrypt
    {
        IShowInfo showInfo = new ShowInfo();

        public async Task<byte[]> Encrypt(string message, byte[] key)
        {
            try
            {
                byte[] enc_byte;

                using (Aes aes = Aes.Create())
                {
                    aes.GenerateIV();
                    aes.Key = key;

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        memStream.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                        {
                            using (StreamWriter encryptWriter = new StreamWriter(cryptoStream))
                            {
                                await encryptWriter.WriteAsync(message);
                            }
                        }

                        enc_byte = memStream.ToArray();
                    }
                }

                return enc_byte;
            }
            catch (Exception ex)
            {
                showInfo.ShowMessage($"The encryption string AES failed - {ex}", 3);
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> Decrypt(byte[] enc_mess, byte[] key)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    int BlockSize = aes.BlockSize / 8;

                    byte[] iv = new byte[BlockSize];
                    byte[] mess = new byte[enc_mess.Length - BlockSize];

                    for (int i = 0; i < BlockSize; i++)
                        iv[i] = enc_mess[i];

                    for (int i = BlockSize, j = 0; i < enc_mess.Length; i++, j++)
                        mess[j] = enc_mess[i];

                    string decryptedMessage;

                    using (MemoryStream memStream = new MemoryStream(mess))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            using (StreamReader decryptReader = new StreamReader(cryptoStream))
                            {
                                decryptedMessage = await decryptReader.ReadToEndAsync();
                            }
                        }
                    }

                    return decryptedMessage;
                }

            }
            catch (Exception)
            {
                throw new Exception("Было принято сообщение, но его не удалось расшифровать");
                //showInfo.ShowMessage($"The decryption string AES failed - {ex}", 3);
                //throw new Exception(ex.Message);
            }
            //catch (Exception ex)
            //{
            //    showInfo.ShowMessage($"The decryption string AES failed - {ex}", 3);
            //    throw new Exception(ex.Message);
            //}

        }
    }
}