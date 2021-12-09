using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    class DB
    {
        List<int> Indices { get; } = new List<int>();

        public void Add(int i)
        {
            Indices.Add(i);
        }

        public int GenerateNew()
        {
            int i = 0;
            while (Indices.Contains(i))
                i++;
            Add(i);
            return i;
        }

        public void Clear()
        {
            Indices.Clear();
        }
    }
}
