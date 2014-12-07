using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Timers;

namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your node data structure and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }


            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        private const int CITY_ICON_SIZE = 5;

        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region Public members
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed); 
            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Finds the distance between two points
        /// </summary>
        private double getDistance(PointF from, PointF to)
        {
            return Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
        }

        /// <summary>
        /// Finds the angle(theta) between two points
        /// </summary>
        private double getAngle(PointF origin, PointF to)
        {
            float xDiff = origin.X - to.X;
            float yDiff = origin.Y - to.Y;
            return ((double)Math.Atan2(yDiff, xDiff) * (double)(180 / Math.PI)) + 180;
        }

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        //public void GenerateProblem(int size) // unused
        //{
        //   this.GenerateProblem(size, Modes.Normal);
        //}

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        public void solveProblem()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int x;
            Route = new ArrayList(); 
            // this is the trivial solution. 
            for (x = 0; x < Cities.Length; x++)
            {
                Route.Add( Cities[Cities.Length - x -1]);
            }
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            timer.Stop();
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            //update the time
            Program.MainForm.tbElapsedTime.Text = "" + timer.Elapsed;
            // do a refresh. 
            Program.MainForm.Invalidate();

        }

        ///random
        public void solveRandomProblem()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Route = new ArrayList();

            List<int> spots = new List<int>();
            for (int i = 0; i < Cities.Length; i++)
            {
                spots.Add(i);
            }
            Random rand = new Random();
            int r;
            while (spots.Count > 0)
            {
                r = rand.Next(0, spots.Count);
                Route.Add(Cities[spots[r]]);
                spots.RemoveAt(r);
            }
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            timer.Stop();
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            //update the time
            Program.MainForm.tbElapsedTime.Text = "" + timer.Elapsed;
            // do a refresh. 
            Program.MainForm.Invalidate();

        }

        // greedy solver
        public void greedySolve()
        {
            int initial = 0;
            bool pathNotFound = true;
            HashSet<int> visited = new HashSet<int>();

            // loop through initial starting nodes for greedy path
            // until one of them finishes visiting all cities 
            // without hitting a dead end
            while (pathNotFound)
            {
                pathNotFound = false;
                Route = new ArrayList();
                City cursor = Cities[initial];
                Route.Add(Cities[initial]);
                visited.Add(initial);

                // starting with city at index `initial`
                // go to the nearest city and repeat
                // untill all cities have been visited
                while (visited.Count != Cities.Length)
                {
                    double dist = Double.PositiveInfinity;
                    int bestNeighbor = -1;

                    for (int i = 0; i < Cities.Length; i++)
                    {
                        if (!visited.Contains(i))
                        {
                            if (cursor.costToGetTo(Cities[i]) < dist)
                            {
                                dist = cursor.costToGetTo(Cities[i]);
                                bestNeighbor = i;
                            }
                        }
                    }

                    if (bestNeighbor == -1)
                    {
                        // no path available
                        // break and try a different start node
                        // (class spec says no backtracking)
                        pathNotFound = true;
                        break;
                    }
                    else
                    {
                        // found closest neighbor, go there
                        visited.Add(bestNeighbor);
                        Route.Add(Cities[bestNeighbor]);
                        cursor = Cities[bestNeighbor];
                    }

                }

                // if the last path failed to find all cities...
                // ...start wiht a different initial city
                initial++;
            }

            // assign as best solution so far
            bssf = new TSPSolution(Route);
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            // do a refresh. 
            Program.MainForm.Invalidate();
        }
        #endregion

        #region Branch & Bound
        /// I didn't give B&B it's own region to be narcisisstic, it just 
        /// kind of got big on me


        /// <summary>
        /// Row reduce the given matrix in the partial state
        /// </summary>
        private PartialState reduce(PartialState stateIn)
        {
            double lowerBound = stateIn.LowerBound;
            double[,] matrix = stateIn.Matrix;
            int size = matrix.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                /// If we have already left this edge, then we don't need to reduce
                /// the row because it has already been set to infinity
                if (stateIn.Edges.Exists(x => x.Item1 == i))
                    continue;

                /// Get the lowest value in the row
                double lowestRowValue = Double.PositiveInfinity;
                for (int j = 0; j < size; j++)
                {
                    if (matrix[i, j] < lowestRowValue)
                        lowestRowValue = matrix[i, j];
                }

                /// If that value is not zero and is not infinite, then we need to reduce the row
                if (lowestRowValue != 0 && !Double.IsInfinity(lowestRowValue))
                {
                    for (int j = 0; j < size; j++)
                        matrix[i, j] = matrix[i, j] - lowestRowValue;
                }

                /// Add the lowest row value to the lower bound before going to
                /// the next row
                lowerBound += lowestRowValue;
            }

            /// Now we just need to reduce the columns and we will have a
            /// reduced cost martrix
            for (int j = 0; j < size; j++)
            {
                /// If we have already entered this edge, then
                if (stateIn.Edges.Exists(x => x.Item2 == j))
                    continue;

                /// Find the lowest value
                double lowestColValue = Double.PositiveInfinity;
                for (int i = 0; i < size; i++)
                {
                    if (matrix[i, j] < lowestColValue)
                        lowestColValue = matrix[i, j];
                }

                /// If that value is not zero and is not infinite, then we need to reduce the column
                if (lowestColValue != 0 && !Double.IsInfinity(lowestColValue))
                {
                    for (int i = 0; i < size; i++)
                        matrix[i, j] = matrix[i, j] - lowestColValue;
                }

                /// Add that value to the lower bound of this state
                lowerBound += lowestColValue;
            }
            return new PartialState(matrix, lowerBound, stateIn.Edges);
        }

        private PartialState includeEdgeState(PartialState state, Tuple<int, int> edgeToInclude)
        {
            /// We need to make a copy of the partial state.
            PartialState includeState = new PartialState(
                (double[,])state.Matrix.Clone(),
                state.LowerBound,
                new List<Tuple<int, int>>(state.Edges));

            /// Set the inverse position to infinity to not allow backtracking there.
            includeState.Matrix[edgeToInclude.Item2, edgeToInclude.Item1] = Double.PositiveInfinity;

            /// Set the values for the entire column of the destination to infinity so there are no cycles
            int size = includeState.Matrix.GetLength(0);
            for (int i = 0; i < size; i++)
                includeState.Matrix[i, edgeToInclude.Item2] = Double.PositiveInfinity;

            /// Set the values for the entire row of the origin to infinity so there are no cycles
            for (int j = 0; j < size; j++)
                includeState.Matrix[edgeToInclude.Item1, j] = Double.PositiveInfinity;

            /// Add the included edge to the Parital State Route
            includeState.Edges.Add(edgeToInclude);

            /// Delete edges that would give us a premature cycle
            if (includeState.Edges.Count < size - 1)
            {
                int startCity = edgeToInclude.Item1;
                while (includeState.Edges.Find(x => x.Item2 == startCity) != null)
                    startCity = includeState.Edges.Find(x => x.Item2 == startCity).Item1;

                int endCity = edgeToInclude.Item2;
                while (includeState.Edges.Find(x => x.Item1 == endCity) != null)
                    endCity = includeState.Edges.Find(x => x.Item1 == endCity).Item2;

                while (startCity != endCity)
                {
                    includeState.Matrix[endCity, startCity] = Double.PositiveInfinity;
                    includeState.Matrix[edgeToInclude.Item2, startCity] = Double.PositiveInfinity;
                    startCity = includeState.Edges.Find(x => x.Item1 == startCity).Item2;
                }
            }

            return reduce(includeState);
        }

        private PartialState excludeEdgeState(PartialState state, Tuple<int, int> edgeToExclude)
        {
            /// We need to make a copy of the partial state.
            PartialState excludeState = new PartialState(
                (double[,])state.Matrix.Clone(),
                state.LowerBound,
                new List<Tuple<int, int>>(state.Edges));

            /// Exclude the given edge, then reduce and return the matrix
            excludeState.Matrix[edgeToExclude.Item1, edgeToExclude.Item2] = Double.PositiveInfinity;
            return reduce(excludeState);
        }

        private double excludeCost(PartialState state, Tuple<int, int> edgeToExclude)
        {
            int size = state.Matrix.GetLength(0);

            /// Get the smallest row value, aside from the edgeToExclude
            double rowVal = Double.PositiveInfinity;
            for (int i = 0; i < size; i++)
            {
                if (i != edgeToExclude.Item1 && state.Matrix[i, edgeToExclude.Item2] < rowVal)
                    rowVal = state.Matrix[i, edgeToExclude.Item2];
            }

            /// Get the smallest col value, aside from the edgeToExclude
            double colVal = Double.PositiveInfinity;
            for (int j = 0; j < size; j++)
            {
                if (j != edgeToExclude.Item2 && state.Matrix[edgeToExclude.Item1, j] < colVal)
                    colVal = state.Matrix[edgeToExclude.Item1, j];
            }

            return colVal + rowVal;
        }

        private double IncludeExcludeDiff(PartialState state, Tuple<int, int> position)
        {
            PartialState includeState = includeEdgeState(state, position);
            return excludeCost(state, position) - includeState.LowerBound;
        }

        /// <summary>
        ///  Small helper function to help with finding some intial routes for bssf
        /// </summary>
        /// <param name="list">list to shuffle</param>
        /// <returns>a shuffled list</returns>
        private List<City> shuffle(List<City> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                City value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }


        /// <summary>
        /// solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        public void solveBBProblem()
        {
            /// Get our timer and stopwatch ready
            bool intervalExceeded = false;
            Timer timer = new Timer(10 * 60 * 1000); //Minutes * 60 Seconds * 1000 Milliseconds
            ElapsedEventHandler handler = (sender, e) => { intervalExceeded = true; };
            timer.Elapsed += handler;
            Stopwatch stopwatch = new Stopwatch();

            timer.Start();
            stopwatch.Start();

            /// Generate five random, greedy solutions and select the best one as our initial value
            TSPSolution bestSoFar = null;
            List<City> randomCities = new List<City>(GetCities());
            for (int i = 0; i < 5; i++)
            {
                bool validRoute = true;
                HashSet<int> cities = new HashSet<int>();
                Route = new ArrayList();
                randomCities = shuffle(randomCities);
                Route.Add(randomCities[0]);
                for (int x = 1; x < randomCities.Count; x++)
                    cities.Add(x);
                int nextCity = -1;
                double dist = Double.PositiveInfinity;
                while (Route.Count < randomCities.Count && validRoute)
                {
                    foreach (int x in cities)
                    {
                        City temp = (City)Route[0];
                        if (temp.costToGetTo(randomCities[x]) < dist)
                        {
                            nextCity = x;
                            dist = temp.costToGetTo(randomCities[x]);
                        }
                    }

                    if (nextCity != -1)
                    {
                        Route.Insert(0, randomCities[nextCity]);
                        cities.Remove(nextCity);
                        nextCity = -1;
                        dist = double.PositiveInfinity;
                    }
                    else
                        validRoute = false;
                }

                if (Double.IsInfinity(((City)Route[0]).costToGetTo((City)Route[randomCities.Count - 1])))
                    validRoute = false;

                if (!validRoute)
                {
                    i--;
                    continue;
                }


                if (bestSoFar == null)
                {
                    Route.Reverse();
                    bestSoFar = new TSPSolution(Route);
                }
                else
                {
                    TSPSolution tempSolution = new TSPSolution(Route);
                    if (tempSolution.costOfRoute() < bestSoFar.costOfRoute())
                        bestSoFar = tempSolution;
                }
            }

            // call this the best solution so far.
            bssf = bestSoFar;

            City[] myCities = GetCities();

            /// Create and then fill the cost matrix
            double[,] matrix = new double[myCities.Length, myCities.Length];
            for (int i = 0; i < myCities.Length; i++)
            {
                /// Get initial values for the row
                for (int j = 0; j < myCities.Length; j++)
                {
                    if (i == j)
                        matrix[i, j] = Double.PositiveInfinity;
                    else
                        matrix[i, j] = myCities[i].costToGetTo(myCities[j]);
                }
            }

            /// Reduce the cost matrix to get the initial lower bound
            /// and the initial state for this problem
            PartialState initialState
                = reduce(new PartialState(matrix, 0, new List<Tuple<int, int>>()));

            /// Use these variables to keep track of various statistics for the 
            /// performance of the method
            int maxNumStatesStored = 0;
            int numBssfUpdates = 0;
            int numStatesCreated = 0;
            int numStatesPruned = 0;

            /// Insert the initial state into the priority queue, and
            /// we're off to the races
            PriorityQueue pq = new PriorityQueue();
            pq.insert(initialState);

            /// While we still have states in the queue...
            while (!pq.isEmpty() && !intervalExceeded)
            {
                /// Check to see how many states are being stored by the queue, update as needed
                if (pq.size() > maxNumStatesStored) maxNumStatesStored = pq.size();

                /// Get the Partial State with the lowest lower bound
                PartialState myState = pq.deleteMin();

                /// If this partial state has a lower bound that is 
                /// worst than bssf, then we can prune it
                if (bssf.costOfRoute() <= myState.LowerBound)
                {
                    numStatesPruned++;
                    continue;
                }

                /// If the route in this state has included all cities
                if (myState.Edges.Count == myCities.GetLength(0) - 1)
                {
                    Route = new ArrayList();
                    Tuple<int, int> firstEdge = myState.Edges[0];
                    Route.Add(myCities[firstEdge.Item1]);
                    Route.Add(myCities[firstEdge.Item2]);

                    int lastCity = firstEdge.Item2;
                    int firstCity = firstEdge.Item1;
                    for (int i = 2; i < myCities.GetLength(0); i++)
                    {
                        Tuple<int, int> currentEdge
                            = myState.Edges.Find(x => x.Item1 == lastCity);
                        if (currentEdge == null)
                        {
                            Tuple<int, int> tempEdge
                                = myState.Edges.Find(x => x.Item2 == firstCity);
                            if (currentEdge == null)
                                Route.Insert(0, myCities[tempEdge.Item1]);
                            firstCity = tempEdge.Item1;
                        }
                        else
                        {
                            Route.Add(myCities[currentEdge.Item2]);
                            lastCity = currentEdge.Item2;
                        }
                    }

                    TSPSolution tempSolution = new TSPSolution(Route);
                    if (bssf.costOfRoute() > tempSolution.costOfRoute())
                    {
                        bssf = tempSolution;
                        numBssfUpdates++;
                    }
                }
                else
                {
                    /// Find the spots in the partial state that have distances of zero
                    int size = myState.Matrix.GetLength(0);
                    List<Tuple<int, int>> zeroSpots = new List<Tuple<int, int>>();
                    for (int i = 0; i < size; i++)
                    {
                        /// Only check those cities that have not yet been added to our route
                        if (!myState.Edges.Exists(x => x.Item1 == i))
                        {
                            for (int j = 0; j < size; j++)
                            {
                                /// For every city not checking itself, and has a distance of 
                                /// zero after the lower bound, add it
                                if (i != j && myState.Matrix[i, j] == 0)
                                    zeroSpots.Add(new Tuple<int, int>(i, j));
                            }
                        }
                    }

                    /// Pick the zero spot that has the greatest difference between
                    /// including and excluding that particular edge.
                    Tuple<Tuple<int, int>, double> lowestSpot
                        = new Tuple<Tuple<int, int>, double>(zeroSpots[0], IncludeExcludeDiff(myState, zeroSpots[0]));
                    for (int i = 1; i < zeroSpots.Count; i++)
                    {
                        if (IncludeExcludeDiff(myState, zeroSpots[i]) > lowestSpot.Item2)
                            lowestSpot = new Tuple<Tuple<int, int>, double>(zeroSpots[i],
                                                                            IncludeExcludeDiff(myState, zeroSpots[i]));
                    }

                    PartialState includeState = includeEdgeState(myState, lowestSpot.Item1);
                    PartialState excludeState = excludeEdgeState(myState, lowestSpot.Item1);

                    if (includeState.LowerBound < bssf.costOfRoute())
                    {
                        pq.insert(includeState);
                        numStatesCreated++;
                    }
                    else
                        numStatesPruned++;

                    if (excludeState.LowerBound < bssf.costOfRoute())
                    {
                        pq.insert(excludeState);
                        numStatesCreated++;
                    }
                    else
                        numStatesPruned++;
                }
            }

            stopwatch.Stop();

            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            Program.MainForm.tbElapsedTime.Text = stopwatch.Elapsed.TotalSeconds.ToString();
            //Program.MainForm.tbStatesSaved.Text = maxNumStatesStored.ToString();
            //Program.MainForm.tbBSSFUpdates.Text = numBssfUpdates.ToString();
            //Program.MainForm.tbStatesCreated.Text = numStatesCreated.ToString();
            //Program.MainForm.tbStatesPruned.Text = numStatesPruned.ToString();

            // do a refresh. 
            Program.MainForm.Invalidate();

            Console.Out.WriteLine("Max # of States Stored: " + maxNumStatesStored.ToString());
            Console.Out.WriteLine("# of BSSF Updates: " + numBssfUpdates.ToString());
            Console.Out.WriteLine("Total # of States Created: " + numStatesCreated.ToString());
            Console.Out.WriteLine("Total # of States Pruned: " + numStatesPruned.ToString());
        }

        #endregion
    }

}
