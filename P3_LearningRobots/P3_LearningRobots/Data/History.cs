using System;
using System.Collections.Generic;
using System.Linq;

namespace P3_LearningRobots
{
    public class History
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
            return new List<HandMetadata>(history.Skip(Math.Max(0, history.Count - MaxHistory)));
        }

        public IList<HandMetadata> GetAll()
        {
            return new List<HandMetadata>(history);
        }

        //save history state
        public void Save(HandMetadata handMetadata)
        {
            lock (lockHistory)
            {
                history.Add(handMetadata);  //adds at end of list
            }
        }
    }
}
