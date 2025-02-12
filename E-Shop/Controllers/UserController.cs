﻿using E_Shop.Data.Services;
using E_Shop.Utility;
using E_Shop.Models;
using System.Text;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Net;
using Webserver.Services;
using Webserver.Controllers;
using Webserver.Controllers.Content;
using Webserver.Sessions;
using E_Shop.Services;
using Webserver.Controllers.Attributes;

namespace E_Shop.Controllers
{
    internal class UserController : Controller
    {
        private static Dictionary<string, string> statusColors = new Dictionary<string, string>()
        {
            { "Active", "warning" },
            { "Completed", "success" },
            { "Canceled",  "danger" }
        };

        private readonly IUserService _userService;
        private readonly IOrderItemsService _orderItemsService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IUserConfirmKeyService _userConfirmKeyService;
        private readonly IHttpContextAccessor _HttpContextAccessor;
        private readonly IConfigurationProvider _configProvider;
        private readonly EmailService _emailService;
        private readonly LayoutBuilder _layoutBuilder;

        public UserController(
            IUserService userService,
            IOrderItemsService orderItemsService,
            IOrderService orderService,
            IProductService productService,
            IUserConfirmKeyService userConfirmKeyService,
            IHttpContextAccessor httpContextAccessor,
            IConfigurationProvider configProvider,
            EmailService emailService,
            LayoutBuilder layoutBuilder)
        {
            _userService = userService;
            _orderItemsService = orderItemsService;
            _orderService = orderService;
            _productService = productService;
            _userConfirmKeyService = userConfirmKeyService;
            _HttpContextAccessor = httpContextAccessor;
            _configProvider = configProvider;
            _emailService = emailService;
            _layoutBuilder = layoutBuilder;
        }

        [HttpGet]
        public IActionResult Register(Session session)
        {
            _layoutBuilder.Configure(session);
            return View("Register", "User/register.html", Validator.Fill(10));
        }

        [HttpPost]
        public IActionResult Register(Session session, Dictionary<string, string> data)
        {
            Validator validator = new Validator(data);

            validator.Validate("name");
            validator.ValidateEmail("email");
            validator.Validate("psw");
            validator.Validate("psw-repeat");

            if (data["psw"] != data["psw-repeat"])
            {
                validator.SetCustomError("psw", "Passwords don't match");
            }

            if (validator.IsValid && _userService.Get(data["email"]) != null)
            {
                validator.SetCustomError("email", "Account with this email already exists");
            }

            if (!validator.IsValid)
            {
                _layoutBuilder.Configure(session);
                return View("Register error", "User/register.html", validator.ToValueError());
            }

            _userService.Add(new User(data));
            User user = _userService.Get(data["email"])!;

            _SendConfirmEmail(user);

            return Redirect("../User/RegisterSuccess");
        }

