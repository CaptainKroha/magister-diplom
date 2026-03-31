using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Diplom;

namespace newAlgorithm
{
    class OldFirstLevel
    {
        public static bool flag = false;
        private readonly List<int> I;//Вектор интерпритируемых типов данных
        private readonly List<int> Ii;//Вектор интерпритируемых типов данных на текущем шагу алгоритма
        private readonly List<int> mi;//Вектор количества партий данных для каждого типа данных
        private readonly List<int> np1i;//задание для каждого i-го типа данных количества решений по составам партий данных i-ых типов, сформированных на текущей ите-рации алгоритма
        private readonly List<int> np2i;//задание для каждого i-го типа данных количества решений по составам партий данных i-ых типов, сформированных на текущей ите-рации алгоритма
        private List<List<int>> A1i;//Буферизированная матрица составов партий требований на k+1 шаге 
        private List<List<int>> A1;//Матрица составов партий требований на k+1 шаге 
        private List<List<int>> A2;//Матрица составов партий требований фиксированного типа
        private List<List<int>> A;//Матрица составов партий требований на k шаге
        private List<List<int>> ABuf;//Буферизированная матрица составов партий требований

        /// <summary>
        /// Данная переменная описывает количество типов данных обрабатываемых в конвейерной системе
        /// </summary>
        private readonly int dataTypesCount;

        private readonly List<int> countClaims;//Начальное количество требований для каждого типа данных
        private BatchTypeClaims test;
        private int i;//идентификатор текущего изменяемого типа
        private int q2;
        private int k;
        private int f1;//Критерий текущего решения для всех типов
        private int f1Buf;//Критерий текущего решения для всех типов
        private bool solutionFlag;

        /* 
         * Конструктор с параметрами
         * 
         * count_type - количество типов рассматриваемых данных
         * count_claims - количество требований всех типов данных
         * 
         */
        public OldFirstLevel(int count_type, List<int> count_claims)
        {
            this.dataTypesCount = count_type;
            this.countClaims = count_claims;
            this.mi = new List<int>(this.dataTypesCount);
            this.np1i = new List<int>(this.dataTypesCount);
            this.np2i = new List<int>(this.dataTypesCount);
            this.I = new List<int>(this.dataTypesCount);
            this.Ii = new List<int>(this.dataTypesCount);
        }

        /*
         * Функция копирования значений между матрицами, предотвращающая копирование указателей
         * 
         */
        private List<List<int>> CopyMatrix(List<List<int>> inMatrix)
        {
            List<List<int>> ret = new List<List<int>>();
            for (int i = 0; i < inMatrix.Count; i++)
            {
                ret.Add(new List<int>());
                for (int j = 0; j < inMatrix[i].Count; j++)
                {
                    ret[i].Add(inMatrix[i][j]);
                }
            }
            return ret;
        }

        /*
         * Алгоритм формирования начальных решений по составам партий всех типов
         *  
         */
        public void GenerateStartSolution()
        {
            int claim = 2;
            this.A = new List<List<int>>
            {
                new List<int>()
            };
            for (int i = 1; i <= this.dataTypesCount; i++)
            {
                this.I.Add(1);
                this.Ii.Add(1);
                this.mi.Add(claim);
                this.np1i.Add(1);
                this.np2i.Add(1);
                this.A.Add(new List<int>());
                this.A[i].Add(0);
                this.A[i].Add(this.countClaims[i - 1] - claim);
                this.A[i].Add(claim);
            }
            for (int i = 1; i <= this.dataTypesCount; i++)
            {
                if (this.A[i][1] < 2 || this.A[i][1] < this.A[i][2])
                {
                    this.A[i].Clear();
                    this.A[i].Add(0);
                    this.A[i].Add(this.countClaims[i - 1]);
                    this.mi[i - 1] = 1;
                    this.np1i[i - 1] = 0;
                    this.I[i - 1] = 0;
                    this.Ii[i - 1] = 0;
                }
            }
        }

