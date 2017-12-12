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
        public double[] LeftHandPosition { get; set; }
        public double[] RightHandPosition { get; set; }
       #endregion ATTRIBUTES

        public HandMetadata()
        {
            LeftHandPosition = new double[2];  //coordinates: x, y 
            RightHandPosition = new double[2]; //coordinates: x, y
        }

        public HandMetadata(double[] left, double[] right)
        {
            LeftHandPosition = left;
            RightHandPosition = right;
        }

    }
}
