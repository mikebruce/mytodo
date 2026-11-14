using MyToDo.Common;
using MyToDo.Service;
using MyToDo.Shared;
using MyToDo.ViewModels;
using MyToDo.ViewModels.Dialogs;
using MyToDo.Views;
using MyToDo.Views.Dialogs;
using Newtonsoft.Json;
using Prism.DryIoc;
using RestSharp;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Reflection;
using System.Windows;

namespace MyToDo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static readonly string apiUrl = "https://localhost:44335";

        protected override Window CreateShell()
        {
            return Container.Resolve<MainView>();
        }

        public static void LoginOut(IContainerProvider containerProvider)
        {
            AppSession.Clear();
            var httpClient = containerProvider.Resolve<HttpClient>();
            httpClient.DefaultRequestHeaders.Authorization = null;

            Current.MainWindow.Hide();
            var diaglog = containerProvider.Resolve<IDialogService>();
            diaglog.ShowDialog("LoginView", callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
                Current.MainWindow.Show();
            });
        }

        protected override void OnInitialized()
        {
            var httpRestClient = ContainerLocator.Container.Resolve<HttpRestClient>();

            httpRestClient.RefreshTokenAsync = async (refreshToken) =>
            {
                var client = new RestClient();
                var req = new RestRequest($"{apiUrl}/api/Login/RefreshMyToken", Method.Post);
                req.AddJsonBody(new { RefreshToken = refreshToken });
                var resp = await client.ExecuteAsync(req);

                if (!resp.IsSuccessful || string.IsNullOrWhiteSpace(resp.Content))
                    return (null, null);

                var result = JsonConvert.DeserializeObject<ApiResponse>(resp.Content);
                if (result?.IsSuccess != true || string.IsNullOrEmpty(result.Token))
                    return (null, null);

                return (result.Token, result.RefreshToken);
            };

            var diaglog = Container.Resolve<IDialogService>();
            diaglog.ShowDialog("LoginView", callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
                var service = App.Current.MainWindow.DataContext as IConfigureService;
                if (service != null)
                {
                    service.Configure();
                }
                base.OnInitialized();
            });
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDialogWindow, DialogWindow>();

            //containerRegistry.GetContainer().Register<HttpRestClient>(made: Parameters.Of.Type<string>(serviceKey: "webUrl"));

            containerRegistry.GetContainer().RegisterInstance(apiUrl, serviceKey: "webUrl");

            containerRegistry.GetContainer().Register<HttpRestClient>(
                Reuse.Singleton,
                made: Parameters.Of.Type<string>(serviceKey: "webUrl"));

            //containerRegistry.GetContainer().RegisterInstance(@"http://192.168.10.173:8001/", serviceKey: "webUrl");

            containerRegistry.RegisterDialog<LoginView, LoginViewModel>();

            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.Register<IToDoService, ToDoService>();
            containerRegistry.Register<IMemoService, MemoService>();
            containerRegistry.Register<ILoginService, LoginService>();

            containerRegistry.RegisterForNavigation<AddToDoView, AddToDoViewModel>();
            containerRegistry.RegisterForNavigation<AddMemoView, AddMemoViewModel>();
            containerRegistry.RegisterForNavigation<AboutView>();
            containerRegistry.RegisterForNavigation<SkinView, SkinViewModel>();
            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();
            containerRegistry.RegisterForNavigation<IndexView, IndexViewModel>();
            containerRegistry.RegisterForNavigation<ToDoView, ToDoViewModel>();
            containerRegistry.RegisterForNavigation<MemoView, MemoViewModel>();
            containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
        }
    }
}