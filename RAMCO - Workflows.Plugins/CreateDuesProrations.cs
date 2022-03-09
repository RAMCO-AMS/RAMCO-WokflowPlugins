using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk.Messages;

namespace RAMCO___Workflows.Plugins
{
    public partial class CreateDuesProrations : CodeActivity
    {
        // This is an example argument
        //[RequiredArgument]
        //[Input("Select an email to send")]
        //[ReferenceTarget("email")]
        //public InArgument<EntityReference> EmailReference { get; set; }

        [Input("Duration Start")]
        [RequiredArgument]
        public InArgument<DateTime> DurationStart { get; set; }

        [Input("Duration End")]
        [RequiredArgument]
        public InArgument<DateTime> DurationEnd { get; set; }

        [Input("Proration Schedule")]
        [RequiredArgument]
        [AttributeTarget("ramco_service", "ramco_prorationschedule")]
        public InArgument<OptionSetValue> ProrationSchedule { get; set; }

        [RequiredArgument]
        [Input("Dues Product")]
        [ReferenceTarget("cobalt_duesproduct")]
        public InArgument<EntityReference> DuesProduct { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            //Todo: implement your logic here
            // Create instances of workflow context and organization service to make use of their methods
            IWorkflowContext execontext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(execontext.UserId);
            ITracingService tracingService = context.GetExtension<ITracingService>();



        }

        private static void CreateProration(DateTime StartDate, DateTime EndDate, Money Amount, EntityReference TargetDuesProduct, IOrganizationService service)
        {
            Entity pro = new Entity("cobalt_duesproration");
            pro["cobalt_begindate"] = StartDate;
            pro["cobalt_enddate"] = EndDate;
            pro["cobalt_amount"] = Amount;
            pro["cobalt_duesproductid"] = TargetDuesProduct;


            service.Create(pro);
        }


    }
}

