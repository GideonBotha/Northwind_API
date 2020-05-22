using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using System.Data.Entity;
using System.IO;
using System.Web.Hosting;
using System.Net.Http.Headers;
using Northwind_API.Models;
using System.Dynamic;
using System.Data.Entity.Infrastructure;

namespace Northwind_API.Models
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class NorthwindController : ApiController
    {
        [System.Web.Mvc.Route("api/Northwind/getReportData/{country}")]
        [System.Web.Mvc.HttpPost]
        public dynamic getReportData(string country)
        {
            Northwind2Entities db = new Northwind2Entities();
            db.Configuration.ProxyCreationEnabled = false;
            List<Order> orders;

            if(country != "ALL")
            {
                orders = db.Orders.Include(r => r.Employee).Include(a => a.Customer).Where(rr => rr.Employee.Country == country).ToList();
            }
            else
            {
                orders = db.Orders.Include(u => u.Employee).Include(u => u.Customer).ToList();
            }

            return GetExpandoReport(orders);
        }


        private dynamic GetExpandoReport(List<Order> orders)
        {
            //CHART
            dynamic outObject = new ExpandoObject();
            var employeelist = orders.GroupBy(gg => gg.Employee.Country);
            
            List<dynamic> cntr = new List<dynamic>();

            foreach(var group in employeelist)
            {
                dynamic Countries = new ExpandoObject();
                Countries.Name = group.Key;
                Countries.TotalOrders = group.Sum(gg => gg.EmployeeID);
                cntr.Add(Countries);
            }
            outObject.ChartData = cntr;

            //CHART


            //TABLE
            var orderlist = orders.GroupBy(gg => gg.Employee.FirstName);
            List<dynamic> ords = new List<dynamic>();

            foreach (var group in orderlist)
            {
                dynamic Employees = new ExpandoObject();
                Employees.Key = group.Key;
                Employees.TotalOrders = group.Sum(gg => gg.EmployeeID);


                List<dynamic> employeeords = new List<dynamic>();
                foreach (var order in group)
                {
                    dynamic orderObject = new ExpandoObject();
                    orderObject.Name = order.Customer.CompanyName;
                    orderObject.ShipAddress = order.ShipAddress;
                    employeeords.Add(orderObject);
                }
                Employees.EmployeeOrders = employeeords;
                ords.Add(Employees);
            }
            outObject.TableData = ords;

            //TABLE
            return outObject;
        }
    }
}
