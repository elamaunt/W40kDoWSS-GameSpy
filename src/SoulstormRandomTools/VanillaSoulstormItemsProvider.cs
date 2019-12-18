using SoulstormRandomTools.Types;

namespace SoulstormRandomTools
{
    public class VanillaSoulstormItemsProvider : ISoulstormItemsProvider
    {
        public SoulstormItem[] Races { get; } = new SoulstormItem[]
        {
            SoulstormItem.NewRace("csm", "Хаос", "Chaos"),
            SoulstormItem.NewRace("de", "Темные эльдары", "Dark Eldars"),
            SoulstormItem.NewRace("eld", "Эльдары", "Eldar"),
            SoulstormItem.NewRace("ig", "Имперская гвардия", "Imperial Guard"),
            SoulstormItem.NewRace("nec", "Некроны", "Necrons"),
            SoulstormItem.NewRace("ork", "Орки", "Orks"),
            SoulstormItem.NewRace("sm", "Космодесант", "Space Marines"),
            SoulstormItem.NewRace("sob", "Сестры битвы", "Sisters of Battle"),
            SoulstormItem.NewRace("tau", "Тау", "Tau")
        };

        public SoulstormItem[] Maps { get; } = new SoulstormItem[]
        {
            SoulstormItem.NewMap("bm", "Битва в болотах", "Battle Marshes"),
            SoulstormItem.NewMap("br", "Кровавая река", "Blood River"),
            SoulstormItem.NewMap("er", "Изумудруная река", "Emerald River"),
            SoulstormItem.NewMap("fc", "Мертвый город", "Fallen City"),
            SoulstormItem.NewMap("fd", "Отречение Фразира", "Frazier's Demis"),
            SoulstormItem.NewMap("fm", "Фата Морга", "Fata Morgana"),
            SoulstormItem.NewMap("mom", "Встреча мудрых", "Meetings of Minds"),
            SoulstormItem.NewMap("or", "Внешние пределы", "Outer Reaches"),
            SoulstormItem.NewMap("qt", "Триумф Квеста", "Quest's Triumph"),
            SoulstormItem.NewMap("soe", "Храм Экселлиона", "Shrine of Excellion"),
            SoulstormItem.NewMap("te", "Конец покоя", "Tranquilitys End"),
            SoulstormItem.NewMap("tf", "Падение Титана", "Titan's Fall"),
            SoulstormItem.NewMap("vp", "Вихревое плато", "Vortex Plateau")
        };
    }
}
