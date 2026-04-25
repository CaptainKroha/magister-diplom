using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using System;
using System.Collections.Generic;

namespace magisterDiplom.Fabric
{
    internal class TypedPreMShedule : PreMSchedule
    {

        private protected readonly new TypedPreMConfiguration config;
        private readonly List<MatrixYPreMTypes> Y_l;

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
            return Y_l[device].PreMaintenceStatusAfter(packet) == 1;
        }

        protected override int PreMaintenceStatusAfter(int device, int packet)
        {
            return Y_l[device].PreMaintenceStatusAfter(packet);
        }

    }
}
