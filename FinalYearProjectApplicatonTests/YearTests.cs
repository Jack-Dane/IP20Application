using FinalProjectApplication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FinalYearProjectApplicatonTests
{
    [TestClass]
    public class YearTests
    {
        [DataTestMethod]
        [DataRow(2020, 1, 5)]
        [DataRow(2021, 1, 3)]
        [DataRow(2022, 1, 2)]
        public void GetFirstSundayTest(int yearNo, int month, int day)
        {
            Year year = new Year(yearNo);
            DateTime expectedFirstSunday = new DateTime(yearNo, month, day);

            DateTime firstSunday = year.GetFirstSunday;

            Assert.AreEqual(expectedFirstSunday, firstSunday);
        }

        [DataTestMethod]
        [DataRow(2020, 19, 5, 12)]
        [DataRow(2020, 1, 1, 5)]
        [DataRow(2021, 52, 1, 2)]
        public void GetWeekNumberTest(int yearNo, int weekNoInput, int month, int day)
        {
            Year year = new Year(yearNo);
            int expectedWeekNumber = weekNoInput;

            int weekNoOutput = year.GetWeekNumber(new DateTime(yearNo, month, day));

            Assert.AreEqual(expectedWeekNumber, weekNoOutput);
        }

        [TestMethod]
        public void GetStartFinishWeekTest()
        {
            Year year = new Year(2020);
            DateTime expectedStartWeekDay = new DateTime(2020, 1, 26);
            DateTime expectedFinishWeekDay = new DateTime(2020, 2, 1);
            List<DateTime> expectedStartFinishWeek = new List<DateTime>() {expectedStartWeekDay, expectedFinishWeekDay };

            List<DateTime> actualStartFinishWeek = year.GetWeekStartFinish(4);

            CollectionAssert.AreEqual(expectedStartFinishWeek, actualStartFinishWeek);
        }

        [DataTestMethod]
        [DataRow(2020)]
        [DataRow(2021)]
        [DataRow(2019)]
        public void GetYearTest(int yearNo)
        {
            Year year = new Year(yearNo);
            int expectedYear = yearNo;

            int actualYear = year.YearNumber;

            Assert.AreEqual(expectedYear, actualYear);
        }
    }
}
