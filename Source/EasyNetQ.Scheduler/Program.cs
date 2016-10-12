using log4net.Config;
using Topshelf;

namespace EasyNetQ.Scheduler
{
    using System.Configuration;

    public class Program
    {
        static void Main()
        {
            XmlConfigurator.Configure();

            HostFactory.Run(hostConfiguration =>
            {
                hostConfiguration.EnableServiceRecovery(serviceRecoveryConfiguration =>
                {
                    serviceRecoveryConfiguration.RestartService(delayInMinutes: 1); // On the first service failure, reset service after a minute
                    serviceRecoveryConfiguration.SetResetPeriod(days: 0); // Reset failure count after every failure
                });

                hostConfiguration.RunAsLocalSystem();
                var serviceId = ConfigurationManager.AppSettings["ServiceId"];
                hostConfiguration.SetDescription(serviceId);
                hostConfiguration.SetDisplayName(serviceId);
                hostConfiguration.SetServiceName(serviceId);

                hostConfiguration.Service<ISchedulerService>(serviceConfiguration =>
                {
                    serviceConfiguration.ConstructUsing(_ => SchedulerServiceFactory.CreateScheduler());

                    serviceConfiguration.WhenStarted((service, _) =>
                    {
                        service.Start();
                        return true;
                    });
                    serviceConfiguration.WhenStopped((service, _) =>
                    {
                        service.Stop();
                        return true;
                    });
                });
            });
        }
    }
}
