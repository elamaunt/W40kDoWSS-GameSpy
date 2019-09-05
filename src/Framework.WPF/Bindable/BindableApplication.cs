using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace Framework.WPF
{
    public abstract partial class BindableApplication : System.Windows.Application
    {
        public ApplicationViewModel ViewModel { get; private set; }

        protected abstract IEnumerable<Module> CreateModules();
        
        protected override void OnStartup(StartupEventArgs e)
        {
#if !SPACEWAR
            ViewModel = CreateApplicationViewModel();
            Bootstrapper.Run(CreateModules().ToArray());
            FrameBinder.Bind(this, ViewModel);
#endif
            base.OnStartup(e);
        }
        
        protected virtual ApplicationViewModel CreateApplicationViewModel()
        {
            return new ApplicationViewModel();
        }
    }
}
