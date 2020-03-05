using System.Collections.Generic;
using RandomTools.Types;

namespace RandomTools
{
    public class DefaultDowItemsProvider : IDowItemsProvider
    {
        public DowItem[] Races { get; } = 
        {
            DowItem.AddRace("csm", "Хаос", "Chaos"),
            DowItem.AddRace("de", "Темные эльдары", "Dark Eldars"),
            DowItem.AddRace("eld", "Эльдары", "Eldar"),
            DowItem.AddRace("ig", "Имперская гвардия", "Imperial Guard"),
            DowItem.AddRace("nec", "Некроны", "Necrons"),
            DowItem.AddRace("ork", "Орки", "Orks"),
            DowItem.AddRace("sm", "Космодесант", "Space Marines"),
            DowItem.AddRace("sob", "Сестры битвы", "Sisters of Battle"),
            DowItem.AddRace("tau", "Тау", "Tau")
        };


        public DowItem[] Maps { get; } = 
        {
            DowItem.AddMap2p("bm", "Битва в болотах", "Battle Marshes"),
            DowItem.AddMap2p("br", "Кровавая река", "Blood River"),
            DowItem.AddMap2p("er", "Изумудруная река", "Emerald River"),
            DowItem.AddMap2p("fc", "Мертвый город", "Fallen City"),
            DowItem.AddMap2p("fd", "Отречение Фразира", "Frazier's Demis"),
            DowItem.AddMap2p("fm", "Фата Морга", "Fata Morgana"),
            DowItem.AddMap2p("mom", "Встреча мудрых", "Meetings of Minds"),
            DowItem.AddMap2p("or", "Внешние пределы", "Outer Reaches"),
            DowItem.AddMap2p("qt", "Триумф Квеста", "Quest's Triumph"),
            DowItem.AddMap2p("soe", "Храм Экселлиона", "Shrine of Excellion"),
            DowItem.AddMap2p("te", "Конец покоя", "Tranquilitys End"),
            DowItem.AddMap2p("tf", "Падение Титана", "Titan's Fall"),
            DowItem.AddMap2p("vp", "Вихревое плато", "Vortex Plateau"),
            DowItem.AddMap2p("mb", "Лунная база плато", "Moonbase"),
            
            DowItem.AddMap4p("pl", "Панрейские низины", "Panrea Lowlands"),
            DowItem.AddMap4p("ds", "Спираль рока", "Doom Spiral"),
            
            DowItem.AddMap6p("al", "Альварус", "Alvarus"),
            DowItem.AddMap6p("ms", "Морталис", "Mortalis"),
            
            DowItem.AddMap8p("os", "Оазис Шарра", "Oasis of Sharr"),

        };
    }
}
