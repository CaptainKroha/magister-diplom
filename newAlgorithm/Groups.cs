using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newAlgorithm
{
    class Batch
    {
        //Структура, необходимая для работы второго уровня (составов групп партий данных)
        public int countClaims;//так ты их описал
        public int typeClaims;
        public List<int> claims;

        public Batch(int valueCount, int valueType)
        {
            // this.SetCountClaims(valueCount, valueClaims);
            this.SetCountClaims(valueCount);
            this.SetTypeClaims(valueType);
        }

        public int GetCountClaims()
        {
            return this.countClaims;
        }

        public int GetTypeBid()
        {
            return this.typeClaims;
        }

        // public void SetCountClaims(int value, List<int> valueClaims)
        // {
        //     this.countClaims = value;
        //     this.claims = new List<int>(this.countClaims);
        // }
        public void SetCountClaims(int value)
        {
            this.countClaims = value;
            this.claims = new List<int>(this.countClaims);
        }

        public void SetTypeClaims(int value)
        {
            this.typeClaims = value;
        }

        public void IncrementCountClaims(int value)
        {
            this.countClaims += value;
        }

        public void DecrementCountClaims(int value)
        {
            this.countClaims -= value;
        }
    }
    class Groups
    {
        public int Z = 5;//допустим
        public List<int> M = new List<int>();
        public int hi1 = 0;//dsdsdиндекс партии данных  i-го типа, размещаемой в группе  , число данных в которой соответ-ствует значению элемента   матрицы А. Я не понимаю что это но оно нужно для алгоритма
        public List<List<Batch>> groups = null;
        public List<int> Nz = new List<int>();//множество номеров (идентификаторов) групп партий
        public List<List<List<int>>> Nz1 = new List<List<List<int>>>();//сами группы
        public List<int> Nzt = new List<int>();//текущее (изменяемое) множество номеров групп партий, с которым оперирует алгоритм
        public int i = 0;//тип данных, партия которых размещается в группе
        public int z = 0;//индекс текущей рассматриваемой группы, в которую добавляется партия  -го типа;
        public List<int> k = new List<int>();//количествоелементов в Nz
        public List<int> I1 = new List<int>();//текущее (изменяемое) множество типов данных, партии которых размещаются в группах
        public List<int> I2 = new List<int>();

        public Groups(int Z1)
        {
            this.Z = Z1;
            for (int j = 0; j < Z1; j++)
                this.I1.Add(j);
            for (int j = 0; j < Z1; j++)
                this.Nz.Add(j);
        }
        public void Set_I1(int i)
        {
            for (int j = 0; j < i; j++)
                this.I1.Add(j);
        }
        public void Set_I2(int i)
        {
            for (int j = 0; j < i; j++)
                this.I2.Add(j);
        }
        public void Set_M(int i)
        {
            for (int j = 0; j < i; j++)
                this.M.Add(0);
        }
        public void Set_Nzt(List<int> i)
        {
            this.Nzt.Clear();
            this.Nzt.AddRange(i);
        }
    }   
}
