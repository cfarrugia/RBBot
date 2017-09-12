using RBBot.Core.Database;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading.Triangulation
{
    /// <summary>
    /// An adaptation of Johnson algorithm to find cycles.
    /// </summary>
    public class CycleFinder
    {
        private Exchange exchange;
        private int[,] graph = null;
        private Dictionary<int, Currency> currencyDict = new Dictionary<int, Currency>();
        private List<int[]> cycles = new List<int[]>();

        public CycleFinder(Exchange exchange)
        {
            this.exchange = exchange;

            currencyDict = new Dictionary<int, Currency>();
            using (var ctx = new RBBotContext())
            {
                currencyDict = ctx.Currencies.ToDictionary(x => x.Id, y => y);
            }

            // Get an array of the trade pairs in a jagged array
            graph = new int[exchange.ExchangeTradePairs.Count, 2];

            for (int i = 0; i < exchange.ExchangeTradePairs.Count; i++)
            {
                graph[i, 0] = exchange.ExchangeTradePairs.ElementAt(i).TradePair.FromCurrencyId;
                graph[i, 1] = exchange.ExchangeTradePairs.ElementAt(i).TradePair.ToCurrencyId;
            }

        }
        /// <summary>
        /// This method returns a list of cycles (being a list of currencies each cycle)
        /// </summary>
        public List<List<Currency>> GetCycles()
        {
            // Populate the cycles.
            for (int i = 0; i < graph.GetLength(0); i++)
                for (int j = 0; j < graph.GetLength(1); j++)
                {
                    findNewCycles(new int[] { graph[i, j] });
                }

            // For each cycle, return the currency lists. 
            var curCycles = new List<List<Currency>>();
            foreach (var cycle in cycles)
            {
                curCycles.Add(cycle.Select(x => currencyDict[x]).ToList());
            }

            return curCycles;
        }

        private void findNewCycles(int[] path)
        {
            int n = path[0];
            int x;
            int[] sub = new int[path.Length + 1];

            for (int i = 0; i < graph.GetLength(0); i++)
                for (int y = 0; y <= 1; y++)
                    if (graph[i, y] == n)
                    //  edge referes to our current node
                    {
                        x = graph[i, (y + 1) % 2];
                        if (!visited(x, path))
                        //  neighbor node not on path yet
                        {
                            sub[0] = x;
                            Array.Copy(path, 0, sub, 1, path.Length);
                            //  explore extended path
                            findNewCycles(sub);
                        }
                        else if ((path.Length > 2) && (x == path[path.Length - 1]))
                        //  cycle found
                        {
                            int[] p = normalize(path);
                            int[] inv = invert(p);
                            if (isNew(p) && isNew(inv))
                                cycles.Add(p);
                        }
                    }
        }

        private bool equals(int[] a, int[] b)
        {
            bool ret = (a[0] == b[0]) && (a.Length == b.Length);

            for (int i = 1; ret && (i < a.Length); i++)
                if (a[i] != b[i])
                {
                    ret = false;
                }

            return ret;
        }

        private int[] invert(int[] path)
        {
            int[] p = new int[path.Length];

            for (int i = 0; i < path.Length; i++)
                p[i] = path[path.Length - 1 - i];

            return normalize(p);
        }

        //  rotate cycle path such that it begins with the smallest node
        private int[] normalize(int[] path)
        {
            int[] p = new int[path.Length];
            int x = smallest(path);
            int n;

            Array.Copy(path, 0, p, 0, path.Length);

            while (p[0] != x)
            {
                n = p[0];
                Array.Copy(p, 1, p, 0, p.Length - 1);
                p[p.Length - 1] = n;
            }

            return p;
        }

        private bool isNew(int[] path)
        {
            bool ret = true;

            foreach (int[] p in cycles)
                if (equals(p, path))
                {
                    ret = false;
                    break;
                }

            return ret;
        }

        private int smallest(int[] path)
        {
            int min = path[0];

            foreach (int p in path)
                if (p < min)
                    min = p;

            return min;
        }

        private bool visited(int n, int[] path)
        {
            bool ret = false;

            foreach (int p in path)
                if (p == n)
                {
                    ret = true;
                    break;
                }

            return ret;
        }
    }
}
