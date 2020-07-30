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
    public class ManagerTest
    {
        [TestMethod]
        public void GetEmployeePayrollTest()
        {
            Mock<IPayrollObserver> payrollMock = new Mock<IPayrollObserver>();

            Manager manager = new Manager("1234", "4", GetEmployees(), null);

            manager.AddPayrollObserver(payrollMock.Object);

            manager.GetPayrollWeek();

            List<PayrollData> expectedPayroll = GetEmployeesPayroll();
            List<PayrollData> actualPayroll = manager.PayrollWeek;

            Assert.AreEqual(expectedPayroll.Count, actualPayroll.Count);

            for (int i=0; i<actualPayroll.Count; i++)
            {
                Assert.AreEqual(expectedPayroll[i].EmployeeName, actualPayroll[i].EmployeeName);
                Assert.AreEqual(expectedPayroll[i].HoursRota, actualPayroll[i].HoursRota);
                Assert.AreEqual(expectedPayroll[i].HoursWorked, actualPayroll[i].HoursWorked);
                Assert.AreEqual(expectedPayroll[i].HolidayHours, actualPayroll[i].HolidayHours);
                Assert.AreEqual(expectedPayroll[i].ContractHours, actualPayroll[i].ContractHours);
                Assert.AreEqual(expectedPayroll[i].ErrorMessage, actualPayroll[i].ErrorMessage);
            }

            payrollMock.Verify(x => x.UpdatePayroll(It.IsAny<List<PayrollData>>()), Times.Exactly(1));
        }

        [TestMethod]
        public void GetEmployeesDataTest()
        {
            Manager manager = new Manager("1234", "4", GetEmployees(), null);
            Mock<IWorkHoursObserver> workHoursObserver = new Mock<IWorkHoursObserver>();
            manager.AddWorkHoursObserver(workHoursObserver.Object);
            manager.GetEmployeesData(0, 10, new Year(2020));

            List<EmployeeWorkWeek> expected = GetWorkHours("1234", new DateTime(2020, 3, 25));
            List<EmployeeWorkWeek> actual = manager.EmployeeWorkWeek;

            Assert.AreEqual(expected.Count, actual.Count);

            for (int i=0; i<expected.Count; i++)
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

        [TestMethod]
        public void UpdateEmployeeRotaTest()
        {
            Manager manager = new Manager("1234", "4", GetEmployees(), null);
            Mock<IWorkHoursObserver> workHoursObserver = new Mock<IWorkHoursObserver>();
            manager.GetEmployeesData(0, 10, new Year(2020));
            manager.AddWorkHoursObserver(workHoursObserver.Object);
            manager.UpdateEmployeeRota(GetWorkHours("1234", new DateTime(2020, 3, 20)));

            workHoursObserver.Verify(x => x.WorkHoursUpdated(), Times.Once());
        }

        [TestMethod]
        public void UpdateEmployeeRotaTestFails()
        {
            Manager manager = new Manager("1234", "4", GetEmployeesFailWorkHours(), null);
            Mock<IWorkHoursObserver> workHoursObserver = new Mock<IWorkHoursObserver>();
            manager.GetEmployeesData(0, 20, new Year(2020));
            manager.AddWorkHoursObserver(workHoursObserver.Object);
            manager.UpdateEmployeeRota(GetWorkHoursFailNoRotaFinishTime("1234", new DateTime(2020, 3, 20)));

            List<EmployeeWorkWeek> expected = GetWorkHoursFailNoRotaFinishTime("1234", new DateTime(2020, 3, 20));

            workHoursObserver.Verify(x => x.WorkHoursError("The rota could not be updated due to: \n"
                + string.Format("Only rota start time or rota finish time enetered on date: {0} \n", expected[0].Date.ToShortDateString())
                + "Please select the work week again"), Times.Once());

            manager.UpdateEmployeeRota(GetWorkHoursFailNoRotaFinishTime("1234", new DateTime(2020, 3, 20)));
            expected = GetWorkHoursFailNoRotaStartTime("1234", new DateTime(2020, 3, 20));

            workHoursObserver.Verify(x => x.WorkHoursError("The rota could not be updated due to: \n"
                + string.Format("Only rota start time or rota finish time enetered on date: {0} \n", expected[0].Date.ToShortDateString())
                + "Please select the work week again"), Times.Exactly(2));

            manager.UpdateEmployeeRota(GetWorkHoursFailStartTimeAfterEndTime("1234", new DateTime(2020, 3, 20)));
            expected = GetWorkHoursFailStartTimeAfterEndTime("1234", new DateTime(2020, 3, 20));

            workHoursObserver.Verify(x => x.WorkHoursError("The rota could not be updated due to: \n"
                + string.Format("The rota start time is after the rota finish time on date: {0} \n", expected[0].Date.ToShortDateString())
                + "Please select the work week again"), Times.Once());

            manager.UpdateEmployeeRota(GetWorkHoursFailHolidayBooked("1234", new DateTime(2020, 4, 30)));
            expected = GetWorkHoursFailHolidayBooked("1234", new DateTime(2020, 4, 30));

            workHoursObserver.Verify(x => x.WorkHoursError("The rota could not be updated due to: \n"
                + string.Format("Holiday booked on date: {0} \n", expected[0].Date.ToShortDateString())
                + "Please select the work week again"), Times.Once());

            manager.UpdateEmployeeRota(GetWorkHoursFine("1234", new DateTime(2020, 4, 20)));
            expected = GetWorkHoursFine("1234", new DateTime(2020, 4, 20));

            workHoursObserver.Verify(x => x.WorkHoursError("The rota could not be updated due to: \n"
                + string.Format("The employee is scheduled to work on date: {0} \n", expected[0].Date.ToShortDateString())
                + "Please select the work week again"), Times.Once());

            manager.UpdateEmployeeRota(GetWorkHoursInvalidFormat("1234", new DateTime(2020, 4, 20)));
            expected = GetWorkHoursInvalidFormat("1234", new DateTime(2020, 4, 20));

            workHoursObserver.Verify(x => x.WorkHoursError("The rota could not be updated due to: \n"
                + string.Format("Valid time not entered on date: {0} \n", expected[0].Date.ToShortDateString())
                + "Please select the work week again"), Times.Once());
        }

        [TestMethod]
        public void SetEmployeeHolidayTest()
        {
            Manager manager = new Manager("1234", "4", GetEmployeesHolidayTest(), null);

            manager.SetEmployeeHolidays();

            List<EmployeeHoliday> expectedResult = new List<EmployeeHoliday>();
            expectedResult.AddRange(GetHolidays());
            expectedResult.AddRange(GetHolidays2());

            List<EmployeeHoliday> actualResult = manager.EmployeeHolidays;

            Assert.AreEqual(expectedResult.Count, actualResult.Count);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i].Date, actualResult[i].Date);
                Assert.AreEqual(expectedResult[i].EmployeeName, actualResult[i].EmployeeName);
                Assert.AreEqual(expectedResult[i].Id, actualResult[i].Id);
                Assert.AreEqual(expectedResult[i].State, actualResult[i].State);
            }
        }

        private List<Employee> GetEmployeesHolidayTest()
        {
            Mock<IEmployeeDatabase> employeeDatabase = new Mock<IEmployeeDatabase>();

            employeeDatabase.SetupSequence(x => x.GetHolidays(It.IsAny<int>()))
                .Returns(GetHolidays())
                .Returns(GetHolidays2())
                .Returns(GetHolidays())
                .Returns(GetHolidays2());

            List<Employee> employees = new List<Employee>
            {
                new Employee("Jack", "Dane", 4, 3, "1234", "Jackd98.jd@gmail.com", employeeDatabase.Object),
                new Employee("Penny", "Smith", 40, 4, "1234", "PennySmith@gmail.com", employeeDatabase.Object),
            };
            return employees;
        }

        private List<Employee> GetEmployees()
        {
            Mock<IEmployeeDatabase> employeeDatabase = new Mock<IEmployeeDatabase>();

            employeeDatabase.SetupSequence(x => x.GetEmployeePayroll(It.IsAny<int>(), It.IsAny<string>(), 
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(GetEmployeesPayroll()[0])
                .Returns(GetEmployeesPayroll()[1]);

            employeeDatabase.Setup(x => x.GetHolidays(It.IsAny<int>()))
                .Returns(new List<EmployeeHoliday>());

            employeeDatabase.Setup(x => x.GetEmployeeWorkWeek(It.IsAny<List<EmployeeWorkWeek>>(), It.IsAny<int>(), 
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(GetWorkHours("1234", new DateTime(2020, 3, 25)));

            List <Employee> employees = new List<Employee>
            {
                new Employee("Jack", "Dane", 4, 3, "1234", "Jackd98.jd@gmail.com", employeeDatabase.Object),
                new Employee("Penny", "Smith", 40, 4, "1234", "PennySmith@gmail.com", employeeDatabase.Object),
            };
            return employees;
        }

        private List<Employee> GetEmployeesFailWorkHours()
        {
            Mock<IEmployeeDatabase> employeeDatabase = new Mock<IEmployeeDatabase>();

            employeeDatabase.SetupSequence(x => x.GetEmployeePayroll(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), 
                It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(GetEmployeesPayroll()[0])
                .Returns(GetEmployeesPayroll()[1]);

            employeeDatabase.Setup(x => x.GetHolidays(It.IsAny<int>()))
                .Returns(GetHolidays());

            employeeDatabase.SetupSequence(x => x.GetEmployeeWorkWeek(It.IsAny<List<EmployeeWorkWeek>>(), It.IsAny<int>(), 
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(GetWorkHoursFine("1234", new DateTime(2020, 3, 20)))
                .Returns(GetWorkHoursFailNoRotaFinishTime("1234", new DateTime(2020, 3, 20)))
                .Returns(GetWorkHoursFailNoRotaStartTime("1234", new DateTime(2020, 3, 20)))
                .Returns(GetWorkHoursFailStartTimeAfterEndTime("1234", new DateTime(2020, 3, 20)))
                .Returns(GetWorkHoursFailHolidayBooked("1234", new DateTime(2020, 4, 30)))
                .Returns(GetWorkHoursFailWorkingOtherStore("1234", new DateTime(2020, 4, 20)))
                .Returns(GetWorkHoursFine("1234", new DateTime(2020, 3, 20)));

            List<Employee> employees = new List<Employee>
            {
                new Employee("Jack", "Dane", 4, 3, "1234", "Jackd98.jd@gmail.com", employeeDatabase.Object),
                new Employee("Penny", "Smith", 40, 4, "1234", "PennySmith@gmail.com", employeeDatabase.Object),
            };
            return employees;
        }

        private List<PayrollData> GetEmployeesPayroll()
        {
            List<PayrollData> employeePayroll = new List<PayrollData>()
            {
                new PayrollData
                {
                    EmployeeName = "Jack Dane",
                    HoursRota = 4.4,
                    HoursWorked = 5,
                    HolidayHours = 0,
                    ContractHours = 4,
                    ErrorMessage = ""
                },
                new PayrollData
                {
                    EmployeeName = "Penny Smith",
                    HoursRota = 40,
                    HoursWorked = 32,
                    HolidayHours = 8,
                    ContractHours = 40,
                    ErrorMessage = ""
                }
            };
            return employeePayroll;
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

        private List<EmployeeWorkWeek> GetWorkHoursFailNoRotaFinishTime(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "12:00:00",RotaFinishTime = "00:00:00",LunchTime = "00:00:00"
                }
            };
            return employeesRota;
        }

        private List<EmployeeWorkWeek> GetWorkHoursFailNoRotaStartTime(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "00:00:00",RotaFinishTime = "12:00:00",LunchTime = "00:00:00"
                }
            };
            return employeesRota;
        }

        private List<EmployeeWorkWeek> GetWorkHoursFailStartTimeAfterEndTime(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "14:00:00",RotaFinishTime = "12:00:00",LunchTime = "00:00:00"
                }
            };
            return employeesRota;
        }

        private List<EmployeeWorkWeek> GetWorkHoursFailHolidayBooked(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "09:00:00",RotaFinishTime = "12:00:00",LunchTime = "00:00:00"
                }
            };
            return employeesRota;
        }

        private List<EmployeeWorkWeek> GetWorkHoursFailWorkingOtherStore(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = "4321",Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "14:00:00",RotaFinishTime = "12:00:00",LunchTime = "00:00:00"
                }
            };
            return employeesRota;
        }

        private List<EmployeeWorkWeek> GetWorkHoursFine(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "12:00:00",RotaFinishTime = "16:00:00",LunchTime = "00:00:00"
                }
            };
            return employeesRota;
        }

        private List<EmployeeWorkWeek> GetWorkHoursInvalidFormat(string storeId, DateTime startDate)
        {
            List<EmployeeWorkWeek> employeesRota = new List<EmployeeWorkWeek>
            {
                new EmployeeWorkWeek
                {
                    StoreId = storeId,Date = startDate,Day = startDate.DayOfWeek.ToString(),StartTime = "00:00:00",
                    FinishTime = "00:00:00",RotaStartTime = "aa:00:00",RotaFinishTime = "16:00:00",LunchTime = "00:00:00"
                }
            };
            return employeesRota;
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
                    Date = new DateTime(2020,4,26)
                },
            };

            return holidays;
        }

        private List<EmployeeHoliday> GetHolidays2()
        {
            List<EmployeeHoliday> holidays = new List<EmployeeHoliday>
            {
                new EmployeeHoliday
                {
                    Id = 3,
                    EmployeeName = "Penny Smith",
                    State = "Approved",
                    Date = new DateTime(2020,3,10)
                },
            };

            return holidays;
        }
    }
}
