namespace P3_LearningRobots
{
    /// <summary>
    /// HandMetadata
    /// </summary>
    public class HandMetadata
    {
        #region ATTRIBUTES
        public double[] LeftHandPosition { get; set; }
        public double[] RightHandPosition { get; set; }
        #endregion ATTRIBUTES

        /// <summary>
        /// 
        /// </summary>
        public HandMetadata()
        {
            LeftHandPosition = new double[2];  //coordinates: x, y 
            RightHandPosition = new double[2]; //coordinates: x, y
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public HandMetadata(double[] left, double[] right)
        {
            LeftHandPosition = left;
            RightHandPosition = right;
        }
    }
}
