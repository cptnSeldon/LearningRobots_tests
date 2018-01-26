using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P3_LearningRobots
{
    class History
    {
        #region ATTRIBUTES
        private IList<HandMetadata> history;
        private Object lockHistory;
        public int MaxHistory { get; set; }
        #endregion ATTRIBUTES

        //constructor
        public History()
        {
            history = new List<HandMetadata>();
            lockHistory = new Object();
            MaxHistory = 2;
        }

        //retrieve current history
        public IList<HandMetadata> GetList()
        {
            return new List<HandMetadata>(history);
        }

        //save history state
        public void Save(HandMetadata handMetadata)
        {
            lock (lockHistory)
            {
                if (history.Count == MaxHistory)
                {
                    history.RemoveAt(0);
                }
                history.Add(handMetadata);  //adds at end of list
            }
        }
    }
}
