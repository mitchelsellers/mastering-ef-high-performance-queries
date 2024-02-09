using System.Diagnostics;
using MasteringEfDemo.Models;
using MasteringEfDemo.ViewModels;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MasteringEfDemo
{
    public interface IDemoEngine
    {
        /// <summary>
        /// This demo showcases the performance change/benefit of disabling change tracking
        /// </summary>
        void DemoChangeTracking();
        /// <summary>
        /// Demos what can happen if you execute a query and have related entities 
        /// </summary>
        void DemoMultiQueryExecution();
        /// <summary>
        /// Demos the benefits of using projections for queries to improve performance
        /// </summary>
        void DemoProjections();
        /// <summary>
        /// Showcases what can happen if you include too much
        /// </summary>
        void DemoOverInclusion();
        /// <summary>
        /// Demos te use of paging with EF
        /// </summary>
        void DemoPagedResults();

        /// <summary>
        /// Demonstrates the process of query splitting
        /// </summary>
        void DemoSplitQuery();

        /// <summary>
        /// Demonstration of advanced query formatting
        /// </summary>
        void DemoStringAggregation();

        /// <summary>
        /// Showcases the usage of functions with dates
        /// </summary>
        void DemoDateFunctions();

        /// <summary>
        /// Example of a bulk update with a filter
        /// </summary>
        void DemoBulkUpdate();
    }
    public class DemoEngine(AdventureWorks2022Context context) : IDemoEngine
    {
        public void DemoChangeTracking()
        {
            //Non-projections
            var actionTimer = new Stopwatch();
            actionTimer.Start();
            var items = context.Customers
                .Include(c => c.Store)
                .Include(c => c.Person)
                .ToList();
            actionTimer.Stop();
            var elapsed1 = actionTimer.Elapsed;
            actionTimer.Reset();
            actionTimer.Start();
            var items2 = context.Customers
                .AsNoTracking()
                .Include(c => c.Store)
                .Include(c => c.Person)
                .ToList();
            actionTimer.Stop();
            var elapsed2 = actionTimer.Elapsed;

            Log.Information("Query with Tracking Execution {elapsed}", elapsed1);
            Log.Information("Query with Tracking Count {Count}", items.Count());
            Log.Information("Query W/o Tracking Execution {Elapsed}", elapsed2);
            Log.Information("Query W/o Tracking Count {Count}", items2.Count());
        }

        public void DemoMultiQueryExecution()
        {
            var query = context.Customers
                .AsNoTracking()
                .Include(c => c.Person)
                .ToList();

            //Naughty!
            query = query.Where(c => c.Store.Name == "Riders Company").ToList();
        }

        public void DemoProjections()
        {
            //Non-projections
            var actionTimer = new Stopwatch();
            actionTimer.Start();
            var items = context.Customers
                .Include(c => c.Store)
                .Include(c => c.Person)
                .ToList();
            actionTimer.Stop();
            var elapsed1 = actionTimer.Elapsed;
            actionTimer.Reset();
            actionTimer.Start();
            var customers = context.Customers
                .AsNoTracking()
                .Select(c => new ListCustomerViewModel
                {
                    AccountNumber = c.AccountNumber,
                    ContactFirstName = c.Person.FirstName,
                    ContactLastName = c.Person.LastName,
                    ContactTitle = c.Person.Title,
                    CustomerId = c.CustomerId,
                    StoreName = c.Store.Name
                })
                .ToList();
            actionTimer.Stop();
            var elapsed2 = actionTimer.Elapsed;

            Log.Information("Full Query Execution {elapsed}", elapsed1);
            Log.Information("Full items Count {Count}", items.Count());
            Log.Information("Projection Query Execution {Elapsed}", elapsed2);
            Log.Information("Projection Count {Count}", customers.Count());
        }

        public void DemoOverInclusion()
        {
            //Proper way
            var toUpdate = context.People
                .First(p => p.BusinessEntityId == 1);
            toUpdate.FirstName = "John";
            context.SaveChanges();

            //bad Way
            var secondUpdate = context.People
                .Include(p => p.Customers)
                .First(p => p.BusinessEntityId == 1);
            secondUpdate.FirstName = "Ken";
            context.SaveChanges();
        }

        public void DemoPagedResults()
        {
            var customers = context.Customers
                .AsNoTracking()
                .Where(c => c.TerritoryId != null);

            //Get a count now
            var totalRecords = customers.Count();

            //Now setup for paged results
            var secondPageOf50 = customers
                .TagWith("Paged Query")
                .TagWithCallSite()
                .OrderBy(c => c.Person.FirstName)
                .ThenBy(c => c.Person.LastName)
                .Skip(50)
                .Take(50)
                .ToList();
        }

        public void DemoSplitQuery()
        {//Non-projections
            var actionTimer = new Stopwatch();
            actionTimer.Start();
            var items = context.People
                .AsNoTracking()
                .Include(p => p.EmailAddresses)
                .Include(p => p.BusinessEntityContacts)
                .Include(p => p.Customers)
                .ToList();
            actionTimer.Stop();
            var elapsed1 = actionTimer.Elapsed;
            actionTimer.Reset();
            actionTimer.Start();
            var customers = context.People
                .AsNoTracking()
                .Include(p => p.EmailAddresses)
                .Include(p => p.BusinessEntityContacts)
                .Include(p => p.Customers)
                .AsSplitQuery()
                .ToList();
            actionTimer.Stop();
            var elapsed2 = actionTimer.Elapsed;

            Log.Information("Without Split Execution {elapsed}", elapsed1);
            Log.Information("Without Split Count {Count}", items.Count());
            Log.Information("With Split Execution {Elapsed}", elapsed2);
            Log.Information("With Split Count {Count}", customers.Count());
        }

        public void DemoStringAggregation()
        {
            var personList = context.Customers
                .AsNoTracking()
                .Where(c => c.Person != null)
                .GroupBy(g => g.Person.FirstName)
                .Select(s => new
                {
                    s.Key,
                    Accounts = string.Join(", ", s.Select(customer => customer.AccountNumber))
                })
                .ToList();

            Log.Information("Total Records {Count}", personList.Count());
            foreach (var item in personList.Take(15))
            {
                Log.Information("{Name} Accounts: {Accounts}", item.Key, item.Accounts);
            }
        }

        public void DemoDateFunctions()
        {
            //Old Way
            var minDate = DateTime.UtcNow.AddYears(-1);
            var clients = context.Customers
                .AsNoTracking()
                .Where(c => c.ModifiedDate >= minDate)
                .ToList();

            //New Way
            var clients2 = context.Customers
                .AsNoTracking()
                .Where(c => c.ModifiedDate > DateTime.Now.AddYears(-1))
                .ToList();

            //Fun with functions
            var functionExample = context.Customers
                .AsNoTracking()
                .Select(c => new
                {
                    c.CustomerId,
                    ModifiedDaysAgo = EF.Functions.DateDiffDay(c.ModifiedDate, DateTime.Now),
                    c.ModifiedDate,
                    ModifiedCentralTime = EF.Functions.AtTimeZone(c.ModifiedDate, "Central Standard Time")
                })
                .Take(15);
            foreach (var item in functionExample)
            {
                Log.Information("{id} - Modified {days} ago - {Stored} - {Central} CST", item.CustomerId, item.ModifiedDaysAgo, item.ModifiedDate, item.ModifiedCentralTime);
            }
        }

        public void DemoBulkUpdate()
        {
            context.Customers
                .Where(c => c.ModifiedDate < DateTime.UtcNow.AddYears(4))
                .ExecuteUpdate(c => c
                    .SetProperty(u => u.ModifiedDate, u => u.ModifiedDate.AddDays(5)));
        }
    }
}
