using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProjectTerminal.Commands
{
    public static class project
    {
        public static string id_token = "";
        public static string access_token = "";
        public static string projectsessionapiurl = "";
        public static string projectstate = "";


        // Open browser and login to project for the web
        public static string login()
        {
            bool id_found = false;
            int change_count = 0;
            
            var options = new LaunchOptions
            {
                Headless = false
            };

            options.DefaultViewport = null;

            // Launch the browser to gather login credentials
            using (var browser = Puppeteer.LaunchAsync(options).Result)
            {
                var page = (browser.PagesAsync().Result)[0]; 

                // Event handler for page changes - we are looking for the return token for the user
                void evt_ChangeHandler(object sender, TargetChangedArgs e)
                {
                    Uri u = new Uri(e.Target.Url);
                    id_token = HttpUtility.ParseQueryString(u.Fragment).Get("#id_token");


                    if ((u.Host == "project.microsoft.com") && (id_token != null))
                        id_found = true;

                    // count the number of times the event is fired
                    change_count++;
                }

                browser.TargetChanged += evt_ChangeHandler;


                _ = page.GoToAsync("http://project.microsoft.com").Result;

                // wait for the user to enter credentials or return once the user exceeds the number of page changes
                while ((!id_found) && (change_count < 12))
                    Task.Delay(100);

                browser.CloseAsync();
            }
            if (id_found)
                return Properties.strings.project_login_Success;
            else
                return Properties.strings.project_login_TokenNotFound;
        }

        public static string open(string projectid) //"c50e08c8-10ae-48fb-86c0-2530bbc8183e"
        {

            //id_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkN0VHVoTUptRDVNN0RMZHpEMnYyeDNRS1NSWSJ9.eyJhdWQiOiI4MTk4ZTdlZS0wMDc0LTQxZDYtYjA4Ni0wYWUxNmYyMThhYzgiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vYzA5NmIxY2ItZTdkOS00NDhiLWJmODktMGM2NDZmZmJiNWRjL3YyLjAiLCJpYXQiOjE1ODc5MjMwMzcsIm5iZiI6MTU4NzkyMzAzNywiZXhwIjoxNTg3OTI2OTM3LCJuYW1lIjoiQWRtaW4iLCJvaWQiOiI2OGQ4OGNlYy1iMmRiLTRmMTAtYmQ1Ni1lMTgxZjFkNmJlZmUiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJhZG1pbkBwcm9qZWN0LW9wcy5jb20iLCJzdWIiOiJBMFBXdGFYWFh2a0tFdkxPTXZ4Q3FNTXgzUFpHMkRBZTQzY3RnaXJZTHhZIiwidGlkIjoiYzA5NmIxY2ItZTdkOS00NDhiLWJmODktMGM2NDZmZmJiNWRjIiwidXRpIjoiRnNwdTFoeHJZRVczdmJHSWtxZFhBQSIsInZlciI6IjIuMCJ9.sP0TDxd99AEEjLUH0kHZmqSmk0QmLCWg83uGZaO2L1m813rBRMI6AJbUZBPehPyB7-ShP-mQEytiV_49YUveGlftqj5fVrv8v1Oxgpibq1mt89UxJJW736d9UBm-c17lhObT5mQ_MUdhAsQoymeNXNZ_TodHctfDSfRbwz70oBv2GjVN8KuoOu8UGEPdN4sd7itFUIJGLDQR8xffKtbf4s3NFvVChUmu_jxvnJvilkeBf_LteOgtk-ThqYlMVUR78pqORclfnxvaQdqUaddIrCfVNL7nkJ4M0k6tDN-6E2hUYr7Onu7zdZTm34ZNLYQFdDGoEjcPghtTC7ELjuFb9Q";

            if (id_token.Length > 0)
            {

                // setup Http client for queries
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + id_token);
                HttpResponseMessage response;

                string projurl = "";
                StringContent content;
                string json = "";

                // Get default org
                projurl = "https://project.microsoft.com/default.crm.dynamics.com/api/v1/GetModProjCdsEndpoint";
                response = client.GetAsync(projurl).Result;
                var getmodprojcdsendpoint_reponse = new { cdsUrl = "", cdsProvisioningState = "", packageInstallState = "", installedPackageVersion = "", latestPackageVersion = "", isLatestPackageInstalled = false, cdsDiscoveryTimeInMilliseconds = 0, packageDiscoveryTimeInMilliseconds = 0, canSendEmailToMe = false };
                var getmodprojcdsendpoint = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, getmodprojcdsendpoint_reponse);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(getmodprojcdsendpoint.cdsUrl);


                // Get org info
                projurl = "https://project.microsoft.com/pss/api/v1.0/xrm/GetOrgInfo";
                json = JsonConvert.SerializeObject(new { xrmUrl = getmodprojcdsendpoint.cdsUrl });
                content = new StringContent(json, Encoding.UTF8, "application/json");
                response = client.PostAsync(projurl, content).Result;

                var getorginfo_response = new { orgProjectType = "" };
                var getorginfo = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, getorginfo_response);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(getorginfo.orgProjectType);



                // Open project and retrieve updated id_token
                projurl = "https://project.microsoft.com/pss/api/v1.0/xrm/openproject";
                json = JsonConvert.SerializeObject(new { xrmUrl = getmodprojcdsendpoint.cdsUrl, xrmProjectId = projectid });
                content = new StringContent(json, Encoding.UTF8, "application/json");
                response = client.PostAsync(projurl, content).Result;

                var openproject_response = new { accessToken = "", forwarded = false, projectState = "", projectSessionApiUrl = "", projectNotificationUrl = "", routeKey = "", timezoneName = "", timezoneOffset = "" };
                var openproject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, openproject_response);
                response.EnsureSuccessStatusCode();

                // Update global varibles with project details
                access_token = openproject.accessToken;
                projectsessionapiurl = openproject.projectSessionApiUrl;
                projectstate = openproject.projectState;

                return Properties.strings.project_open_ProjectOpen;
            }
            else
            {
                return Properties.strings.project_open_ErrLoginBeforeOpen;
            }
        }

        public static string addtask(string name, int outlinelevel)
        {
            if ((access_token != "") && (projectsessionapiurl != ""))
            {
                // setup Http client for queries
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);
                HttpResponseMessage response;

                string projurl = "";
                StringContent content;
                string json = "";

                // Create a task
                projurl = projectsessionapiurl + "/tasks";
                json = JsonConvert.SerializeObject(new { name = name, outlineLevel = outlinelevel });
                content = new StringContent(json, Encoding.UTF8, "application/json");
                response = client.PostAsync(projurl, content).Result;

                var addtask_response = new {id="", created=false};
                var addtask = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, addtask_response);
                response.EnsureSuccessStatusCode();

                return Properties.strings.project_task_TaskAdded;
            }
            else
            {
                return Properties.strings.project_task_ErrOpenBeforeAdd;
            }
        }

    }
}
