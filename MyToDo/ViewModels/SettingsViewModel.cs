using MyToDo.Common.Models;
using MyToDo.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyToDo.ViewModels
{
    public class SettingsViewModel:BindableBase
    {
        private readonly IRegionManager regionManager;

        public SettingsViewModel(IRegionManager regionManager)
        {
            MenuBars=new ObservableCollection<MenuBar> ();
            this.regionManager = regionManager;
            NavigateCommmand = new DelegateCommand<MenuBar>(Navigate);
            CreateMenuBar(); 
        }
        private void Navigate(MenuBar bar)
        {
            if (bar == null || string.IsNullOrWhiteSpace(bar.NameSpace))
            {
                return;
            }
            regionManager.Regions[PrismManager.SettingsViewRegionName].RequestNavigate(bar.NameSpace);

        }
        public DelegateCommand<MenuBar> NavigateCommmand { get; private set; }
        private ObservableCollection<MenuBar> menuBars;
        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }



        void CreateMenuBar()
        {
            MenuBars.Add(new MenuBar() { Icon = "Palette", Title = "个性化", NameSpace = "SkinView" });
            MenuBars.Add(new MenuBar() { Icon = "Cog", Title = "系统设置", NameSpace = "" });
            MenuBars.Add(new MenuBar() { Icon = "InformationOutline", Title = "关于更多", NameSpace = "AboutView" });
          
        }
    }
}
