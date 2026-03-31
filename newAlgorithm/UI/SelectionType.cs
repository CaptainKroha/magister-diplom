using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom
{
    /// <summary>
    /// Перечисления отвечающее за способ обработки данных
    /// </summary>
    public enum SelectoinType : int
    {
        // Турнирная селекция
        TournamentSelection = 0,

        // Метод рулетки
        RouletteMethod = 1,

        // Равномерное ранжирование
        UniformRanking = 2,

        // Сигма отсечение
        SigmaClipping = 3,

        // Неопредлено
        Undefined = -1,
    }
}
