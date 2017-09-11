using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Helpers
{
    public class Vertex<TValue>
    {
        public int Index { get; set; }
        public int Lowlink { get; set; }

        public TValue Value { get; private set; }

        public HashSet<Vertex<TValue>> Dependencies { get; private set; }

        public Vertex(TValue value)
        {
            Index = -1;
            Lowlink = -1;
            Dependencies = new HashSet<Vertex<TValue>>();
            this.Value = value;
        }

        public override string ToString()
        {
            return string.Format("Vertex: {0}", this.Value.ToString());
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Vertex<TValue> other = obj as Vertex<TValue>;


            if (other == null)
                return false;

            return this.Value.Equals(other.Value);
        }
    }

}
