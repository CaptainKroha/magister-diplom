
using magisterDiplom.Fabric;
using magisterDiplom.Model.Configuration;
using magisterDiplom.Utils;
using System.Collections.Generic;
using System.Linq;
using static magisterDiplom.Schedule;


namespace magisterDiplom.HierarchicalGameModel
{
    internal class SecondLevel
    {

        private readonly Schedule _schedule;
        private readonly ILogger _logger;

        private SecondLevel(Schedule schedule, ILogger logger)
        {
            _schedule = schedule;
            _logger = logger;
        }

        public static SecondLevel CreateForSimplePreMaintence(PreMConfiguration configuration, ILogger logger)
        {
            var schedule = new SimplePreMSchedule(configuration, logger);
            return new SecondLevel(schedule, logger);
        }

        public static SecondLevel CreateForTypedPreMaintence(TypedPreMConfiguration configuration, ILogger logger)
        {
            var schedule = new TypedPreMShedule(configuration, logger);
            return new SecondLevel(schedule, logger);
        }

        public int Makespan
        {
            get {
                return _schedule.MakeSpan;
            }
            
        }

        public SecondLevelOutput Build(List<int> m, List<List<int>> A_matrix)
        {
            _logger.Print("////////////////////////////////////////////////////");
            _logger.Print("A:", A_matrix);

            _schedule.Update(m.Sum());
            List<int> dataTypes = _schedule.DataTypesInPriority();

            for (int batch = 0; batch < m.Max(); batch++)
            {
                foreach (int dataType in dataTypes)
                {
                    if (batch >= m[dataType])
                        continue;

                    _schedule.Add(dataType, A_matrix[dataType][batch]);
                }
            }

            _schedule.Optimize();
            return _schedule.Result();
        }

    }
}