        private string _GetHash(User user)
        {
            StringBuilder data = new StringBuilder();
            data.Append(user.Id);
            data.Append(user.Username);
            data.Append(user.Email);
            data.Append(user.Password);
            data.Append(DateTime.Now.Ticks);

            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data.ToString()));
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        private void _SendConfirmEmail(User user)
        {
            string key = _GetHash(user);
            var confirmKey = new UserConfirmKey(user.Id, key);

            if (_userConfirmKeyService.Get(user.Id) != null)
            {
                _userConfirmKeyService.Update(confirmKey);
            }
            else
            {
                _userConfirmKeyService.Add(confirmKey);
            }

            string from = _configProvider.GetSetting("ShopEmail")!;
            string to = user.Email;

            MailMessage mail = new MailMessage(from, to);

            mail.Subject = "Confirm email";

            StringBuilder body = new StringBuilder();
            body.Append(File.ReadAllText($"{AbsolutePath}Views/User/conf-email_p1.html"));
            body.Append(string.Format(File.ReadAllText($"{AbsolutePath}Views/User/conf-email_p2.html"),
                user.Email, _HttpContextAccessor.Context.Request.UserHostName, key));

            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            _emailService.Send(mail);
        }

        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View("Registration success", "User/register-success.html");
        }

        [HttpPost]
        public IActionResult ConfirmEmail(Dictionary<string, string> data)
        {
            Validator validator = new Validator(data);
            if (!validator.Validate("key", 60, 70))
            {
                return Error(HttpStatusCode.BadRequest);
            }

            User? user = _userConfirmKeyService.Get(data["key"]);
            
            if (user == null)
            {
                return Error(HttpStatusCode.NotFound);
            }
            
            user.Verified = true;

            _userService.Update(user.Id, user);
            _userConfirmKeyService.Delete(user.Id);

            return Redirect("Login");
        }

        [HttpGet]
        public IActionResult Login(Session session)
        {
            _layoutBuilder.Configure(session);
            return View("Login", "User/login.html", Validator.Fill(6));
        }

        [HttpPost]
        public IActionResult Login(Session session, Dictionary<string, string> data)
        {
            User[] admins = _userService.GetAll("admin");
            foreach (User admin in admins)
            {
                if (data["email"] ==  admin.Email && data["psw"] == admin.Password)
                {
                    session.AddUser(admin);
                    return Redirect("../Home/Index");
                }
            }

            Validator validator = new Validator(data);

            validator.ValidateEmail("email");
            validator.Validate("psw");

            validator.AddInfoField("status");

            if (!validator.IsValid)
            {
                _layoutBuilder.Configure(session);
                return View("Login", "User/login.html", validator.ToValueError());
            }

            User? user = _userService.Get(data["email"], data["psw"]);

            if (user != null)
            {
                if (!user.Verified)
                {
                    validator.SetCustomError("status", "Confirm email to continue.<br><br>");
                    return View("Login", "User/login.html", validator.ToValueError());
                }
                session.AddUser(user);
                return Redirect("../Home/Index");
            }

            validator.SetCustomError("status", "Incorrect email or password<br><br>");

            _layoutBuilder.Configure(session);
            return View("Login", "User/login.html", validator.ToValueError());
        }

        [HttpGet]
        public IActionResult RequestResetPassword()
        {
            return View("Reset password", "User/request-reset-psw.html", Validator.Fill(2));
        }

        [HttpPost] 
        public IActionResult RequestResetPassword(Dictionary<string, string> data)
        {
            Validator validator = new Validator(data);
            validator.ValidateEmail("email");

            if (validator.IsValid && _userService.Get(data["email"]) == null)
            {
                validator.SetCustomError("email", "No registered accounts with this email");
            }

            if (!validator.IsValid)
            {
                return View("Reset password", "User/request-reset-psw.html", validator.ToValueError());
            }

            _SendResetPswEmail(_userService.Get(data["email"])!);

            return Redirect("../Home/Index");
        }

        private void _SendResetPswEmail(User user)
        {
            string key = _GetHash(user);
            var confirmKey = new UserConfirmKey(user.Id, key);

            if (_userConfirmKeyService.Get(user.Id) != null)
            {
                _userConfirmKeyService.Update(confirmKey);
            }
            else
            {
                _userConfirmKeyService.Add(confirmKey);
            }

            string from = _configProvider.GetSetting("ShopEmail")!;
            string to = user.Email;

            MailMessage mail = new MailMessage(from, to);

            mail.Subject = "Reset password";

            StringBuilder body = new StringBuilder();
            body.Append(File.ReadAllText($"{AbsolutePath}Views/User/reset-psw-email_p1.html"));
            body.Append(string.Format(File.ReadAllText($"{AbsolutePath}Views/User/reset-psw-email_p2.html"),
                _HttpContextAccessor.Context.Request.UserHostName, key));

            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            _emailService.Send(mail);
        }

        [HttpPost]
        public IActionResult ResetPassword(Dictionary<string, string> data)
        {
            Validator validator = new Validator(data);
            if (!validator.Validate("key", 60, 70))
            {
                return Error(HttpStatusCode.BadRequest);
            }

            if (_userConfirmKeyService.Get(data["key"]) == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            List<string> args = new List<string>(Validator.Fill(4));
            args.Add(data["key"]);
            
            return View("Reset password", "User/reset-psw.html", args.ToArray());
        }

        [HttpPost]
        public IActionResult ProceedResetPassword(Dictionary<string, string> data)
        {
            Validator validator = new Validator(data);
            validator.maxLength = 100;

            validator.Validate("psw");
            validator.Validate("psw-repeat");

            if (!validator.Validate("key", 60, 70))
            {
                return Error(HttpStatusCode.BadRequest);
            }

            if (data["psw"] != data["psw-repeat"])
            {
                validator.SetCustomError("psw", "Passwords don't match");
            }

            if (!validator.IsValid)
            {
                List<string> args = new List<string>(validator.ToValueError());
                args.Add(data["key"]);

                return View("Reset password", "User/reset-psw.html", args.ToArray());
            }

            User? user = _userConfirmKeyService.Get(data["key"]);

            if (user == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            user.Password = data["psw"];
            _userService.Update(user.Id, user);

            _userConfirmKeyService.Delete(user.Id);

            return Redirect("Login");
        }

        [HttpGet]
        public IActionResult OrderHistory(Session session)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            int userId = int.Parse(session.Properties["id"]);

            Order[] orders = _orderService.GetAll(userId);

            string data = "<p class=\"mt-2\">No purchase history</p>";

            if (orders.Length > 0 )
            {
                data = File.ReadAllText($"{AbsolutePath}Views/User/_table.html");
                string row = File.ReadAllText($"{AbsolutePath}Views/User/_row.html");
                
                StringBuilder rows = new StringBuilder();

                foreach ( Order order in orders )
                {
                    OrderItem[] orderItems = _orderItemsService.GetAll(order.Id);
                    List<string> items = new List<string>();

                    foreach( OrderItem item in orderItems )
                    {
                        Product? product = _productService.Get(item.ProductId);
                        if ( product == null )
                        {
                            continue;
                        }
                        items.Add($"{product.Name} x{item.Number}");
                    }

                    if (!statusColors.TryGetValue(order.Status, out string? color))
                    {
                        color = "";
                    }
                    rows.AppendLine(string.Format(row, order.Id, color, order.Status,
                        string.Join(", ", items), string.Format("{0:F2}$", order.TotalCost)));
                }
                data = string.Format(data, rows.ToString());
            }          

            _layoutBuilder.Configure(session);
            return View("Order history", "User/index.html", "Order history", data);
        }

        [HttpGet]
        public IActionResult Edit(Session session)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            string username = session.Properties["username"];

            List<string> parameters = [username, username];

            for (int i = 0; i < 7; i++)
            {
                parameters.Add("");
            }

            return View($"Edit - {username}", "User/edit.html", parameters.ToArray());
        }

        [HttpPost]
        public IActionResult UpdateUsername(Session session, Dictionary<string, string> data)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            User? user = _userService.Get(int.Parse(session.Properties["id"]));

            if (user == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            Validator validator = new Validator(data);

            validator.Validate("username");

            if (!validator.IsValid)
            {
                string[] values = validator.ToValueError();

                List<string> parameters = [session.Properties["username"]];
                for (int i = 0; i < 8; i++)
                {
                    parameters.Add("");
                }
                parameters[1] = values[0];
                parameters[2] = values[1];

                return View($"Edit - {session.Properties["username"]}", "User/edit.html", parameters.ToArray());
            }

            user.Username = data["username"];
            _userService.Update(user.Id, user);
            session.Properties["username"] = data["username"];

            return Redirect("../Home/Index");
        }

        [HttpPost]
        public IActionResult UpdatePassword(Session session, Dictionary<string, string> data)
        {
            if (!session.Authorized)
            {
                return Error(HttpStatusCode.Unauthorized);
            }

            User? user = _userService.Get(int.Parse(session.Properties["id"]));

            if (user == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            Validator validator = new Validator(data);

            validator.Validate("psw-old");
            validator.Validate("psw");
            validator.Validate("psw-repeat");

            if (data["psw-old"] != user.Password)
            {
                validator.SetCustomError("psw-old", "Incorrect password");
            }
            if (data["psw"] != data["psw-repeat"])
            {
                validator.SetCustomError("psw", "Passwords don't match");
            }

            if (!validator.IsValid)
            {
                string[] values = validator.ToValueError();
                string username = session.Properties["username"];

                List<string> parameters = [username, username, ""];

                foreach (string value in values)
                {
                    parameters.Add(value);
                }

                return View($"Edit - {username}", "User/edit.html", parameters.ToArray());
            }

            user.Password = data["psw"];
            _userService.Update(user.Id, user);

            return Redirect("Logout");
        }

        [HttpGet]
        public IActionResult Logout(Session session)
        {
            session.Authorized = false;
            session.Properties = new Dictionary<string, string>();

            _layoutBuilder.Configure(session);
            return Redirect("../../Home/Index");
        }
    }
}
