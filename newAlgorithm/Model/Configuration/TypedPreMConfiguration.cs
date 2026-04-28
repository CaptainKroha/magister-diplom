using newAlgorithm.Model;
using System.Collections.Generic;

namespace magisterDiplom.Model.Configuration
{
    public class TypedPreMConfiguration : PreMConfiguration
    {
        public int PreMaintenceTypesCount {  get; set; }

        public List<int> InactionCosts { get; set; }

        public int[,] PreMaintenanceCosts { get; set; }

        public int[,] PreMaintenanceDurations { get; set; }

        public TypedPreMConfiguration() { }

        public TypedPreMConfiguration(PreMConfig preMConfig) : base(preMConfig) { }

    }
}
