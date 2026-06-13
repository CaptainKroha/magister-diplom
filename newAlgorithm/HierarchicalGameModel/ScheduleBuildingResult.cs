using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static magisterDiplom.Schedule;

namespace magisterDiplom.HierarchicalGameModel
{
    public class ScheduleBuildingResult
    {

        public int Makespan;
        public List<List<int>> BatchesConfiguration;
        public SecondLevelOutput Schedule;

        public ScheduleBuildingResult(int makespan, List<List<int>> batchesConfiguration, SecondLevelOutput schedule)
        {
            Makespan = makespan;
            BatchesConfiguration = batchesConfiguration;
            Schedule = schedule;
        }
    }
}
