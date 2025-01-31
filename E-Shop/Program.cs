using E_Shop.Controllers;
using E_Shop.Data;
using E_Shop.Data.Services;
using E_Shop.Services;
using Webserver;

internal class Program
{
    private static void Main(string[] args)
    {
        var server = new WebServer(
            hostUrl: "http://localhost:8080/",
            hostDir: GetProjectPath(),
            configPath: "appsettings.json"
        );

        DbConnectionProvider.ConnectionString = server.GetConfig("ConnectionString");
        EmailSender.Initialize(
            host: server.GetConfig("SmtpHost")!,
            username: server.GetConfig("SmtpUsername")!,
            password: server.GetConfig("SmtpPassword")!
        );

        server.AddService<IUserService, UserService>();
        server.AddService<ICategoryService, CategoryService>();
        server.AddService<IProductService, ProductService>();
        server.AddService<IProductCategoryService, ProductCategoryService>();
        server.AddService<ICartService, CartService>();
        server.AddService<IOrderItemsService, OrderItemsService>();
        server.AddService<IOrderService, OrderService>();
        server.AddService<IUserConfirmKeyService, UserConfirmKeyService>();
        server.AddService<IProductKeyService, ProductKeyService>();

        server.AddController<HomeController>();
        server.AddController<UserController>();
        server.AddController<CategoryController>();
        server.AddController<ProductsController>();
        server.AddController<CartController>();
        server.AddController<OrderController>();
        server.AddController<PaymentController>();
        server.AddController<ProductKeyController>();

        server.Run();
    }

    private static string GetProjectPath() =>
        Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
}