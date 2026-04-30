using newAlgorithm.Model;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

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

        public override string ToString()
        {
            string res = base.ToString();
            res += $"PreMaintenceTypesCount: {PreMaintenceTypesCount}" + Environment.NewLine;
            res += "InactionCosts: [";
            foreach(int i in InactionCosts)
            {
                res += $"{i} ";
            }
            res += "]" + Environment.NewLine;

            res += "PreMaintenceCosts:" + Environment.NewLine + "[";
            for(int i = 0; i < PreMaintenanceCosts.GetLength(0); i++)
            {
                for(int j = 0; j < PreMaintenanceCosts.GetLength(1); j++)
                {
                    res += $"{PreMaintenanceCosts[i, j]} ";
                }
                if (i < PreMaintenanceCosts.GetLength(0) - 1) res += Environment.NewLine;
            }
            res += "]" + Environment.NewLine;

            res += "PreMaintenanceDurations:" + Environment.NewLine + "[";
            for (int i = 0; i < PreMaintenanceDurations.GetLength(0); i++)
            {
                for (int j = 0; j < PreMaintenanceDurations.GetLength(1); j++)
                {
                    res += $"{PreMaintenanceDurations[i, j]} ";
                }
                if (i < PreMaintenanceDurations.GetLength(0) - 1) res += Environment.NewLine;
            }
            res += "]" + Environment.NewLine;

            return res;
        }

    }
}
