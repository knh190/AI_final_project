using NUnit.Framework;
using MCTS;

namespace Tests
{
    public class MonteCarloTests
    {
        [Test]
        public void TestDataOne()
        {
            int[] inf = InfluenceMapFixture1();
            State initState = new State(inf, 5, 5);

            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            search.MaxSearchStep = 10;
            search.MaxExpandLevel = 1;
            search.Initialize(initState, NaiveStrategy.SimulateRandomPlayout);

            // choose the best place on influence map
            State state = search.NextMove();
            Assert.AreEqual(0, state.MoveToMapIndex);
        }

        [Test]
        public void TestDataTwo()
        {
            int[] inf = InfluenceMapFixture2();
            State initState = new State(inf, 0, 0);

            MonteCarloTreeSearch search = new MonteCarloTreeSearch();
            search.MaxSearchStep = 10;
            search.MaxExpandLevel = 1;
            search.Initialize(initState, NaiveStrategy.SimulateRandomPlayout);

            // choose the best place on influence map
            State state = search.NextMove();
            Assert.AreEqual(3, state.MoveToMapIndex);
        }

        private static int[] InfluenceMapFixture1()
        {
            int[] inf = { 5, 2, -1, 0, 0 };
            return inf;
        }

        private static int[] InfluenceMapFixture2()
        {
            int[] inf = { 0, 2, 0, 5, -1 };
            return inf;
        }
    }
}
