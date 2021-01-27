using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using ProjectTerminal.Models;

namespace ProjectTerminal.Commands
{
    public static class cds
    {
        private static CrmServiceClient targetOrg;
        private static IOrganizationService targetOrgSvc;


        public static string login(string orgUrl, string userName, string password)
        {
            string conn = $@"Url = {orgUrl};AuthType = Office365;UserName = {userName};Password = {password};RequireNewInstance = True";

            targetOrg = new CrmServiceClient(conn);

            if (!targetOrg.IsReady)
            {
                return $"Error connecting to {targetOrg.ConnectedOrgFriendlyName}";
            }
            else
            {
                targetOrgSvc = (IOrganizationService)targetOrg.OrganizationWebProxyClient != null ? (IOrganizationService)targetOrg.OrganizationWebProxyClient : (IOrganizationService)targetOrg.OrganizationServiceProxy;

                RetrieveVersionRequest versionRequest = new RetrieveVersionRequest();
                RetrieveVersionResponse versionResponse = (RetrieveVersionResponse)targetOrgSvc.Execute(versionRequest);

                return $"Connected to {targetOrg.ConnectedOrgFriendlyName} version {versionResponse.Version}";
            }
        }


        public static string listcalendars()
        {
            QueryExpression q = new QueryExpression("calendar")
            {
                ColumnSet = new ColumnSet(true),
                NoLock = true
            };

            //q.Criteria.AddCondition("type", ConditionOperator.Equal, 0);
            //q.Criteria.AddCondition("name", ConditionOperator.Equal, "Holidays");

            EntityCollection businessClosureCalendars = targetOrgSvc.RetrieveMultiple(q);

            foreach (Entity e in businessClosureCalendars.Entities)
            {
                string s = "";
                int ruleCount = 0;
                string calType = "";


                // determine the type of calendar
                if (e.Contains("type"))
                {
                    OptionSetValue o = (OptionSetValue)e["type"];
                    switch (o.Value)
                    {
                        case 0: 
                            calType = "Default";
                            break;
                        case 1: calType = "Customer Service";
                            break;
                        case 2: calType = "Holiday Schedule";
                            break;
                        case -1: calType = "Inner Calendar";
                            break;
                        default: calType = "Unknown";
                            break;
                    }
                }

                if (e.Contains("name")) { s += $"{e["name"]} "; }
                if (e.Contains("description")) { s += $"({e["description"]}) "; }
                if (e.Contains("type")) { s += $"type:{calType} "; }
                s += $"id:{e.Id} ";


                EntityCollection f = (EntityCollection)e.Attributes["calendarrules"];
                ruleCount = f.Entities.Count;

                Console.WriteLine($"{s}rules:{ruleCount}");

                foreach (Entity rule in f.Entities)
                {
                    s = "";
                    if (rule.Attributes.Contains("starttime")) { s += $"s:{rule.Attributes["starttime"]} "; }
                    if (rule.Attributes.Contains("duration")) { s += $"d:{rule.Attributes["duration"]} "; }
                    if (rule.Attributes.Contains("pattern")) { s += $"{rule.Attributes["pattern"]} "; }


                    Console.WriteLine($"  {rule.Id} {s}");
                }

                Console.WriteLine();
            }

            return "";
        }

        public static string savecalendarasical(string id)
        {
            Entity cal = targetOrgSvc.Retrieve("calendar", Guid.Parse(id), new ColumnSet(true));

            return $"{cal["name"]}";

        }

        public static string get()
        {
            return "";

        }
    }
}
