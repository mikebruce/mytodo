using MyToDo.Events;
using MyToDo.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.ViewModels
{
    public class NavigationViewModel :BindableBase, INavigationAware
    {
        public readonly IContainerProvider containerProvider;

        public readonly IEventAggregator eventAggregator;
        public NavigationViewModel(IContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
            eventAggregator = containerProvider.Resolve<IEventAggregator>();
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
        public void UpdateLoading(bool isOpen)
        {
            eventAggregator.UpdateLoading(new UpdateModel { IsOpen = isOpen });
        }
    }
}
