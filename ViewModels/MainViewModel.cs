using MahApps.Metro.Controls;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class MainViewModel
    {
        public static HamburgerMenu HamburgerMenuControl { get; set; }
        public MainViewModel()
        {
            ServiceDB.GetOtherStuff();
        }
    }
}