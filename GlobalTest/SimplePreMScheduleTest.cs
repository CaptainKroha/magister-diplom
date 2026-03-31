using magisterDiplom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using magisterDiplom.Fabric;
using magisterDiplom.Model;

namespace GlobalTest
{

    [TestClass]
    public class SimplePreMScheduleTest
    {

        [TestMethod]
        public void MyTest()
        {

            // Создаём структуру конфигурации
            Config config = Config.GetDebugConfig_2();
            PreMConfig preMConfig = new(
                config,
                new() { 7, 7, 7 },
                new() { 0.1, 0.1, 0.1 },
                new() { 0.9, 0.9, 0.9 },
                0.50
            );

            // Создаём объект класса расписания
            SimplePreMSchedule schedule = new (config, preMConfig);

            // Выполняем вызов функции Build
            schedule.Build(new List<List<int>> {
                new () { 4, 4, 2 },
                new () { 4, 4, 2 },
                new () { 4, 4, 2 },
            });
        }

    }
}
