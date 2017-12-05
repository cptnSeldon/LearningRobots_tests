using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHands
{
    public class HandMetadata
    {

        #region ATTRIBUTES
        public int[] LeftHandPosition { get; set; }
        public int[] RightHandPosition { get; set; }
       #endregion ATTRIBUTES

        public HandMetadata()
        {
            LeftHandPosition = new int[2];  //coordinates: x, y 
            RightHandPosition = new int[2]; //coordinates: x, y
        }

        public HandMetadata(int[] left, int[] right)
        {
            LeftHandPosition = left;
            RightHandPosition = right;
        }

        

    }
}
