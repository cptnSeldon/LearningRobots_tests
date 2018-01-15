using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHands
{

    enum State
    {
        UNDEFINED,
        OK,         //directed in the right rectangle
        WARNING,    //directed in the wrong rectangle
        SUCCESS,    //in the good rectangle
        ERROR       //in the wrong rectangle
    };


    class SequenceManager
    {
        #region ATTRIBUTES

        //sequence list: initilized by user input -> default = A-B-C-D-E depending on rectangles created
        List<int> sequence = new List<int>();
        IEnumerator<int> iterator;
        State currentStateRight;
        State currentStateLeft;

        #endregion ATTRIBUTES

        //constructor
        public SequenceManager()
        {
            iterator = sequence.GetEnumerator();
        }

        //set sequence
        public void SetSequence(List<int> initialSequence)
        {
            //if null or empty -> false
            if(!(initialSequence?.Any() ?? false))
            {
                sequence = new List<int>(initialSequence);
                iterator = sequence.GetEnumerator();
            }
        }

        //get current
        public int GetCurrent() { return iterator.Current; }

        //
        public State GetState(int currentRectangle, int directedRectangle, bool isLeftHand)
        {
            State localState;

            #region HANDLING CURRENT STATE
            //error in the sequence
            if (currentRectangle != iterator.Current && currentRectangle > -1)
                localState = State.ERROR;
            //not in any rectangle, directed to a wrong one
            else if (currentRectangle == -1 && directedRectangle != iterator.Current && directedRectangle != -1)
                localState = State.WARNING;
            //
            else if (directedRectangle == iterator.Current && directedRectangle == -1)
                localState = State.OK;

            else if (currentRectangle == iterator.Current)
                localState = State.SUCCESS;
            else
                localState = State.UNDEFINED;
            #endregion HANDLING CURRENT STATE

            //iterate over sequence list when quitting current rectangle
            if ((isLeftHand && currentStateLeft == State.SUCCESS || !isLeftHand && currentStateRight == State.SUCCESS) && localState != State.SUCCESS)
            {
                GoToNextValue();
                localState = State.UNDEFINED;
            }

            if (isLeftHand)
                currentStateLeft = localState;
            else
                currentStateRight = localState;

            return localState;
        }

        //Generates default sequence for now
        public void GenerateSequence(int numberOfRectangle, int numberOfSequence)
        {
            sequence.Clear();

            for (int i = 0; i < numberOfSequence ; i++)
            {
                sequence.Add(i % numberOfRectangle);
            }

            iterator = sequence.GetEnumerator();
        }

        //return sequence
        public List<int> GetSequence()
        {
            return sequence;
        }

        //print sequence: test purposes
        public void PrintSequence()
        {
            foreach (int s in sequence)
                Console.Write(s + " ");
            Console.WriteLine();
        }

        //next sequence
        private void GoToNextValue()
        {
            //if current value is the last one in the sequence, go back to the first one
            if (!(iterator?.MoveNext()??true)) //not (if null -> default: true)
            {
                iterator.Reset();
                iterator.MoveNext();
            }
        }

        
    }
}
