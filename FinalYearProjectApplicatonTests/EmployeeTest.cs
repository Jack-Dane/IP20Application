using FinalProjectApplication;
using Autofac.Extras.Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Moq;

namespace FinalYearProjectApplicatonTests
{
    [TestClass]
    public class EmployeeTest
    {

        [TestMethod]
        public void EmployeeRefreshHolidayTest()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IEmployeeDatabase>()
                    .Setup(x => x.GetHolidays(3))
                    .Returns(GetHolidays());

                var employee = mock.Create<Employee>(
                    new NamedParameter("firstName", "Jack"),
                    new NamedParameter("lastName", "Dane"),
                    new NamedParameter("contractHours", 4),
                    new NamedParameter("id", 3),
                    new NamedParameter("storeId", "1234"),
                    new NamedParameter("email", "jackd98.jd@gmail.com")
                    );

                var expected = GetHolidays();

                var actual = employee.EmployeeHoliday;

                Assert.AreEqual(expected.Count, actual.Count);
            }
        }

        [TestMethod]
        public void GetHoursTest()
        {
            using (var mock = AutoMock.GetLoose())
            {
                string firstName = "Jack";
                string lastName = "Dane";
                int contractHours = 4;
                int employeeId = 3;
                string storeId = "1234";
                string email = "jackd98.jd@gmail.com";

                Year year = new Year(2020);
                int weekNo = 17;
                List<DateTime> startFinishWorkDays = year.GetWeekStartFinish(weekNo);
                DateTime startWeek = startFinishWorkDays[0];
                DateTime finishWeek = startFinishWorkDays[1];

                mock.Mock<IEmployeeDatabase>()
                    .Setup(x => x.GetEmployeeWorkWeek(It.IsAny<List<EmployeeWorkWeek>>(), employeeId, 
                    storeId, startWeek, finishWeek))
                    .Returns(GetWorkHours(storeId, startWeek));

                mock.Mock<IEmployeeDatabase>()
                    .Setup(y => y.GetHolidays(employeeId))
                    .Returns(GetHolidays());


                var employee = mock.Create<Employee>(
                    new NamedParameter("firstName", firstName),
                    new NamedParameter("lastName", lastName),
                    new NamedParameter("contractHours", contractHours),
                    new NamedParameter("id", employeeId),
                    new NamedParameter("storeId", storeId),
                    new NamedParameter("email", email)
                    );

                List<EmployeeWorkWeek> expected = GetExpectedWorkHours(storeId, startWeek);

                List<EmployeeWorkWeek> actual = employee.GetHours(weekNo, year);

                Assert.AreEqual(expected.Count, actual.Count);

                for(int i=0; i< expected.Count; i++)
                {
                    Assert.AreEqual(expected[i].Date, actual[i].Date);
                    Assert.AreEqual(expected[i].Day, actual[i].Day);
                    Assert.AreEqual(expected[i].FinishTime, actual[i].FinishTime);
                    Assert.AreEqual(expected[i].LunchTime, actual[i].LunchTime);
                    Assert.AreEqual(expected[i].RotaFinishTime, actual[i].RotaFinishTime);
                    Assert.AreEqual(expected[i].RotaStartTime, actual[i].RotaStartTime);
                    Assert.AreEqual(expected[i].StartTime, actual[i].StartTime);
                    Assert.AreEqual(expected[i].StoreId, actual[i].StoreId);
                }
            }
        }

        [TestMethod]
        public void GetPayrollTest()
        {
            using (var mock = AutoMock.GetLoose())
            {
                string firstName = "Jack";
                string lastName = "Dane";
                int contractHours = 4;
                int employeeId = 3;
                string storeId = "1234";
                string email = "jackd98.jd@gmail.com";

                Year year = new Year(2020);
                List<DateTime> dates = year.GetWeekStartFinish(15);
                DateTime firstDate = dates[0];
                DateTime secondDate = dates[1];

                mock.Mock<IEmployeeDatabase>()
                    .Setup(x => x.GetEmployeePayroll(employeeId, storeId, firstDate, secondDate, firstName + " " + lastName))
                    .Returns(GetPayroll());

                var employee = mock.Create<Employee>(
                    new NamedParameter("firstName", firstName),
                    new NamedParameter("lastName", lastName),
                    new NamedParameter("contractHours", contractHours),
                    new NamedParameter("id", employeeId),
                    new NamedParameter("storeId", storeId),
                    new NamedParameter("email", email)
                    );

                PayrollData actual = employee.GetEmployeePayroll(15, year);

                PayrollData expected = ExpectedGetPayroll();

                Assert.AreEqual(actual.EmployeeName, expected.EmployeeName);
                Assert.AreEqual(actual.ErrorMessage, expected.ErrorMessage);
                Assert.AreEqual(actual.HolidayHours, expected.HolidayHours);
                Assert.AreEqual(actual.HoursRota, expected.HoursRota);
                Assert.AreEqual(actual.HoursWorked, expected.HoursWorked);
            }
        }

        [TestMethod]
        public void UpdateRotaTest()
        {
            using(var mock = AutoMock.GetLoose())
            {
                string firstName = "Jack";
                string lastName = "Dane";
                int contractHours = 4;
                int employeeId = 3;
                string storeId = "1234";
                string email = "jackd98.jd@gmail.com";

                Year year = new Year(2020);
                List<DateTime> dates = year.GetWeekStartFinish(15);
                DateTime firstDate = dates[0];
                DateTime secondDate = dates[1];

                var employee = mock.Create<Employee>(
                    new NamedParameter("firstName", firstName),
                    new NamedParameter("lastName", lastName),
                    new NamedParameter("contractHours", contractHours),
                    new NamedParameter("id", employeeId),
                    new NamedParameter("storeId", storeId),
                    new NamedParameter("email", email)
                    );

                foreach (EmployeeWorkWeek employeeWorkDay in GetWorkHoursRotaUpdate(storeId, firstDate))
                {
                    employee.UpdateRota(employeeWorkDay);
                }

                mock.Mock<IEmployeeDatabase>()
                    .Verify(x => x.DeleteFromWorkWeek(employeeId, It.IsAny<DateTime>()), Times.Exactly(7));

                mock.Mock<IEmployeeDatabase>()
                    .Verify(x => x.InsertWorkHours(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(5));
            }
        }

        private List<EmployeeHoliday> GetHolidays()
        {
            List<EmployeeHoliday> holidays = new List<EmployeeHoliday>
            {
                new EmployeeHoliday
                {
                    Id = 3,
                    EmployeeName = "Jack Dane",
                    State = "Approved",
                    Date = new DateTime(2020,4,30)
                },
                new EmployeeHoliday
                {
                    Id = 4,
                    EmployeeName = "Jack Dane",
                    State = "Pending",
                    Date = new DateTime(2020,4,25)
                },
                new EmployeeHoliday
                {
                    Id = 4,
                    EmployeeName = "Jack Dane",
                    State = "Denied",
                    Date = new DateTime(2020,4,25)
                }
            };

            return holidays;
        }

        private List<EmployeeWorkWeek> GetWorkHours(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",LunchTime = "00:00:00"

                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(1),Day = startDate.AddDays(1).DayOfWeek.ToString(),
                    StartTime = "12:00:00",FinishTime = "16:00:00",RotaStartTime = "12:00:00",RotaFinishTime = "16:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(2),Day = startDate.AddDays(2).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(3),Day = startDate.AddDays(3).DayOfWeek.ToString(),
                    StartTime = "12:00:00",FinishTime = "16:00:00",RotaStartTime = "12:00:00",RotaFinishTime = "16:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(4),Day = startDate.AddDays(4).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(5),Day = startDate.AddDays(5).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(6),Day = startDate.AddDays(6).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
            };

            return employeesRota;
        }

        private List<EmployeeWorkWeek> GetWorkHoursRotaUpdate(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "12:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(1),Day = startDate.AddDays(1).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "16:00:00",RotaStartTime = "12:00:00",RotaFinishTime = "16:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(2),Day = startDate.AddDays(2).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "01:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(3),Day = startDate.AddDays(3).DayOfWeek.ToString(),
                    StartTime = "12:00:00",FinishTime = "16:00:00",RotaStartTime = "12:00:00",RotaFinishTime = "16:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(4),Day = startDate.AddDays(4).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "16:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(5),Day = startDate.AddDays(5).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(6),Day = startDate.AddDays(6).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
            };

            return employeesRota;
        }

        private List<EmployeeWorkWeek> GetExpectedWorkHours(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(1),Day = startDate.AddDays(1).DayOfWeek.ToString(),
                    StartTime = "12:00:00",FinishTime = "16:00:00",RotaStartTime = "12:00:00",RotaFinishTime = "16:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(2),Day = startDate.AddDays(2).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(3),Day = startDate.AddDays(3).DayOfWeek.ToString(),
                    StartTime = "12:00:00",FinishTime = "16:00:00",RotaStartTime = "12:00:00",RotaFinishTime = "16:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = "Holiday",Date = startDate.AddDays(4),Day = startDate.AddDays(4).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(5),Day = startDate.AddDays(5).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate.AddDays(6),Day = startDate.AddDays(6).DayOfWeek.ToString(),
                    StartTime = "00:00:00",FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "00:00:00",
                    LunchTime = "00:00:00"
                },
            };

            return employeesRota;
        }

        private PayrollData GetPayroll()
        {
            PayrollData payroll = new PayrollData
            {
                HoursRota = 8.0,
                HoursWorked = 8.5,
                ErrorMessage = ""
            };
            return payroll;
        }

        private PayrollData ExpectedGetPayroll()
        {
            PayrollData payroll = new PayrollData
            {
                HoursRota = 8.0,
                HoursWorked = 8.5,
                ErrorMessage = "",
                HolidayHours = 0,
                ContractHours = 4,
                EmployeeName = "Jack Dane"
            };
            return payroll;
        }
    }
}
