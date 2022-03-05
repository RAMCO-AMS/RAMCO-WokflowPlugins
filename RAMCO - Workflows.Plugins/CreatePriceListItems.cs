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
    public partial class CreatePriceListItems : CodeActivity
    {
        // This is an example argument
        //[RequiredArgument]
        //[Input('Select an email to send')]
        //[ReferenceTarget('email')]

        //public InArgument<EntityReference> EmailReference { get; set; }

        [Input("Product")]
        [RequiredArgument]
        [ReferenceTarget("product")]
        public InArgument<EntityReference> Product { get; set; }
        
        [RequiredArgument]
        [Input("Price")]
        public InArgument<Money> Price { get; set; }

        protected override void Execute(CodeActivityContext context)
        {

            // Establish connection to current workflow and organization
            IWorkflowContext execontext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(execontext.InitiatingUserId);

            //Get product's GUID
            Guid productId = this.Product.Get(context).Id;
            Money listPrice = this.Price.Get(context);



            //Query for defaul UoM
            QueryExpression query = new QueryExpression {EntityName= "uom" };
            query.ColumnSet = new ColumnSet("uomid");

            Entity defaultUnit = service.RetrieveMultiple(query).Entities.FirstOrDefault();

            EntityReference _defaultUnit = defaultUnit.ToEntityReference();
   
            //Build fetch for all active price lists
            string fetchxml = @"
        <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
          <entity name='pricelevel'>
            <attribute name='name' />
            <attribute name='statecode' />
            <attribute name='pricelevelid' />
            <order attribute='name' descending='false' />
            <filter type='and'>
              <condition attribute='statecode' operator='eq' value='0' />
            </filter>
          </entity>
        </fetch>";


            //Query Active Price Lists and store in collection
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchxml));

            //Create price list item for each
            foreach (Entity p in results.Entities)
            {
                Entity pli = new Entity("productpricelevel");
                pli["productid"] = new EntityReference("product",productId);
                pli["pricelevelid"] = new EntityReference("pricelevel",p.Id);
                pli["amount"] = listPrice;
                pli["uomid"] = _defaultUnit;


                service.Create(pli);
            }

        }
    }
}

