using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading.Triangulation
{
    /// <summary>
    /// This is a pretty simple observer. It keeps track of the current market price for each exchange pair and seeks any possible triangulation within the same exchange
    /// 
    /// </summary>
    public class TriangulationManager
    {
        #region Singleton initialization

        private static volatile TriangulationManager instance;
        private static object syncRoot = new Object();

        private TriangulationManager() { }

        /// <summary>
        /// We just want one instance of the market price observer.
        /// </summary>
        public static TriangulationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TriangulationManager();
                    }
                }

                return instance;
            }
        }

        #endregion
    }
}
