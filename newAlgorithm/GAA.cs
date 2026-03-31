using magisterDiplom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newAlgorithm
{
    public class GAA
    {
        /// <summary>
        /// Данная переменная содержит информацию о количестве типов данных
        /// </summary>
        protected readonly int dataTypesCount;

        /// <summary>
        /// Данная переменная определяем фиксированные ли партии
        /// </summary>
        private readonly bool isFixedBatches;

        /// <summary>
        /// Данная переменная представляет из себя список хромосом
        /// </summary>
        public List<Xromossomi> nabor = new List<Xromossomi>();

        /// <summary>
        /// Данная переменная определяет количество данных каждого типа
        /// </summary>
        int batchCount = 10;

        private readonly List<int> batchCountList;        // Начальное количество требований для каждого типа данных
        Random rand = new Random();
        public List<int> _fitnesslist = new List<int>();


        /// <summary>
        /// Данный конструктор создаёт экземпляр класса GAA
        /// </summary>
        /// <param name="dataTypesCount">Количество типов данных</param>
        /// <param name="batchCountList">Вектор длиной dataTypesCount, каждый элемент которого будет равен batchCount</param>
        /// <param name="isFixedBatches">Являются ли партии фиксированными</param>
        /// <param name="batchCount">Количество данных каждого типа</param>
        public GAA(int dataTypesCount, List<int> batchCountList, bool isFixedBatches, int batchCount)
        {
            this.dataTypesCount = dataTypesCount;
            this.batchCountList = batchCountList;
            this.isFixedBatches = isFixedBatches;
            this.batchCount = batchCount;
        }

        // Данный класс описывает хромосому
        public class Xromossomi
        {
            static int size = 10;
            public List<List<int>> GenList = new List<List<int>>();
            public List<int> GenListOst = new List<int>();
            public List<int> GenA = new List<int>();
            public List<int> GenC = new List<int>();
            public List<int> GenB = new List<int>();
            public int ostA = size;
            public int ostB = size;
            public int ostC = size;
            public Xromossomi(int i)
            {
                ostA = i;
                ostB = i;
                ostC = i;
            }
            public Xromossomi() { }
        }

        /// <summary>
        /// Данная функция создаём список из хромосом размеров переданного в аргументе
        /// </summary>
        /// <param name="size">Размер списка из хромосом</param>
        /// <returns></returns>
        public List<Xromossomi> SetXrom(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var s = nachlist();
                if (!nabor.Contains(s))
                {
                    nabor.Add(nachlist());
                }
                else
                {
                    i--;
                }
            }
            //xor(size);

            return nabor;
        }

        public int getSelectionPopulation(SelectoinType selection, out int s)
        {
            // TODO: Данная функция не будет работать без вызова, прежде, calcGAFitnessList

            List<int> SortFitnessList = new List<int>(_fitnesslist);
            SortFitnessList.Sort();
            s = _fitnesslist.IndexOf(SortFitnessList[0]);
            return SortFitnessList[0];
        }

        public int calcSetsFitnessList(bool directedTime , decimal GenerationCount,int countHromos = 50)
        {

            // Двумерный список
            var flagmanHromosom = new List<List<int>>();

            // Для каждого типа днных выполняем обработку
            for (var dataType = 0; dataType < dataTypesCount; dataType++)
                flagmanHromosom.Add( new List<int> {batchCount - 2 ,  2});
            
                

            var r = ToArrayList();
            var test = new Sets(Form1.compositionSets, Form1.timeSets);
            var FitnessList = new List<int>();
            List<int> CountTime = new List<int>();

            for (int i = 0; i < GenerationCount + 1; i++)
            {
                this.SetXrom(countHromos);
                r = ToArrayList();
                r.Add(flagmanHromosom);
                foreach (var elem in r)
                {
                    var shedule = new Shedule(elem);
                    shedule.ConstructShedule();
                    shedule.BuildMatrixRWithTime();
                    test.GetSolution(shedule.ReturnMatrixRWithTime());
                    CountTime.Add(test.GetNewCriterion(directedTime));
                }
            }
           
            return CountTime.ToArray().Min();
        }

        private List<List<List<int>>> ToArrayList()
        {
            List<List<List<int>>> r = new List<List<List<int>>>();
            foreach (var elem in nabor)
            {
                foreach (var elem2 in elem.GenList)
                {
                    elem2.Sort();
                    elem2.Reverse();
                    elem2.RemoveAll(e => e == 0);
                }

                r.Add(elem.GenList);
            }
            return r;
        }

        public List<List<int>> calcFitnessList()
        {
            var r = ToArrayList();
            var FitnessList = new List<int>();
            var tuple = new Dictionary<int, List<List<int>>>();

            // Инициализируем вектор
            var listint = new List<int>();

            foreach (var elem in r)
            {
                // Создаём расписание на основе каждого элемента матрица r
                Shedule shedule = new Shedule(elem);
               
                var time = shedule.GetTime();
                
                if(tuple.ContainsKey(time))
                    continue;
                tuple.Add(time, elem);

                // Добавляем в вектор время
                listint.Add(time);



            }

            return tuple[listint.Min()];
        }

        private Xromossomi nachlist()
        {
            // Инициализируем хромосому 
            Xromossomi xrom = new Xromossomi();

            // Инициализируем в xrom вектор GenList длиной dataTypesCount, как наполненный списками. 
            for (int dataType = 0; dataType < dataTypesCount; dataType++)
                xrom.GenList.Add(new List<int>());

            // Добавляем в вектор GenListOst все элементы из списка batchCountList
            xrom.GenListOst.AddRange(batchCountList);

            for (int GenElement = 0; GenElement < xrom.GenList.Count; GenElement++)
            {
                int buff = 0;
                for (int _batchCount = 0; _batchCount < batchCount / 2 - 1; _batchCount++)
                {
                    if (xrom.GenListOst[GenElement] == 2)
                    {
                        buff = 2;
                        xrom.GenListOst[GenElement] = 0;
                    }
                    else if (xrom.GenListOst[GenElement] == 1)
                    {
                        xrom.GenList[GenElement][xrom.GenList[GenElement].Count - 1]++;
                        xrom.GenListOst[GenElement] = 0;
                        buff = 0;
                    }
                    else if (xrom.GenListOst[GenElement] == 0)
                        buff = 0;
                    else
                        xrom.GenListOst[GenElement] -= buff = rand.Next(2, xrom.GenListOst[GenElement]);
                    xrom.GenList[GenElement].Add(buff);
                }
                xrom.GenList[GenElement].Add(xrom.GenListOst[GenElement]);
            }
            return xrom;
        }

        #region Неиспользуемые функции

        private void xor(int size, List<Xromossomi> nabr = null)
        {
            List<Xromossomi> naborInternal = new List<Xromossomi>(nabor);
            if (nabr != null)
            {
                naborInternal = new List<Xromossomi>(nabr);
            }


            int[] massA = new int[size];
            int[] massB = new int[size];
            int[] massC = new int[size];
            int buff;
            int v;
            for (int i = 0; i < size; i++)
            {
                massA[i] = i;
            }
            List<Xromossomi> nabor1 = new List<Xromossomi>();
            for (int i = 0; i < size; i++)
                nabor1.Add(new Xromossomi());

            for (int i = 0; i < size; i++)
            {
                buff = massA[v = rand.Next(size)];
                massA[v] = massA[v = rand.Next(size)];
                massA[v] = buff;
            }
            //
            for (int i = 0; i < size; i++)
            {
                buff = massB[v = rand.Next(size)];
                massB[v] = massB[v = rand.Next(size)];
                massB[v] = buff;
            }
            //
            for (int i = 0; i < size; i++)
            {
                buff = massC[v = rand.Next(size)];
                massC[v] = massC[v = rand.Next(size)];
                massC[v] = buff;
            }

            for (int i = 0; i < size; i++)
            {
                nabor1[i].GenA = naborInternal[massA[i]].GenA;
                nabor1[i].GenB = naborInternal[massA[i]].GenB;
                nabor1[i].GenC = naborInternal[massA[i]].GenC;
            }
            naborInternal = nabor1;
        }

        private List<List<int>> TestArray()
        {
            List<List<int>> a1 = new List<List<int>>();
            List<int> a = new List<int>();
            a.Add(10);
            a.Add(2);
            List<int> b = new List<int>();
            b.Add(10);
            b.Add(2);
            List<int> c = new List<int>();
            c.Add(10);
            c.Add(2);

            a1.Add(a);
            a1.Add(b);
            a1.Add(c);
            return a1;
        }

        private List<List<int>> GenerateR(IReadOnlyList<List<int>> m)
        {
            var result = new List<List<int>>();
            var summ = m.Sum(t => t.Count);
            for (int dataType = 0; dataType < dataTypesCount; dataType++)
            {
                result.Add(new List<int>());
                for (var j = 0; j < summ; j++)
                {
                    result[dataType].Add(0);
                }
            }
            var ind = 0;
            for (var i = 0; i < m.Count; i++)
            {
                for (var j = 0; j < m[i].Count; j++)
                {
                    result[i][ind] = m[i][j];
                    ind++;
                }
            }
            return result;
        }

        private int[] calcGAFitnessList()
        {
            var r = ToArrayList();
            List<int> CountTime = new List<int>();
            var FitnessList = new List<int>();
            var GaaSecondLevel = new GaaSecondLevel();
            foreach (var elem in r)
            {
                var listint = new List<Shedule>();
                for (var i = 0; i < 50; i++)
                {

                    var shedule = new Shedule(GaaSecondLevel.GetGaaSecondLevelGroup(elem));
                    shedule.ConstructShedule();
                    listint.Add(shedule);
                }
                var timelist = listint.Select(list => list.GetTime());
                FitnessList.Add(timelist.Min());


            }
            _fitnesslist = FitnessList;
            return CountTime.ToArray();
        }

        private List<List<List<int>>> ToArray()
        {
            List<List<List<int>>> arrResult = new List<List<List<int>>>();
            foreach (var hromosoma in nabor)
            {
                List<List<int>> arr = new List<List<int>>();
                int index = 0;
                arr.Add(new List<int>());
                for (int i = 0; i < hromosoma.GenA.Count; i++)
                {
                    if (hromosoma.GenA[i] > 0)
                        arr[index].Add(hromosoma.GenA[i]);
                }
                index++;
                arr.Add(new List<int>());
                //2 строка
                for (int i = 0; i < hromosoma.GenB.Count; i++)
                {
                    if (hromosoma.GenB[i] > 0)
                        arr[index].Add(hromosoma.GenB[i]);
                }
                index++;
                arr.Add(new List<int>());
                //3 строка
                for (int i = 0; i < hromosoma.GenC.Count; i++)
                {
                    if (hromosoma.GenC[i] > 0)
                        arr[index].Add(hromosoma.GenC[i]);
                }

                foreach (var elem in arr)
                {
                    elem.Sort();
                    elem.Reverse();
                }
                arrResult.Add(new  List<List<int>>(arr));
            }
            
            return arrResult;

        }

        private void mutation()
        {
            Random rand = new Random();

            int i;
            foreach (var elem1 in nabor)
            {

                for (int i1 = 0; i1 < 1; i1++)
                {
                    if (elem1.GenA[i = rand.Next(elem1.GenA.Count - 1)] - 2 > 1)
                    {
                        elem1.GenA[i] -= 2;
                        elem1.GenA[rand.Next(elem1.GenA.Count - 1)] += 2;
                    }
                    if (elem1.GenB[i = rand.Next(elem1.GenB.Count - 1)] - 2 > 1)
                    {
                        elem1.GenB[i] -= 2;
                        elem1.GenB[rand.Next(elem1.GenB.Count - 1)] += 2;
                    }
                    if (elem1.GenC[i = rand.Next(elem1.GenC.Count - 1)] - 2 > 1)
                    {
                        elem1.GenC[i] -= 2;
                        elem1.GenC[rand.Next(elem1.GenC.Count - 1)] += 2;
                    }
                }
            };

        }

        private Xromossomi nach()
        {


            Xromossomi xrom = new Xromossomi();
            int buff = 0;

            for (int i = 0; i < batchCount / 2 - 1; i++)
            {
                if (xrom.ostA == 2)
                {
                    buff = 2;
                    xrom.ostA = 0;
                }
                else
                    if (xrom.ostA == 1)
                {
                    xrom.GenA[xrom.GenA.Count - 1]++;
                    xrom.ostA = 0;
                    buff = 0;
                }
                else
                        if (xrom.ostA == 0)
                    buff = 0;
                else
                    xrom.ostA -= buff = rand.Next(2, xrom.ostA);
                xrom.GenA.Add(buff);
            }
            xrom.GenA.Add(xrom.ostA);
            //
            int s = batchCount % 3 == 2 ? batchCount / 3 + 1 : batchCount / 3;
            int t = batchCount % 3 == 1 || batchCount % 3 == 2 ? batchCount / 3 + 1 : batchCount / 3;
            buff = 0;
            for (int i = 0; i < batchCount / 2 - 1; i++)
            {
                if (xrom.ostB == 0 && 10 == xrom.GenA.Sum() && xrom.GenA[2] != 0)
                {

                }

                if (xrom.ostB == 2)
                {
                    buff = 2;
                    xrom.ostB = 0;
                }
                else
                    if (xrom.ostB == 1)
                {
                    if (xrom.GenB.Count == 0)
                        xrom.GenA[(batchCount / 3) - 1]++;
                    else
                        xrom.GenB[xrom.GenB.Count - 1]++;
                    xrom.ostB = 0;
                }
                else
                        if (xrom.ostB == 0)
                    buff = 0;
                else
                    xrom.ostB -= buff = rand.Next(2, xrom.ostB);
                xrom.GenB.Add(buff);
            }
            xrom.GenB.Add(xrom.ostB);

            ///
            for (int i = 0; i < batchCount / 2 - 1; i++)
            {
                if (xrom.ostC == 2)
                {
                    buff = 2;
                    xrom.ostC = 0;
                }
                else
                    if (xrom.ostC == 1)
                {
                    xrom.GenC[xrom.GenC.Count - 1]++;
                    xrom.ostC = 0;
                }
                else
                        if (xrom.ostC == 0)
                    buff = 0;
                else
                    xrom.ostC -= buff = rand.Next(2, xrom.ostC);
                xrom.GenC.Add(buff);
            }

            if (xrom.ostC == 1)
            {
                xrom.GenC[xrom.GenC.Count - 1]++;
                xrom.ostC = 0;
            }
            else
                xrom.GenC.Add(xrom.ostC);

            return xrom;
        }
        
        #endregion
    }
}
