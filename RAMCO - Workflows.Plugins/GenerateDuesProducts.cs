using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk.Messages;

namespace RAMCO___Workflows.Plugins
{
    public partial class GenerateDuesProducts : CodeActivity
    {
        // This is an example argument
        //[RequiredArgument]
        //[Input("Select an email to send")]
        //[ReferenceTarget("email")]
        //public InArgument<EntityReference> EmailReference { get; set; }

        [RequiredArgument]
        [Input("Application Fee to be Renewed")]
        [ReferenceTarget("cobalt_membershipapplicationfee")]
        public InArgument<EntityReference> AppFee { get; set; }

        [Input("New Dues Option")]
        [ReferenceTarget("cobalt_duesoption")]
        public InArgument<EntityReference> NewDuesOption { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            // Todo: implement your logic here
            // Create instances of workflow context and organization service to make use of their methods
            IWorkflowContext execontext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(execontext.UserId);
            ITracingService tracingService = context.GetExtension<ITracingService>();

                    tracingService.Trace("Begin Workflow Execution");
            //Build Query for Services related to App Fee

            string fetchStart = @"
                        <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                          <entity name='ramco_service'>
                            <attribute name='ramco_serviceid' />
                            <attribute name='ramco_name' />
                            <attribute name='ramco_prorationschedule' />
                            <attribute name='ramco_currentproduct' />
                            <attribute name='ramco_amount' />
                            <order attribute='ramco_name' descending='false' />
                            <filter type='and'>
                              <filter type='or'>
                                <condition attribute='ramco_enddate' operator='null' />
                                <condition attribute='ramco_enddate' operator='next-x-years' value='10' />
                              </filter>
                            </filter>
                            <link-entity name='ramco_service_membershipapplication' from='ramco_serviceid' to='ramco_serviceid' visible='false' intersect='true'>
                              <link-entity name='cobalt_membershipapplicationfee' from='cobalt_membershipapplicationfeeid' to='cobalt_membershipapplicationfeeid' alias='ac'>
                                <filter type='and'>
                                  <condition attribute='cobalt_membershipapplicationfeeid' operator='eq' uitype='cobalt_membershipapplicationfee' value='";


            string appFeeGUID = this.AppFee.Get(context).Id.ToString();
                    tracingService.Trace("App Fee ID Converted to String");

            string fetchEnd = @"' />
                                </filter>
                              </link-entity>
                            </link-entity>
                          </entity>
                        </fetch>";
            
            

            string fetchxml = fetchStart + appFeeGUID + fetchEnd;
                    tracingService.Trace("Query string built");

            //Define Billing Year String
            string billingYear = DateTime.Now.Year.ToString();
            string billingYearString = billingYear.Replace(",","");


            //Query for related services
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchxml));
                    tracingService.Trace("Generate Entity Collection");

            //Create dues product for each service
            foreach (Entity p in results.Entities)
            {
                        tracingService.Trace("Begin foreach loop");
                Guid productId = p.GetAttributeValue<EntityReference>("ramco_currentproduct").Id;

                //rdp = Ramco Dues Product
                Entity rdp = new Entity("cobalt_duesproduct");
                rdp["cobalt_duesoptionid"] = new EntityReference("cobalt_duesoption", this.NewDuesOption.Get(context).Id);
                        tracingService.Trace("Set dues option reference");
                rdp["cobalt_productid"] = new EntityReference("product", productId);
                        tracingService.Trace("Set product reference");
                rdp["ramco_parentservice"] = new EntityReference("ramco_service", p.GetAttributeValue<Guid>("ramco_serviceid"));
                        tracingService.Trace("Set service reference");
                rdp["cobalt_name"] = billingYearString + " | " + p.GetAttributeValue<string>("ramco_name");
                        tracingService.Trace("Set name String");

                service.Create(rdp);
                tracingService.Trace("create dues product... restart loop");
            }

        }

 

    }
}

