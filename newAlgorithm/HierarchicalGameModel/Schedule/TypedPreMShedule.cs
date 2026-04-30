using magisterDiplom.Model;
using magisterDiplom.Model.Configuration;
using magisterDiplom.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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

        private readonly MatrixYPreMTypes[] Y_l;

        #region Программный интерфейс

        public TypedPreMShedule(TypedPreMConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
            config = configuration;
            Y_l = new MatrixYPreMTypes[config.deviceCount];
            for(int device = 0; device < config.deviceCount; device++)
            {
                Y_l[device] = new MatrixYPreMTypes(config.PreMaintenceTypesCount);
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
            OptimizeByDeletePreMaintences();
            if (success)
            {
                OptimizeByChangePreMaintencesType();
            }
               
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
            if(preMaintenceType == -1)
            {
                return 0;
            }

            return config.PreMaintenanceDurations[device, preMaintenceType];
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

        protected void OptimizeByDeletePreMaintences()
        {
            success = true;
            Calculate();
            if (SolutionUnacceptable())
            {
                success = false;
                return;
            }

            for (int batch = 0; batch < ScheduleSize() - 1; batch++)
            {
                for (int device = 0; device < config.deviceCount; device++)
                {
                    Y_l[device].UnsetPreMaintence(0, batch);
                    Calculate();
                    if (SolutionUnacceptable())
                    {
                        Y_l[device].SetPreMaintence(0, batch);
                    }
                }
            }
        }

        protected void OptimizeByChangePreMaintencesType()
        {

            _logger.Debug("-=-=-Оптимизация типов ПТО-=-=-");

            int[] w_l = new int[config.deviceCount];
            int[] j_l = new int[config.deviceCount];

            List<List<int>> PM_l = new List<List<int>>(config.deviceCount);
            for(int device = 0; device < config.deviceCount; ++device)
            {
                PM_l.Add(new List<int>(matrixTPM[device].Count));
                foreach(var preMSet in matrixTPM[device])
                {
                    PM_l[device].Add(preMSet.BatchIndex);   
                }
            }

            _logger.Print("PM_l:", PM_l);

            int s = 1;

            while (true)
            {
                _logger.Print($"Iteration {s}");

                int zero_prem_left = 0;
                for (int device = 0; device < config.deviceCount; ++device)
                {
                    if (PM_l[device].Count > 0)
                    {
                        j_l[device] = PM_l[device].First();
                    }
                    else
                    {
                        j_l[device] = -1;
                        zero_prem_left++;
                    }
                }

                _logger.Print("j_l:", j_l);

                if(zero_prem_left == config.deviceCount)
                {
                    break;
                }

                for (int i = 0; i < Y_l.Length; i++ )
                {
                    _logger.Print($"Y_{i+1}", Y_l[i].ToListList());
                }

                Calculate();
                int best_f2 = F2_criteria();

                int device_max_grad = -1;
                int G = 0;

                int k_l = 0;

                while (DevicesToProcessLeft(PM_l, j_l))
                {
                    if (G > 0)
                    {
                        Y_l[device_max_grad].UnsetPreMaintence(w_l[device_max_grad], j_l[device_max_grad]);
                        Y_l[device_max_grad].SetPreMaintence(w_l[device_max_grad] + k_l, j_l[device_max_grad]);
                        w_l[device_max_grad] += k_l;
                        _logger.Print("Upgrade found");
                        _logger.Print("w_l:", w_l);
                        break;
                    }
                    else
                    {
                        k_l++;
                        _logger.Print($"k_l: {k_l}");

                        for (int device = 0; device < config.deviceCount; device++)
                        {
                            if (j_l[device] == -1 || !PM_l[device].Contains(j_l[device]))
                            {
                                continue;
                            }

                            w_l[device] = Y_l[device].PreMaintenceStatusAfter(j_l[device]);
                            if (w_l[device] + k_l < config.PreMaintenceTypesCount)
                            {
                                Y_l[device].UnsetPreMaintence(w_l[device], j_l[device]);
                                Y_l[device].SetPreMaintence(w_l[device] + k_l, j_l[device]);
                                _logger.Print($"Device: {device}; [{w_l[device]}, {j_l[device]}] -> [{w_l[device] + k_l}, {j_l[device]}]");

                                Calculate();
                                if (!SolutionUnacceptable())
                                {
                                    int current_f2 = F2_criteria();
                                    int grad = current_f2 - best_f2;
                                    _logger.Print($"grad: {grad}");

                                    if (grad < 0 && -grad > G)
                                    {
                                        device_max_grad = device;
                                        G = -grad;
                                    }
                                }
                                else
                                {
                                    _logger.Print("Solution unacceptable");
                                }
                                Y_l[device].UnsetPreMaintence(w_l[device] + k_l, j_l[device]);
                                Y_l[device].SetPreMaintence(w_l[device], j_l[device]);
                            }
                            else
                            {
                                _logger.Print($"Device {device} left");
                                PM_l[device].Remove(j_l[device]);
                            }
                        }
                    }
                }

                s++;
            }

        }

        private bool DevicesToProcessLeft(List<List<int>> PM_l, int[] j_l)
        {
            for(int device = 0;  device < config.deviceCount; device++)
            {
                if (PM_l[device].Contains(j_l[device])) return true;
            }
            return false;
        }

        #endregion

    }
}
