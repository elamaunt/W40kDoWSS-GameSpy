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
            DowItem.AddMap2p("er", "Изумрудная река", "Emerald River"),
            DowItem.AddMap2p("fc", "Мертвый город", "Fallen City"),
            DowItem.AddMap2p("fd", "Отречение Фразира", "Frazier's Demis"),
            DowItem.AddMap2p("fm", "Фата Морга", "Fata Morgana"),
            DowItem.AddMap2p("mom", "Встреча мудрых", "Meetings of Minds"),
            DowItem.AddMap2p("or", "Внешние пределы", "Outer Reaches"),
            DowItem.AddMap2p("qt", "Героизм Квеста", "Quest's Triumph"),
            DowItem.AddMap2p("soe", "Храм Экселлиона", "Shrine of Excellion"),
            DowItem.AddMap2p("te", "Конец покоя", "Tranquilitys End"),
            DowItem.AddMap2p("tf", "Дух Титана", "Titan's Fall"),
            DowItem.AddMap2p("vp", "Вихревое плато", "Vortex Plateau"),
            DowItem.AddMap2p("mb", "Лунная база", "Moonbase"),
            
            DowItem.AddMap4p("pl", "Панрейские низины", "Panrea Lowlands"),
            DowItem.AddMap4p("ds", "Спираль рока", "Doom Spiral"),
            DowItem.AddMap4p("bp", "Ошибка Биффи", "Biffy's Peril"),
            DowItem.AddMap4p("dp", "Пик смерти", "Dread Peak"),
            DowItem.AddMap4p("gp", "Перевал Гурмуна", "Gurmun's Pass"),
            DowItem.AddMap4p("ss", "Храм Святых", "Saint's Square"),
            DowItem.AddMap4p("tx", "Тиборакс", "Tiboraxx"),
            DowItem.AddMap4p("ts", "Сель", "Torrents"),
            DowItem.AddMap4p("ghc", "Кратер Гор Хаэль", "Gor'Hael Crater"),
            DowItem.AddMap4p("if", "Ледоход", "Ice Flow"),
            
            DowItem.AddMap6p("as", "Альварус", "Alvarus"),
            DowItem.AddMap6p("ms", "Морталис", "Mortalis"),
            DowItem.AddMap6p("pe", "Пармениэ", "Parmenie"),
            DowItem.AddMap6p("tm", "Таргорум", "Thargorum"),
            DowItem.AddMap6p("fi", "Остров Ярости", "Fury Island"),
            DowItem.AddMap6p("op", "Ориестанские равнины", "Oriestan Plains"),
            
            DowItem.AddMap8p("oos", "Оазис Шарра", "Oasis of Sharr"),
            DowItem.AddMap8p("me", "Монсэ", "Monse"),
            DowItem.AddMap8p("ca", "Керулеа", "Cerulea"),
            DowItem.AddMap8p("bg", "Могильник", "Burial Grounds"),
            DowItem.AddMap8p("jl", "Низины Ялаганда", "Jalaganda Lowlands"),
        };
    }
}
