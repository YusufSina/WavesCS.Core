﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS.Core
{
    public class DataTransaction : Transaction
    {
        public DictionaryObject Entries { get; }
        public override byte Version { get; set; } = 1;

        public DataTransaction(char chainId, byte[] senderPublicKey, DictionaryObject entries,
            decimal? fee = null) : base(chainId, senderPublicKey)
        {
            Entries = entries;
            Fee = fee ?? ((GetBody().Length + 70) / 1024 + 1) * 0.001m;
        }

        public DataTransaction(DictionaryObject tx) : base(tx)
        {
            Entries = tx.GetObjects("data")
                        .ToDictionary(o => o.GetString("key"), Node.DataValue);

            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
        }

        public override byte[] GetBody()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.DataTx);
                writer.Write(Version);
                writer.Write(SenderPublicKey);
                writer.WriteShort((short)Entries.Count);

                foreach (var pair in Entries)
                {
                    var key = Encoding.UTF8.GetBytes(pair.Key);
                    writer.WriteShort((short)key.Length);
                    writer.Write(key);
                    writer.WriteObject(pair.Value);
                }

                writer.WriteLong(Timestamp.ToLong());
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                return stream.ToArray();
            }
        }

        public override byte[] GetBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.WriteByte(0);
            writer.Write(GetBody());
            writer.Write(GetProofsBytes());

            return stream.ToArray();
        }

        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject
            {
                {"type", (byte) TransactionType.DataTx},
                {"version", Version},
                {"senderPublicKey", SenderPublicKey.ToBase58() },
                {"data", Entries.Select(pair => new DictionaryObject
                {
                    {"key", pair.Key},
                    {"type", pair.Value is long ? "integer" : (pair.Value is bool ? "boolean" : (pair.Value is string ? "string"  : "binary"))},
                    {"value", pair.Value is byte[] bytes ? bytes.ToBase64() : pair.Value }                    
                })},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()},                
            };

            if (Sender != null)
                result.Add("sender", Sender);

            return result;
        }

        protected override bool SupportsProofs()
        {
            return true;
        }
    }
}