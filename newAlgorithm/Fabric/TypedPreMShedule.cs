using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using System;
using System.Collections.Generic;

namespace magisterDiplom.Fabric
{
    internal class TypedPreMShedule : PreMSchedule
    {

        public class TypedPreMaintenceSecondLevelOutput : PreMaintenceSecondLevelOutput
        {
            public List<List<List<int>>> Yl_Matrixes { get; private set; } = null;

            public TypedPreMaintenceSecondLevelOutput(TypedPreMShedule schedule) : base(schedule)
            {
                Yl_Matrixes = new List<List<List<int>>>(schedule.config.deviceCount);
                for(int device = 0; device < schedule.config.deviceCount; ++device)
                {
                    Yl_Matrixes.Add(new List<List<int>>());
                    Yl_Matrixes[device] = schedule.Y_l[device].ToListList();
                }
            }
        }

        private protected readonly new TypedPreMConfiguration config;

        private readonly List<MatrixYPreMTypes> Y_l;

        #region Программный интерфейс

        public TypedPreMShedule(TypedPreMConfiguration configuration) : base(configuration)
        {
            config = configuration;
            Y_l = new List<MatrixYPreMTypes>(config.deviceCount);
            for(int i = 0; i < config.deviceCount; i++)
            {
                Y_l.Add(new MatrixYPreMTypes(config.PreMaintenceTypesCount));
            }
        }
        
        public override void Update(int batchesCount)
        {
            base.Update(batchesCount);
            for(int i = 0; i < config.deviceCount; i++)
            {
                Y_l[i] = new MatrixYPreMTypes(config.PreMaintenceTypesCount);
            }
        }

        public override void Optimize()
        {
            throw new NotImplementedException();
        }

        public override SecondLevelOutput Result()
        {
            return new TypedPreMaintenceSecondLevelOutput(this);
        }

        #endregion

        #region Служебные переопределенные процедуры

        protected override int F2_criteria()
        {
            return TotalPreMaintenceCost() + TotalDeviceInactionCost();
        }

        protected override void AddColumnY()
        {
            foreach (var y in Y_l) 
            {
                y.AddColumn();   
            }
        }

        protected override void AddPreMaintenceAfterLastBatch()
        {
            foreach (var y in Y_l)
            {
                y.SetPreMaintenceLastPacketFirstType();
            }
        }

        protected override bool HasPreMaintenceAfter(int device, int packet)
        {
            return Y_l[device].PreMaintenceStatusAfter(packet) != -1;
        }

        protected override int PreMaintanceDurationAfter(int device, int packet)
        {
            int preMaintenceType = Y_l[device].PreMaintenceStatusAfter(packet);
            if(preMaintenceType == 0)
            {
                return 0;
            }

            return config.PreMaintenanceTimes[device, preMaintenceType];
        }

        #endregion

        #region Служебные методы

        protected int TotalPreMaintenceCost()
        {
            int totalCost = 0;
            for (int device = 0; device < config.deviceCount; device++)
            {
                foreach (PreMSet preMaintence in matrixTPM[device])
                {
                    int preMaintenceType = Y_l[device].PreMaintenceStatusAfter(preMaintence.BatchIndex);
                    totalCost += config.PreMaintenanceCosts[device, preMaintenceType];
                }
            }
            return totalCost;
        }

        protected int TotalDeviceInactionCost()
        {
            int result = 0;
            for (int device = 0; device < config.deviceCount; device++)
            {
                result += DeviceInactionDuration(device) * config.InactionCosts[device];   
            }
            return result;
        }

        #endregion

    }
}
