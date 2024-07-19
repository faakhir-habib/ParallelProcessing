using System.Diagnostics;
using Medallion.Threading;
using Quartz;

namespace ParallelProcessing.Server
{

    public class ParallelProcessingJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly IDistributedLockProvider _lockProvider;

        public ParallelProcessingJob(IEmailService emailService, IDistributedLockProvider lockProvider)
        {
            _emailService = emailService;
            _lockProvider = lockProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using (await _lockProvider.AcquireLockAsync("ParallelProcessingLock"))
            {
                // Ensure this job runs only once across all instances

                // Sample list of items
                var items = new List<string> { "Item1", "Item2", "Item3" };

                var tasks = items.Select(item => Task.Run(() => ProcessItem(item)));
                await Task.WhenAll(tasks);

                // Notify if job was not started
                if (!context.ScheduledFireTimeUtc.HasValue)
                {
                    await _emailService.SendAlertAsync("Job did not start as scheduled");
                }
            }
        }

        private void ProcessItem(string item)
        {
            Debug.WriteLine($"{item}, {DateTime.Now:O}");
        }
    }

}
