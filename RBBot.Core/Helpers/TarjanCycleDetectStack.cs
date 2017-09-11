using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Helpers
{

    public class TarjanCycleDetectStack<TValue>
    {
        protected List<List<Vertex<TValue>>> _StronglyConnectedComponents;
        protected Stack<Vertex<TValue>> _Stack;
        protected int _Index;

        public List<List<Vertex<TValue>>> DetectCycle(List<Vertex<TValue>> graph_nodes)
        {
            _StronglyConnectedComponents = new List<List<Vertex<TValue>>>();

            _Index = 0;
            _Stack = new Stack<Vertex<TValue>>();

            foreach (Vertex<TValue> v in graph_nodes)
            {
                if (v.Index < 0)
                {
                    StronglyConnect(v);
                }
            }

            return _StronglyConnectedComponents;
        }

        private void StronglyConnect(Vertex<TValue> v)
        {
            v.Index = _Index;
            v.Lowlink = _Index;

            _Index++;
            _Stack.Push(v);

            foreach (Vertex<TValue> w in v.Dependencies)
            {
                if (w.Index < 0)
                {
                    StronglyConnect(w);
                    v.Lowlink = Math.Min(v.Lowlink, w.Lowlink);
                }
                else if (_Stack.Contains(w))
                {
                    v.Lowlink = Math.Min(v.Lowlink, w.Index);
                }
            }

            if (v.Lowlink == v.Index)
            {
                List<Vertex<TValue>> cycle = new List<Vertex<TValue>>();
                Vertex<TValue> w;

                do
                {
                    w = _Stack.Pop();
                    cycle.Add(w);
                } while (v != w);

                _StronglyConnectedComponents.Add(cycle);
            }
        }
    }

}