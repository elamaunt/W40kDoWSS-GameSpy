using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamSpy.ViewModels.PageViewModels
{
    public interface IPageViewModel
    {
        string PageName { get; set; }
        void SetPageName();
    }
}
