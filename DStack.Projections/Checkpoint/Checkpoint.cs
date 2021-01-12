using System;
using System.Threading.Tasks;

namespace DStack.Projections
{
    public class Checkpoint
    {
        public string Id { get; set; }
        public long Value { get; set; }
    }
}
