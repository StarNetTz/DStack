﻿using System;
using System.Threading.Tasks;

namespace DStack.Projections
{
    public class Checkpoint
    {
        public string Id { get; set; }
        public ulong Value { get; set; }
    }
}
