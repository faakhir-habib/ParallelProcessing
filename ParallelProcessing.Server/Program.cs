
using Medallion.Threading.SqlServer;
using Medallion.Threading;
using Quartz;

namespace ParallelProcessing.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddQuartz(q =>
            {
                q.UsePersistentStore(s =>
                {
                    s.UseBinarySerializer();
                    s.UseSqlServer(options =>
                    {
                        options.ConnectionString = "Data Source=FAAKHIR-HABIB\\FAAKHIR;Initial Catalog=TestDb;Encrypt=False;Integrated Security=true;Connection Timeout=30;";
                    });
                });

                var jobKey = new JobKey("ParallelProcessingJob");
                q.AddJob<ParallelProcessingJob>(opts => opts.WithIdentity(jobKey));
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("ParallelProcessingJob-trigger")
                    .WithCronSchedule("*/5 * * * * ?")); // Run every 5 seconds
            });

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            builder.Services.AddSingleton<IEmailService, EmailService>();

            builder.Services.AddSingleton<IDistributedLockProvider>(_ =>
                new SqlDistributedSynchronizationProvider(
                    "Data Source=FAAKHIR-HABIB\\FAAKHIR;Initial Catalog=TestDb;Encrypt=False;Integrated Security=true;Connection Timeout=30;"));

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
