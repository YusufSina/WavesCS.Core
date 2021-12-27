﻿using System;
using System.IO;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS.Core
{
    public class AliasTransaction : Transaction
    {
        public string Alias { get; }
        public override byte Version { get; set; } = 2;

        public AliasTransaction(byte[] senderPublicKey, string alias, char chainId, decimal fee = 0.001m) : 
            base(chainId, senderPublicKey)
        {
            Alias = alias;
            Fee = fee;
        }

        public AliasTransaction(DictionaryObject tx) : base(tx)
        {
            Alias = tx.GetString("alias");
            Version = tx.GetByte("version");
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
            ChainId = tx.ContainsKey("chainId") ? tx.GetChar("chainId") : '\0';
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Alias);

                if (Version > 1)
                    writer.WriteByte(Version);

                writer.Write(SenderPublicKey);
                writer.WriteShort(Alias.Length + 4);
                writer.Write((byte) 0x02);
                writer.Write((byte) ChainId);
                writer.WriteShort(Alias.Length);
                writer.Write(Encoding.ASCII.GetBytes(Alias));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override byte[] GetBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            if (Version == 1)
            {
                writer.Write(GetBody());
                writer.Write(Proofs[0]);
            }
            else
            {
                writer.WriteByte(0);
                writer.Write(GetBody());
                writer.Write(GetProofsBytes());
            }

            return stream.ToArray();
        }

        internal override byte[] GetBytesForId()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Alias);
                writer.Write((byte)0x02);
                writer.Write((byte)ChainId);
                writer.WriteShort(Alias.Length);
                writer.Write(Encoding.UTF8.GetBytes(Alias));
                return stream.ToArray();
            }
        }

        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
                {
                    {"version", Version },
                    {"type", (byte) TransactionType.Alias},
                    {"senderPublicKey", SenderPublicKey.ToBase58()},
                    {"alias", Alias},
                    {"fee", Assets.WAVES.AmountToLong(Fee)},
                    {"timestamp", Timestamp.ToLong()}
                };

            if (Sender != null)
                result.Add("sender", AddressEncoding.GetAddressFromPublicKey(SenderPublicKey, ChainId));

            return result;
        }

        protected override bool SupportsProofs()
        {
            return Version > 1;
        }
    }
}