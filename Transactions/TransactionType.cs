﻿using System;

namespace WavesCS.Core
{
    public enum TransactionType : byte
    {
        Issue = 3,
        Transfer = 4,
        Reissue = 5,
        Burn = 6,
        Exchange = 7,
        Lease = 8,
        LeaseCancel = 9,
        Alias = 10,
        MassTransfer = 11,
        DataTx = 12,
        SetScript = 13,
        SponsoredFee = 14,
        SetAssetScript = 15,
        InvokeScript = 16
    }
}
