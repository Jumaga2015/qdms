﻿// -----------------------------------------------------------------------
// <copyright file="StoredDataInfo.cs" company="">
// Copyright 2013 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;
using ProtoBuf;

namespace QDMS
{
    [ProtoContract]
    public class StoredDataInfo
    {
        [ProtoMember(1)]
        public int InstrumentID { get; set; }

        [ProtoMember(2)]
        public BarSize Frequency { get; set; }

        [Column(TypeName = "datetime(3)")]
        public DateTime EarliestDate { get; set; }

        [Column(TypeName = "datetime(3)")]
        public DateTime LatestDate { get; set; }

        [ProtoMember(3)]
        [NotMapped]
        public long EarliestDateAsLong
        {
            get
            {
                return EarliestDate.Ticks;
            }
            set
            {
                EarliestDate = DateTime.FromBinary(value);
            }
        }

        [ProtoMember(4)]
        [NotMapped]
        public long LatestDateAsLong
        {
            get
            {
                return LatestDate.Ticks;
            }
            set
            {
                LatestDate = DateTime.FromBinary(value);
            }
        }
    }
}
