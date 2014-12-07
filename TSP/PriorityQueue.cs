using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
	class PriorityQueue
	{
		private Node[] heapArray;
		private int heapCounter;

		// Constructor
		public PriorityQueue()
		{
			heapArray = new Node[32];
			heapCounter = 0;
		}

        #region Public Methods

        // Returns whether the Priority Queue is empty or not
		public bool isEmpty() { return heapCounter == 0; }

        // Returns the number of elements in the priority queue
        public int size() { return heapCounter; }

		// Insert a new node with the given partial state
        public void insert(PartialState partialStateIn)
        {
            /// Resize the array to be bigger as needed.
            if (heapArray.Length == heapCounter)
                Array.Resize<Node>(ref heapArray, heapArray.Length * 2);

            heapArray[heapCounter] = new Node(partialStateIn);
            bubbleUp(heapCounter);
            heapCounter++;
        }

		// Return the next node in the queue, and delete it from the queue, allowing the node after to sort up
		public PartialState deleteMin()
		{
			if(heapCounter == 0)
				return null;

            heapCounter--;
            Node result = heapArray[0];
			heapArray[0] = heapArray[heapCounter];
			heapArray[heapCounter] = null;

			if(heapCounter > 0)
				siftDown(0);

			return result.getPartialState();
		}
        #endregion

        #region Private Methods

        // Sift a node with a heavier weight down
		private void siftDown(int nodeIndex)
		{
			// Check to see if both childs are available, and pick the smallest one
			if (getLeftChildIndex(nodeIndex) < heapCounter &&
				getRightChildIndex(nodeIndex) < heapCounter)
			{
				if (heapArray[getLeftChildIndex(nodeIndex)].isCloserThan(heapArray[getRightChildIndex(nodeIndex)]))
				{
					if (heapArray[getLeftChildIndex(nodeIndex)].isCloserThan(heapArray[nodeIndex]))
					{
						swap(nodeIndex, getLeftChildIndex(nodeIndex));
						siftDown(getLeftChildIndex(nodeIndex));
					}
				}
				else
				{
					if (heapArray[getRightChildIndex(nodeIndex)].isCloserThan(heapArray[nodeIndex]))
					{
						swap(nodeIndex, getRightChildIndex(nodeIndex));
						siftDown(getRightChildIndex(nodeIndex));
					}
				}
			}
			// If only the left one is open, check it's value
			else if (getLeftChildIndex(nodeIndex) < heapCounter &&
					 heapArray[getLeftChildIndex(nodeIndex)].isCloserThan(heapArray[nodeIndex]))
			{
				swap(nodeIndex, getLeftChildIndex(nodeIndex));
				siftDown(getLeftChildIndex(nodeIndex));
			}
			// If only the right one is open, check it's value
			else if (getRightChildIndex(nodeIndex) < heapCounter &&
					 heapArray[getRightChildIndex(nodeIndex)].isCloserThan(heapArray[nodeIndex]))
			{
				swap(nodeIndex, getRightChildIndex(nodeIndex));
				siftDown(getRightChildIndex(nodeIndex));
			}
		}

		// Sift a node with a lighter weight up as needed
		private void bubbleUp(int nodeIndex)
		{
			if (nodeIndex == getParentIndex(nodeIndex)) // This node is the root
				return;

			Node thisNode = heapArray[nodeIndex];
			Node parentNode = heapArray[getParentIndex(nodeIndex)];

			// If this node is lighter than it's parent, swap them
			if (thisNode.isCloserThan(parentNode))
			{
				swap(nodeIndex, getParentIndex(nodeIndex));
				bubbleUp(getParentIndex(nodeIndex));
			}
			else
				return;
		}

		// A helper function for swapping two nodes and updating the pointer array as necessary
		private void swap(int node1Index, int node2Index)
		{			
			Node tempNode = heapArray[node1Index];
			heapArray[node1Index] = heapArray[node2Index];
			heapArray[node2Index] = tempNode;
		}

		// Get the node index, the postion of the node in the heap array, of the parent of the specified node
		private int getParentIndex(int nodeIndex)
		{
			return (nodeIndex - 1) / 2;
		}

		// Get the node index for the left child of the specified node
		private int getLeftChildIndex(int nodeIndex)
		{
			return 2 * (nodeIndex + 1) - 1;
		}

		// Get the node index for the right child of the specified node
		private int getRightChildIndex(int nodeIndex)
		{
			return 2 * (nodeIndex + 1);
        }
        #endregion
    }
}
