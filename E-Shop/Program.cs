using E_Shop.Controllers;
using E_Shop.Data;
using E_Shop.Data.Services;
using E_Shop.Services;
using Webserver;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = new WebServerBuilder();

        builder.SetHostUrl("http://localhost:8080/");
        builder.SetHostDir(GetProjectPath());
        builder.SetConfigPath("appSettings.json");

        builder.Services.AddSingleton(() => new DbConnectionProvider(builder.Configuration));
        builder.Services.AddSingleton(() => new EmailService(builder.Configuration));

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IOrderItemsService, OrderItemsService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IOrderItemsService, OrderItemsService>();
        builder.Services.AddScoped<IUserConfirmKeyService, UserConfirmKeyService>();
        builder.Services.AddScoped<IProductKeyService, ProductKeyService>();

        builder.Services.AddSingleton(() => new LayoutBuilder(builder.BuildServiceProvider()));

        builder.AddController<HomeController>();
        builder.AddController<UserController>();
        builder.AddController<CategoryController>();
        builder.AddController<ProductsController>();
        builder.AddController<CartController>();
        builder.AddController<OrderController>();
        builder.AddController<PaymentController>();
        builder.AddController<ProductKeyController>();

        var server = builder.Build();

        server.Run();
    }

    private static string GetProjectPath() =>
        Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
}