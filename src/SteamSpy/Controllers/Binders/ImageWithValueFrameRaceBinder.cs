using Framework;
using GSMasterServer.Data;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using SharedServices;

namespace ThunderHawk
{
    public class ImageWithValueFrameRaceBinder : BindingController<Image, ValueFrame<Race>>
    {
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(Frame.Value), OnRaceChanged);
            OnRaceChanged();
        }

        void OnRaceChanged()
        {
            Uri uri = null;

            switch (Frame.Value)
            {
                case Race.space_marine_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/spaceMarine.png");
                    break;
                case Race.chaos_marine_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/chaos.png");
                    break;
                case Race.ork_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/ork.png");
                    break;
                case Race.eldar_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/eldar.png");
                    break;
                case Race.guard_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/ig.png");
                    break;
                case Race.necron_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/necron.png");
                    break;
                case Race.tau_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/tau.png");
                    break;
                case Race.dark_eldar_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/darkEldar.png");
                    break;
                case Race.sisters_race:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/sob.png");
                    break;
                default:
                    uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Races/default.png");
                    break;
            }

            View.Source = new BitmapImage(uri);
        }
    }
}
