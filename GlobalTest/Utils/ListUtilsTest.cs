using magisterDiplom.Utils;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using newAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTest.Utils
{
    /// <summary>
    /// Данный класс описывает тесты для модуля ListUtils
    /// </summary>
    [TestClass]
    public class ListUtilsTest
    {

        [TestMethod]
        public void VectorIntToStringTest_default()
        {

            List<int> input = new() { 1, 2 };
            string output = "1 2";
            Assert.AreEqual(output, ListUtils.VectorIntToString(input));
        }

        [TestMethod]
        public void VectorIntToStringTest_separator()
        {

            List<int> input = new() { 1, 2 };
            string output = "1552";
            Assert.AreEqual(output, ListUtils.VectorIntToString(input, "55"));
        }

        [TestMethod]
        public void VectorIntToStringTest_prefix()
        {

            List<int> input = new() { 1, 2 };
            string output = "\t1 2";
            Assert.AreEqual(output, ListUtils.VectorIntToString(input, " ", "\t"));
        }

        [TestMethod]
        public void VectorIntToStringTest_postfix()
        {

            List<int> input = new() { 1, 2 };
            string output = "1 2\t";
            Assert.AreEqual(output, ListUtils.VectorIntToString(input, " ", "", "\t"));
        }

        [TestMethod]
        public void VectorIntToStringTest_equalsValuesDeafault()
        {
            List<int> input = new() { 6, 6 };
            string output = "6 6";
            Assert.AreEqual(output, ListUtils.VectorIntToString(input, " ", "", ""));
        }

        [TestMethod]
        public void MatrixIntToStringTest_default()
        {

            List<List<int>> input = new()
            {
                new() { 1, 2 },
                new() { 3, 4 },
            };
            string output = $"1 2{Environment.NewLine}3 4{Environment.NewLine}";
            Assert.AreEqual(output, ListUtils.MatrixIntToString(input));
        }

        [TestMethod]
        public void MatrixIntToStringTest_separator()
        {

            List<List<int>> input = new() { new List<int> { 1, 2 }, new List<int> { 3, 4 } };
            string output = $"15582{Environment.NewLine}35584{Environment.NewLine}";
            Assert.AreEqual(output, ListUtils.MatrixIntToString(input, "558"));
        }

        [TestMethod]
        public void MatrixIntToStringTest_prefix()
        {

            List<List<int>> input = new() { new List<int> { 1, 2 }, new List<int> { 3, 4 } };
            string output = $"\t1 2{Environment.NewLine}\t3 4{Environment.NewLine}";
            Assert.AreEqual(output, ListUtils.MatrixIntToString(input, " ", "\t"));
        }

        [TestMethod]
        public void MatrixIntToStringTest_postfix()
        {

            List<List<int>> input = new() {
                new() { 1, 2 },
                new() { 3, 4 }
            };
            string output = $"\t1 2\t{Environment.NewLine}\t3 4\t{Environment.NewLine}";
            Assert.AreEqual(output, ListUtils.MatrixIntToString(input, " ", "\t", "\t"));
        }

        [TestMethod]
        public void VectorIntDeepCopy()
        {

            List<int> input = new() { 1, 2 };
            List<int> output = ListUtils.VectorIntDeepCopy(input);

            // Проверяем результат копирования
            Assert.AreEqual(output[0], input[0]);
            Assert.AreEqual(output[1], input[1]);

            // Изменяем исходный вектор и выполняем проверку
            input[0] = 0;
            Assert.AreNotEqual(output[0], input[0]);
            Assert.AreEqual(output[1], input[1]);
        }

        [TestMethod]
        public void MatrixIntDeepCopy()
        {

            List<List<int>> input = new() {
                new() { 1, 2 },
                new() { 3, 4 },
            };
            List<List<int>> output = ListUtils.MatrixIntDeepCopy(input);

            // Проверяем результат копирования
            Assert.AreEqual(output[0][0], input[0][0]);
            Assert.AreEqual(output[0][1], input[0][1]);
            Assert.AreEqual(output[1][0], input[1][0]);
            Assert.AreEqual(output[1][1], input[1][1]);

            // Изменяем исходный вектор и выполняем проверку
            input[0][0] = 0;
            input[1][1] = 0;
            Assert.AreNotEqual(output[0][0], input[0][0]);
            Assert.AreEqual(output[0][1], input[0][1]);
            Assert.AreEqual(output[1][0], input[1][0]);
            Assert.AreNotEqual(output[1][1], input[1][1]);
        }

        [TestMethod]
        public void MatrixIntDeepCopy_2()
        {

            List<List<int>> input = new()
            {
                new() { 1, 2 },
                new() { 3, 4, 5 }
            };
            List<List<int>> output = ListUtils.MatrixIntDeepCopy(input);

            // Проверяем результат копирования
            Assert.AreEqual(output[0][0], input[0][0]);
            Assert.AreEqual(output[0][1], input[0][1]);
            Assert.AreEqual(output[1][0], input[1][0]);
            Assert.AreEqual(output[1][1], input[1][1]);
            Assert.AreEqual(output[1][2], input[1][2]);

            // Изменяем исходный вектор и выполняем проверку
            input[0][0] = 0;
            input[1][1] = 0;
            Assert.AreNotEqual(output[0][0], input[0][0]);
            Assert.AreEqual(output[0][1], input[0][1]);
            Assert.AreEqual(output[1][0], input[1][0]);
            Assert.AreNotEqual(output[1][1], input[1][1]);
            Assert.AreEqual(output[1][2], input[1][2]);

            // Изменяем исходный вектор и выполняем проверку
            input[1][2] = 0;
            Assert.AreNotEqual(output[0][0], input[0][0]);
            Assert.AreEqual(output[0][1], input[0][1]);
            Assert.AreEqual(output[1][0], input[1][0]);
            Assert.AreNotEqual(output[1][1], input[1][1]);
            Assert.AreNotEqual(output[1][2], input[1][2]);
        }

        [TestMethod]
        public void MatrixIntRowSwapTest()
        {

            var output = new List<List<int>> {
                new() { 3, 4 },
                new() { 1, 2 },
                new() { 5, 6 },
            };
            var input = new List<List<int>> {
                new() { 1, 2 },
                new() { 3, 4 },
                new() { 5, 6 },
            };
            ListUtils.MatrixIntRowSwap(input, 0, 1);

            for (int i = 0; i < output.Count; i++)
                for (int j = 0; j < output[i].Count; j++)
                    Assert.AreEqual(output[i][j], input[i][j]);

        }

        [TestMethod]
        public void MatrixIntRowSwapTest_Panic()
        {

            var output = new List<List<int>> {
                new() { 1, 2 },
                new() { 3, 4 },
                new() { 5, 6 },
            };
            var input = new List<List<int>> {
                new() { 1, 2 },
                new() { 3, 4 },
                new() { 5, 6 },
            };
            ListUtils.MatrixIntRowSwap(input, 0, 5);

            for (int i = 0; i < output.Count; i++)
                for (int j = 0; j < output[i].Count; j++)
                    Assert.AreEqual(output[i][j], input[i][j]);

        }

        [TestMethod]
        public void MatrixIntColumnSwapTest()
        {

            var output = new List<List<int>> {
                new() { 2, 1 },
                new() { 4, 3 },
                new() { 6, 5 }
            };
            var input = new List<List<int>> {
                new() { 1, 2 },
                new() { 3, 4 },
                new() { 5, 6 }
            };
            ListUtils.MatrixIntColumnSwap(input, 0, 1);

            for (int i = 0; i < output.Count; i++)
                for (int j = 0; j < output[i].Count; j++)
                    Assert.AreEqual(output[i][j], input[i][j]);

        }

        [TestMethod]
        public void MatrixIntColumnSwapTest_Panic()
        {

            var output = new List<List<int>> {
                new() { 1, 2 },
                new() { 3, 4 },
                new() { 5, 6 }
            };
            var input = new List<List<int>> {
                new() { 1, 2 },
                new() { 3, 4 },
                new() { 5, 6 }
            };
            ListUtils.MatrixIntColumnSwap(input, 0, 5);

            for (int i = 0; i < output.Count; i++)
                for (int j = 0; j < output[i].Count; j++)
                    Assert.AreEqual(output[i][j], input[i][j]);

        }

        [TestMethod]
        public void IsMatrixIntEqualTest_1()
        {

            var output = new List<List<int>> {
                new() { 1, 2 },
                new() { 3, 4 },
                new() { 5, 6 }
            };
            var input = new List<List<int>> {
                new() { 1, 2 },
                new() { 3, 4 },
                new() { 5, 6 }
            };

            Assert.IsTrue(ListUtils.IsMatrixIntEqual(output, input));
        }

        [TestMethod]
        public void IsMatrixIntEqualTest_2()
        {

            var output = new List<List<int>> {
                new() { 2, 2 },
                new() { 2, 2 },
                new() { 2, 2 }
            };
            var input = new List<List<int>> {
                new() { 3, 3 },
                new() { 3, 3 },
                new() { 3, 3 }
            };

            Assert.IsFalse(ListUtils.IsMatrixIntEqual(output, input));
        }

        [TestMethod]
        public void IsMatrixIntEqualTest_3()
        {

            var output = new List<List<int>> {
                new() { 2, 2 },
                new() { 2, 2, 4 },
                new() { 2, 2 }
            };
            var input = new List<List<int>> {
                new() { 3, 3 },
                new() { 3, 3 },
                new() { 3, 3 }
            };

            Assert.IsFalse(ListUtils.IsMatrixIntEqual(output, input));
        }

        [TestMethod]
        public void InitVectorIntTest_default()
        {
            List<int> output = new(3) { 0, 0, 0 };
            List<int> res = ListUtils.InitVectorInt(3);

            if (output.Count != res.Count)
                Assert.Fail();

            for (int i = 0; i < res.Count; i++)
                Assert.AreEqual(output[i], res[i]);
        }

        [TestMethod]
        public void InitVectorIntTest_1()
        {
            List<int> output = new(3) { 3, 3, 3 };
            List<int> res = ListUtils.InitVectorInt(3, 3);

            if (output.Count != res.Count)
                Assert.Fail();

            for (int i = 0; i < res.Count; i++)
                Assert.AreEqual(output[i], res[i]);
        }

        [TestMethod]
        public void InitVectorIntTest_2()
        {
            List<int> res = ListUtils.InitVectorInt(-1);

            if (res != null)
                Assert.Fail();
        }

        [TestMethod]
        public void InitMatrixIntTest_default()
        {
            List<List<int>> output = new()
            {
                new() { 0, 0, 0 },
                new() { 0, 0, 0 },
                new() { 0, 0, 0 },
            };
            List<List<int>> matrix = ListUtils.InitMatrixInt(3, 3);

            Assert.IsTrue(ListUtils.IsMatrixIntEqual(output, matrix));
        }

        [TestMethod]
        public void InitMatrixIntTest_1()
        {
            List<List<int>> output = new()
            {
                new() { 4, 4, 4 },
                new() { 4, 4, 4 },
                new() { 4, 4, 4 },
            };
            List<List<int>> matrix = ListUtils.InitMatrixInt(3, 3, 4);

            Assert.IsTrue(ListUtils.IsMatrixIntEqual(output, matrix));
        }

        [TestMethod]
        public void MatrixIntFlipTest_1()
        {
            List<List<int>> output = new()
            {
                new() { 4, 4, 4 },
                new() { 4, 4, 4 },
                new() { 4, 4, 4 }
            };
            List<List<int>> matrix = ListUtils.MatrixIntFlip(output);

            Assert.IsTrue(ListUtils.IsMatrixIntEqual(output, matrix));
        }

        [TestMethod]
        public void MatrixIntFlipTest_2()
        {
            List<List<int>> output = new()
            {
                new() { 4, 4, 4 },
                new() { 4, 3, 4 },
                new() { 4, 4, 4 }
            };
            List<List<int>> matrix = ListUtils.MatrixIntFlip(output);

            Assert.IsTrue(ListUtils.IsMatrixIntEqual(output, matrix));
        }

        [TestMethod]
        public void MatrixIntFlipTest_3()
        {
            List<List<int>> input = new()
            {
                new() { 1, 4, 4, 4 },
                new() { 4, 2, 4, 4 },
                new() { 4, 4, 3, 5 }
            };
            List<List<int>> output = new()
            {
                new() { 1, 4, 4 },
                new() { 4, 2, 4 },
                new() { 4, 4, 3 },
                new() { 4, 4, 5 },
            };
            List<List<int>> matrix = ListUtils.MatrixIntFlip(input);

            Assert.IsTrue(ListUtils.IsMatrixIntEqual(output, matrix));
        }

    }

}
