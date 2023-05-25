namespace Server.Repositories;

public class PlanetNameRepository : IPlanetNameRepository
{
    private readonly List<string> _planetNames;
    public PlanetNameRepository()
    {
        _planetNames = GetInMemoryNames();
    }

    private List<string> GetInMemoryNames()
    {
        return new List<string>()
        {
            "Aetheria",
            "Novaria",
            "Celestria",
            "Zephyria",
            "Solara",
            "Stellara",
            "Elysium",
            "Galaxia",
            "Aurora",
            "Lunaria",
            "Avalon",
            "Vespera",
            "Seraphina",
            "Nebulia",
            "Orion",
            "Aquila",
            "Terra",
            "Maris",
            "Ignis",
            "Ventus",
            "Lux",
            "Umbral",
            "Veridian",
            "Caelum",
            "Hydria",
            "Astralis",
            "Aria",
            "Luminara",
            "Stellaris",
            "Solstice",
            "Nocturna",
            "Crescentia",
            "Aether",
            "Celestialis",
            "Aurum",
            "Sylvana",
            "Eridani",
            "Gaia",
            "Arcana",
            "Mystara",
            "Helios",
            "Nyx",
            "Lumen",
            "Polaris",
            "Nebula",
            "Asteria",
            "Avalonia",
            "Bellatrix",
            "Elysia",
            "Halcyon",
            "Lyra",
            "Meridia",
            "Neptuna",
            "Novalis",
            "Olympia",
            "Pandora",
            "Rigel",
            "Serenity",
            "Triton",
            "Vela",
            "Zara",
            "Andromeda",
            "Artemis",
            "Astraea",
            "Cassiopeia",
            "Dione",
            "Electra",
            "Feronia",
            "Galatea",
            "Hesperia",
            "Iris",
            "Juno",
            "Kalypso",
            "Lysandra",
            "Maia",
            "Naida",
            "Ophelia",
            "Phoebe",
            "Quintessa",
            "Rhea",
            "Selene",
            "Thalassa",
            "Urania",
            "Venusia",
            "Wisteria",
            "Xanthe",
            "Yara",
            "Zephyrine",
            "Adrastea",
            "Bellerophon",
            "Cerelia",
            "Diantha",
            "Evanthe",
            "Freya",
            "Gaiana",
            "Hestia",
            "Ianthe",
            "Juniper",
            "Kallisto",
            "Larissa",
            "Melaina",
            "Nemesis",
            "Oriana",
            "Pallas",
            "Quirina",
            "Roxana",
            "Sapphira",
            "Talia",
            "Vesper",
            "Zinnia"
        };
    }

    public ICollection<string> PlanetNames
    {
        get => _planetNames;
    }
}