using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
	class Node
	{
		private PartialState partialState;

		public Node(PartialState partialStateIn)
		{ this.partialState = partialStateIn; }

		public PartialState getPartialState()
		{ return partialState; }

		public void setPartialState(PartialState value)
		{ this.partialState = value; }

		public bool isCloserThan(Node node)
		{
			// If this node has a null partial state, then it is always farther or of equal distance
			if (this.partialState == null)
				return false;
			// If this node is not infinite, but the other is, then we are closer
			else if (node.partialState == null)
				return true;
			// Otherwise, check to see who's closer
			else
				return this.partialState.LowerBound < node.partialState.LowerBound;
		}
	}
}