        /*
         * Функция вычисления f1 критерия
         * 
         */
        public int GetCriterion(List<List<int>> inMatrix)
        {
            int criterion = 0;
            for (int i = 1; i < inMatrix.Count; i++)
            {
                for (int j = 1; j < inMatrix[i].Count; j++)
                {
                    criterion += inMatrix[i][j];
                }
            }
            int criterionA = 0;
            for (int i = 1; i < this.A.Count; i++)
            {
                for (int j = 1; j < this.A[i].Count; j++)
                {
                    criterionA += this.A[i][j];
                }
            }
            return criterion;
            /*Random rand = new Random();
            int ret = rand.Next(5,15);
            return ret;*/
        }

        /*
         * Функция проверок для реализации алгоритма 
         * 
         * flag == 1 - проверка на количество требований в соответствии с теоремой 1
         * flag == 2 - проверка на количество требований в первой партии в соответствии с теоремой 1
         * 
         */
        public bool CheckingMatrix(int flag)
        {
            switch (flag)
            {
                case 1:
                    try
                    {
                        for (int i = 2; i < this.A2[1].Count; i++)
                            if (this.A2[1][1] < this.A2[1][i])
                            {
                                return false;
                            }
                    }
                    catch
                    {
                        return false;
                    }
                    break;
                case 2:
                    try
                    {
                        if (this.A2[1][1] < 2)
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        /// <summary>
        /// Данная функция выполняет проверку наличия оставшихся в рассмотрении типов переденного списка
        /// </summary>
        /// <param name="type">Список на проверку</param>
        /// <returns>false, если типы остались, иначе true</returns>
        private bool CheckType(List<int> type)
        {

            // Выполняем обработку для каждого типа
            for (int dataType = 0; dataType < this.dataTypesCount; dataType++)

                // Если в списке остались типы на рассмотрении
                if (type[dataType] != 0)

                    // Возвращаем false
                    return false;
            
            // Возвращаем true
            return true;
        }

        /*
         * Алгоритм формирования решения по составам паритй всех типов данных
         * 
         */
        public void GenerateSolution()
        {
            this.GenerateStartSolution();
            OldSecondLevel secondLevel = new OldSecondLevel();
            List<List<int>> temp = this.CopyMatrix(this.A);
            secondLevel.GenerateSolution(temp);
            List<List<int>> tmpMatrixA = secondLevel.ReturnAMatrix();
            this.k = 0;
            this.f1 = this.GetCriterion(tmpMatrixA);
            /*if (this.f1 == 0)
            {
                this.f1 = 99999;
            }*/
            //this.f1 = 20;
            this.f1Buf = this.f1;

            //Добавить вычисление значения критерия
            List<List<int>> MaxA = this.CopyMatrix(this.A);
            int maxF1 = this.f1;
            string s;
            if (flag == true)
            {
                while (!this.CheckType(this.I))
                {
                    this.solutionFlag = false;
                    //1 - Копируем I в Ii
                    for (int j = 0; j < this.dataTypesCount; j++)
                    {
                        this.Ii[j] = this.I[j];
                    }
                    this.ABuf = this.CopyMatrix(this.A);
                    //this.f1 = this.GetCriterion(this.ABuf);
                    //Для каждого рассматриваемого типа
                    for (int iter = 0; iter < this.Ii.Count; iter++)
                    {
                        if (this.Ii[iter] != 0)
                        {
                            this.i = iter;
                            if (this.np1i[this.i] > 0)
                            {
                                this.A1 = this.CopyMatrix(this.ABuf);
                                //MessageBox.Show("Решение по составу партий данных " + (this.i + 1) + " типа на " + (this.k + 1) + " шаге алгоритма");
                                //Получение состава партий фиксированного типа
                                List<List<int>> toBatchAlgoritm = new List<List<int>>
                                {
                                    new List<int>(),
                                    new List<int>()
                                };
                                toBatchAlgoritm[1] = this.A1[this.i + 1];
                                test = new BatchTypeClaims(this.f1, this.i + 1, this.countClaims[this.i], toBatchAlgoritm, this.A1);
                                test.GenerateSolution();

                                //test.PrintMatrix(2);
                                //test.PrintMatrix(3);

                                List<List<int>> tempA2 = test.ReturnMatrix(3);
                                if (tempA2.Count < 2)
                                {
                                    this.mi[this.i]++;
                                    this.A2 = new List<List<int>>
                                    {
                                        new List<int>(),
                                        new List<int>()
                                    };
                                    this.A2[1].Add(0);
                                    this.A2[1].Add(0);
                                    int sum = 0;
                                    for (int j = 1; j < this.mi[this.i]; j++)
                                    {
                                        this.A2[1].Add(2);
                                        sum += 2;
                                    }
                                    this.A2[1][1] = this.countClaims[this.i] - sum;
                                    if (!this.CheckingMatrix(1) && !this.CheckingMatrix(2))
                                    {
                                        this.I[this.i] = 0;
                                        this.mi[this.i] = 0;
                                        continue;
                                    }
                                }
                                else
                                {
                                    //this.A2 = this.CopyMatrix(tempA2);
                                    this.A2 = new List<List<int>>
                                    {
                                        new List<int>()
                                    };
                                    for (int ii = 1; ii < tempA2.Count; ii++)
                                    {
                                        //this.A2.Add(new List<int>());
                                        this.A2.Insert(ii, tempA2[ii]);
                                        this.A2[ii].Insert(0, 0);
                                    }
                                }

                                //Буферизируем текущее решение для сравнения
                                this.A1i = this.CopyMatrix(this.A1);

                                for (this.q2 = 1; this.q2 < this.A2.Count; this.q2++)
                                {
                                    this.A1i[this.i + 1] = this.A2[this.q2];
                                    secondLevel = new OldSecondLevel();
                                    List<List<int>> tempA = CopyMatrix(this.A1i);
                                    secondLevel.GenerateSolution(tempA);
                                    List<List<int>> tempMatrixA = secondLevel.ReturnAMatrix();
                                    int f1g = this.GetCriterion(tempMatrixA);
                                    // Random rand = new Random();
                                    // int ret = rand.Next(5, 15);
                                    // if(ret < 10)
                                    if (f1g >= this.f1Buf)
                                        //if (f1g - this.f1Buf < 0 || f1g == 0)
                                    {
                                        this.f1Buf = f1g;
                                        this.solutionFlag = true;
                                        this.ABuf = this.CopyMatrix(this.A1i);
                                    }
                                }
                            }
                        }
                    }
                    this.k++;
                    if (this.solutionFlag)
                    {
                        if (this.f1Buf > this.f1)
                        {
                            MaxA = this.CopyMatrix(ABuf);
                            maxF1 = this.f1Buf;
                        }
                        this.f1 = this.f1Buf;
                        this.A = this.CopyMatrix(ABuf);
                        s = "Решение на текущем шаге\n";
                        foreach (List<int> row in this.A)
                        {
                            foreach (int colum in row)
                            {
                                if (colum >= 2)
                                    s += colum + ", ";
                            }
                            s += "\n";
                        }
                        MessageBox.Show(s);
                    }
                    else
                    {
                        /*
                        //this.A = new List<List<int>>();
                        //this.A.Add(new List<int>());
                        this.A[currentChangeType + 1].Clear();
                        //for (int ii = 0; ii < this.I.Count; ii++)
                        //{
                        //this.A.Add(new List<int>());
                        this.A[currentChangeType + 1].Add(0);
                        this.A[currentChangeType + 1].Add(0);
                        int sum = 0;
                        //this.mi[currentChangeType]++;
                        for (int j = 1; j < this.mi[currentChangeType]; j++)
                        {
                            this.A[currentChangeType + 1].Add(2);
                            sum += 2;
                        }
                        this.A[currentChangeType + 1][1] = this.countClaims[currentChangeType] - sum;
                        currentChangeType++;
                        if (currentChangeType == this.countType)
                        {
                            currentChangeType = 0;
                        }
                        //}
                        */
                    }
                }
            }
            MessageBox.Show("На текущем " + this.k + " шаге получено решение");
            s = "Максимальное решение\n";
            foreach (List<int> row in MaxA)
            {
                foreach (int colum in row)
                {
                    if (colum >= 2)
                        s += colum + ", ";
                }
                s += "\n";
            }
            MessageBox.Show(s);
            MessageBox.Show("Количество обработанных требований " + maxF1);
        }
    }
}
