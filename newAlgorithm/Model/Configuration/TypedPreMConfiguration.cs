using newAlgorithm.Model;
using System.Collections.Generic;

namespace magisterDiplom.Model.Configuration
{
    public class TypedPreMConfiguration : PreMConfiguration
    {
        public int PreMaintenceTypesCount {  get; set; }

        public List<int> InactionCosts { get; set; }

        public Matrix PreMaintenanceCosts { get; set; }

        public Matrix PreMaintenanceDurations { get; set; }

        public TypedPreMConfiguration(PreMConfig preMConfig) : base(preMConfig) { }

    }
}
