using Microsoft.Azure.Management.ServiceBus.Fluent;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent.Models;

namespace PauseOrResumeSubscriptions
{
    public struct GlobalDeclared
    {
        public static string TOPIC_NAME = "TOPIC_NAME";
        public static string CLEINT_ID = "CLEINT_ID";
        public static string CLEINT_SECRET = "CLEINT_SECRET";
        public static string TENANT_ID = "TENANT_ID";
        public static string SUBSCRIPTION_ID = "SUBSCRIPTION_ID";
        public static string RESOURCE_GROUP_NAME = "RESOURCE_GROUP_NAME";
        public static string SERVICE_BUS_NAMESPACE_NAME = "SERVICE_BUS_NAMESPACE_NAME";
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("===================================================");
            Console.WriteLine("Options:");
            Console.WriteLine("        1. PAUSE");
            Console.WriteLine("        2. RESUME");
            Console.WriteLine("===================================================");
            Console.WriteLine("Please enter any one of the option");
            Console.WriteLine("===================================================");
            var option = Console.ReadLine();

            if (option == "1")
            {
                var isSuccess = await PauseSubscriptionsForTopic(GlobalDeclared.TOPIC_NAME);
            }

            if (option == "2")
            {
                var isSuccess = await ResumeSubscriptionsForTopic(GlobalDeclared.TOPIC_NAME);
            }
        }

        public static async Task<bool> PauseSubscriptionsForTopic(string topicName)
        {
            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(GlobalDeclared.CLEINT_ID, GlobalDeclared.CLEINT_SECRET, GlobalDeclared.TENANT_ID, AzureEnvironment.AzureGlobalCloud);
            var serviceBusManager = ServiceBusManager.Authenticate(credentials, GlobalDeclared.SUBSCRIPTION_ID);
            var serviceBusNamespace = serviceBusManager.Namespaces.GetByResourceGroup(GlobalDeclared.RESOURCE_GROUP_NAME, GlobalDeclared.SERVICE_BUS_NAMESPACE_NAME);
            var topics = await serviceBusNamespace.Topics.ListAsync();
            var topic = topics.FirstOrDefault(t => t.Name == topicName);
            foreach (var subscr in topic.Subscriptions.List())
            {
                SubscriptionInner subscriptionInner = subscr.Inner;
                if (subscriptionInner.Status == EntityStatus.Active)
                    subscriptionInner.Status = EntityStatus.ReceiveDisabled;

                var operation = subscr.Manager.Inner.Subscriptions;
                await operation.CreateOrUpdateAsync(GlobalDeclared.RESOURCE_GROUP_NAME, GlobalDeclared.SERVICE_BUS_NAMESPACE_NAME, topicName, subscr.Name, subscriptionInner);
            }

            return true;
        }

        public static async Task<bool> ResumeSubscriptionsForTopic(string topicName)
        {
            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(GlobalDeclared.CLEINT_ID, GlobalDeclared.CLEINT_SECRET, GlobalDeclared.TENANT_ID, AzureEnvironment.AzureGlobalCloud);
            var serviceBusManager = ServiceBusManager.Authenticate(credentials, GlobalDeclared.SUBSCRIPTION_ID);
            var serviceBusNamespace = serviceBusManager.Namespaces.GetByResourceGroup(GlobalDeclared.RESOURCE_GROUP_NAME, GlobalDeclared.SERVICE_BUS_NAMESPACE_NAME);
            var topics = await serviceBusNamespace.Topics.ListAsync();
            var topic = topics.FirstOrDefault(t => t.Name == topicName);

            foreach (var subscr in topic?.Subscriptions?.List())
            {
                SubscriptionInner subscriptionInner = subscr.Inner;
                if (subscriptionInner.Status != EntityStatus.Active)
                    subscriptionInner.Status = EntityStatus.Active;

                var operation = subscr.Manager.Inner.Subscriptions;
                await operation.CreateOrUpdateAsync(GlobalDeclared.RESOURCE_GROUP_NAME, GlobalDeclared.SERVICE_BUS_NAMESPACE_NAME, topicName, subscr.Name, subscriptionInner);
            }

            return true;
        }
    }
}
