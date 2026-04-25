
using magisterDiplom.Fabric;
using magisterDiplom.Model.Configuration;
using System.Collections.Generic;
using System.Linq;
using static magisterDiplom.Schedule;


namespace magisterDiplom.HierarchicalGameModel
{
    internal class SecondLevel
    {

        private readonly Schedule schedule;

        private SecondLevel(Schedule schedule)
        {
            this.schedule = schedule;
        }

        public static SecondLevel CreateForSimplePreMaintence(PreMConfiguration configuration)
        {
            SimplePreMSchedule schedule = new SimplePreMSchedule(configuration);
            return new SecondLevel(schedule);
        }

        public static SecondLevel CreateForTypedPreMaintence(TypedPreMConfiguration configuration)
        {
            var schedule = new TypedPreMShedule(configuration);
            return new SecondLevel(schedule);
        }

        public int Makespan
        {
            get {
                return schedule.MakeSpan;
            }
            
        }

        public SecondLevelOutput Build(List<int> m, List<List<int>> A_matrix)
        {
            schedule.Update(m.Sum());
            List<int> dataTypes = schedule.DataTypesInPriority();

            for (int batch = 0; batch < m.Max(); batch++)
            {
                foreach (int dataType in dataTypes)
                {
                    if (batch >= m[dataType])
                        continue;

                    schedule.Add(dataType, A_matrix[dataType][batch]);
                }
            }

            schedule.Optimize();
            return schedule.Result();
        }

    }
}
