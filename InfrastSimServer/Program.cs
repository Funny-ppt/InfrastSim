
namespace InfrastSimServer {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.ConfigureHttpJsonOptions((options) => {
                options.SerializerOptions.TypeInfoResolverChain.Add(SourceGenerationContext.Default);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseAuthorization();

            var simulatorService = new SimulatorService();
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            //app.MapGet("/weatherforecast", (HttpContext httpContext) => {
            //    var forecast = Enumerable.Range(1, 5).Select(index =>
            //        new WeatherForecast {
            //            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            //            TemperatureC = Random.Shared.Next(-20, 55),
            //            Summary = summaries[Random.Shared.Next(summaries.Length)]
            //        })
            //        .ToArray();
            //    return forecast;
            //})
            //.WithName("GetWeatherForecast")
            //.WithOpenApi();

            app.MapGet("/simulator", simulatorService.Create)
            .WithName("CreateSimulator")
            .WithDescription("创建新的空模拟器, 返回其 id")
            .WithOpenApi();

            app.MapPost("/simulator", simulatorService.CreateWithData)
            .WithName("CreateSimulatorWithData")
            .WithDescription("创建使用给定的数据新的模拟器, 返回其 id。HTTP Body应传入序列化的模拟器json")
            .WithOpenApi();

            app.MapGet("/simulator/{id}", simulatorService.GetData)
            .WithName("GetSimulatorData")
            .WithDescription("获取给定模拟器的序列化结果, 以便未来读取并使用; 有一个可选的 detailed 参数")
            .WithOpenApi();

            app.MapGet("/simulator/{id}/mowerdata", simulatorService.GetDataForMower)
            .WithName("GetSimulatorDataForMower")
            .WithDescription("获取给定模拟器的数据, 并格式化为Mower易于识别的格式")
            .WithOpenApi();

            app.MapGet("/simulator/{id}/simulate", simulatorService.Simulate)
            .WithName("Simulate")
            .WithDescription("计算给定模拟器运行1分钟后的情况")
            .WithOpenApi();

            app.MapGet("/simulator/{id}/simulate/{until}", simulatorService.SimulateUntil)
            .WithName("SimulateUntil")
            .WithDescription("计算给定模拟器运行到指定之间后的情况")
            .WithOpenApi();

            app.MapGet("/simulator/{id}/operators", simulatorService.GetOperators)
            .WithName("GetOperators")
            .WithDescription("获取所有可用干员及其状态")
            .WithOpenApi();

            app.MapPost("/simulator/{id}/operators", simulatorService.SetUpgraded)
            .WithName("SetUpgraded")
            .WithDescription("设定给出列表干员的精英化程度")
            .WithOpenApi();

            app.MapPost("/simulator/{id}/{facility}", simulatorService.SetFacilityState)
            .WithName("SetFacilityState")
            .WithDescription("接受JSON数据并快速设置设施状态（含降级、升级、创建设施、快速调整干员位置等） strategy表示切贸易站策略;product表示改变制造站产品;level代表升降级设施，但不会计算无人机消耗")
            .WithOpenApi();

            app.MapPost("/simulator/{id}/{facility}/operators", simulatorService.SelectOperators)
            .WithName("SelectOperators")
            .WithDescription("设定某设施的干员列表")
            .WithOpenApi();

            app.MapDelete("/simulator/{id}/{facility}/operators/{idx}", simulatorService.RemoveOperator)
            .WithName("RemoveOperator")
            .WithDescription("撤出某设施的第idx个干员(从1开始计数)")
            .WithOpenApi();

            app.MapDelete("/simulator/{id}/{facility}/operators", simulatorService.RemoveOperator)
            .WithName("RemoveOperators")
            .WithDescription("撤出某设施的全部干员")
            .WithOpenApi();

            app.MapGet("/simulator/{id}/{facility}/collect/{idx?}", simulatorService.Collect)
            .WithName("Collect")
            .WithDescription("收获贸易站的全部产物；交付贸易站全部订单或交付贸易站第idx个订单(从1开始计数) 可选查询参数: idx")
            .WithOpenApi();

            app.MapGet("/simulator/{id}/{facility}/collectAll", simulatorService.CollectAll)
            .WithName("CollectAll")
            .WithDescription("收获全部产物并交付所有订单")
            .WithOpenApi();

            app.MapGet("/simulator/{id}/sanity", simulatorService.Sanity)
            .WithName("Sanity")
            .WithDescription("源石冲理智 必填查询参数: amount")
            .WithOpenApi();

            app.MapGet("/simulator/{id}/{facility}/use_drones", simulatorService.UseDrones)
            .WithName("UseDrones")
            .WithDescription("使用无人机 必填查询参数: amount")
            .WithOpenApi();

            app.Run();
        }
    }
}