using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonzaBotner.Database
{
    public class Counter
    {
        public ulong UserId { get; set; }

        public ulong Count { get; set; }
    }
}
