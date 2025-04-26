using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingdomController : LocatorFunctions
{
    private ElementManagement manager;
    public string kingdomName;
    public int ownSeed;
    public List<GameObject> resources = new List<GameObject>();
    public List<KingdomController> rivals = new List<KingdomController>();
    public List<KingdomController> allies = new List<KingdomController>();
    public int mineralForce;
    public int agriculture;
    public int animals;
    public int seaOpenings;
    public int foodSources;
    public KingdomType type;
    public KingdomType secondaryType;
    public Size size;
    public enum KingdomType
    {
        None,
        Agricultor,
        Ganadero,
        Minero,
        Cazador,
        Pesquero
    }
    public enum Size
    {
        Mediano,
        Pequeño,
        Grande
    }
    private int[] arr = { 0, 0, 0, 0, 0 };

    //Nombres
    private string[] AgricultorPrimario = { "Agra", "Terra", "Campa", "Ferta", "Grana", "Horto", "Semo", "Trigo", "Fructa", "Vinia", "Bera", "Prada", "Arva", "Culta", "Labra", "Maiza", "Cepea", "Pastu", "Rica", "Verdi", "Milla", "Espiga", "Cose", "Herba", "Plant", "Arada", "Raiza", "Semil", "Brota", "Huerta", "Forja", "Malez", "Brote", "Sembra", "Clava", "Rafo", "Calta", "Manto", "Tierra", "Paj", "Ester", "Cuvia", "Labor", "Pamp", "Talo", "Tende", "Uvil", "Hoja", "Zerda", "Erba" };
    private string[] GanaderoPrimario = { "Bovi", "Ovi", "Capra", "Vaca", "Corral", "Lana", "Pecu", "Tablo", "Toro", "Ceba", "Forra", "Cabri", "Albe", "Came", "Yegua", "Gansa", "Rusta", "Ganad", "Porci", "Mul", "Vaqui", "Torda", "Domes", "Prada", "Chiva", "Besti", "Buey", "Yunta", "Cruz", "Brida", "Tarso", "Cresta", "Granja", "Asta", "Juba", "Brama", "Verru", "Parda", "Carda", "Cerril", "Pezu", "Pel", "Rufa", "Corve", "Arreo", "Trilla", "Gor", "Coch", "Brami", "Cerda", "Guar" };
    private string[] MineroPrimario = { "Pico", "Mina", "Forja", "Ferro", "Cobre", "Bronz", "Carbo", "Roca", "Gema", "Crista", "Plata", "Oro", "Tallo", "Excav", "Greda", "Cuarzo", "Prof", "Abis", "Dur", "Peña", "Magna", "Filón", "Pulso", "Perfo", "Esmer", "Cali", "Dures", "Basal", "Yeso", "Obsi", "Lapi", "Crisol", "Grani", "Rubi", "Topa", "Puli", "Gleba", "Profu", "Mote", "Ace", "Mazo", "Brut", "Soca", "Corta", "Laja", "Grosa", "Extrac", "Galen", "Estra", "Fundia", "Molde" };
    private string[] CazadorPrimario = { "Arco", "Flecha", "Trampa", "Presa", "Ven", "Piel", "Astil", "Monte", "Silva", "Acecha", "Caza", "Jaba", "Guald", "Sigil", "Esp", "Huella", "Alce", "Diana", "Bosc", "Fiera", "Presu", "Zarpa", "Montar", "Cuerno", "Fauces", "Uña", "Perse", "Templa", "Filo", "Rasga", "Sombra", "Embos", "Certero", "Tiro", "Fuer", "Ale", "Acech", "Mira", "Esti", "Daga", "Lanza", "Filo", "Aguza", "Cuerva", "Redil", "Jaula", "Trof", "Cuela", "Marra", "Marru", "Troza" };
    private string[] PesqueroPrimario = { "Maris", "Naut", "Anzu", "Red", "Velam", "Nave", "Alga", "Brisa", "Isla", "Rema", "Cardu", "Sal", "Oleaj", "Boya", "Timon", "Cef", "Sirena", "Perla", "Banco", "Pez", "Rebo", "Corr", "Cangr", "Delfi", "Faro", "Raya", "Angui", "Espa", "Tramo", "Marea", "Golfo", "Riada", "Balsa", "Refu", "Brume", "Lancha", "Cayuco", "Boga", "Flota", "Ancla", "Esca", "Ribe", "Litor", "Rizo", "Capit", "Tuna", "Abis", "Sargaz", "Vapor", "Brami" };

    private string[] AgricultorSecundario = { "ver", "tal", "siem", "hor", "gra", "mies", "cose", "pra", "hoj", "tri", "bro", "pas", "ara", "vi", "ceba", "sur", "abo", "for", "fru", "ter", "fer", "pam", "sem", "her", "ra", "espi", "huer", "cam", "plan", "ter", "lab", "cos", "lad", "flo", "alc", "par", "cer", "veg", "cha", "cor", "hen", "bos", "cul", "esp", "pio", "cam", "tre", "tie", "cña", "fro" };
    private string[] GanaderoSecundario = { "est", "bas", "cañ", "lan", "vaq", "pas", "red", "for", "cer", "pra", "rod", "cri", "yug", "rec", "reb", "ceb", "gra", "gan", "cor", "jac", "cab", "mas", "mon", "mul", "fie", "car", "tro", "apa", "her", "mon", "yun", "har", "tau", "dom", "bar", "tri", "jal", "cue", "fin", "bra", "ras", "cab", "cen", "sem", "tar", "pia", "arr", "cab", "esp" };
    private string[] MineroSecundario = { "can", "fil", "vet", "cin", "for", "gal", "yes", "gra", "cri", "abi", "plo", "esc", "piz", "fun", "hie", "bas", "opa", "cua", "oxi", "cin", "esm", "est", "men", "lap", "ard", "cob", "ars", "mol", "tur", "jas", "peñ", "dol", "cin", "azu", "tob", "oni", "mar", "mag", "cri", "zin", "gra", "pir", "rio", "obs", "gne", "ara", "cro", "dio", "ant", "gal" };
    private string[] CazadorSecundario = { "ace", "col", "gar", "sen", "ast", "emb", "fur", "exp", "hue", "sig", "ven", "fau", "cue", "esc", "ace", "dag", "lan", "tro", "emb", "zar", "sil", "pun", "vel", "cor", "fle", "tra", "pre", "som", "cam", "ref", "mon", "ace", "arm", "zor", "jag", "ven", "rap", "cet", "exp", "sur", "bra", "bos", "ali", "fie", "mon", "ocu", "ace", "gua", "caz", "bat", "sig" };
    private string[] PesqueroSecundario = { "bri", "vien", "mar", "nau", "vel", "ole", "tim", "car", "baj", "sir", "are", "rio", "gol", "anc", "nie", "abi", "vap", "cal", "pes", "red", "bru", "nav", "mar", "cos", "isl", "tib", "ang", "rob", "est", "rib", "cef", "acu", "bal", "esc", "cor", "mar", "sal", "rem", "tif", "arr", "med", "cas", "dor", "sur", "pec", "acu", "tro", "per", "vap", "mar" };

    private string[] SufijosPequeños = { "dorf", "stead", "ham", "by", "ley", "wick", "holt", "mere", "stow", "den", "burn", "firth", "brook", "shaw", "croft", "glen", "ridge", "dale", "burrow", "thorp", "wold", " Hollow", "cliff", "wynd", "bluff", "latch", "haven", "moss", "tor" };
    private string[] SufijosMedianos = { "ton", "burg", " Field", "stead", " Fort", "ward", "gate", "harbor", "moor", "watch", "glade", "fens", "brink", " Strand", "marsh", "heath", "crag", "cove", "rill", "spur", "vault", "grove", "march", "run", "reach", "strong", "vale", "bough" };
    private string[] SufijosGrandes = { "holm", "grad", "keep", "hagen", "wald", "hold", "shire", "tide", "deep", "haven", " Citadel", " Tower"," Dome", " Citadel", " Stronghold", " Metropolis", " Empire", " Sovereign", " Capitol", "sanctum", " Sanctuary", " Kingdom", "Realm", " Province", " Palace" };

    public bool generate;

    void LateUpdate()
    {
        if (generate && manager != null)
        {
            if ((manager.NearestResource(transform.position).position - transform.position).magnitude <= 30)
            {
                Transform n = manager.NearestResource(transform.position);
                if (n.GetComponent<MeshFilter>() != null)
                {
                    Destroy(n.GetComponent<MeshFilter>());
                }
            }
            generate = false;
            GameObject kCity = Instantiate(manager.city, transform.position, Quaternion.identity);
            kCity.GetComponent<CityGenerator>().kingdomController = this;
            kCity.GetComponent<CityGenerator>().CityStart();
            kCity.transform.parent = transform;
            kCity.transform.localScale = Vector3.one * 0.025f;
            manager.cities.Add(kCity);
            kCity.GetComponent<CityGenerator>().AddaptHeights();
            if (GetComponent<MeshFilter>() != null)
            {
                Destroy(GetComponent<MeshFilter>());
            }
        }
    }

    public void SetManager(ElementManagement e)
    {
        manager = e;
    }

    public void SetType()
    {
        foreach(GameObject res in resources)
        {
            switch (res.tag)
            {
                case "EmptyField":
                    animals+=2;
                    agriculture++;
                    foodSources+=2;
                    arr[1]+=2;
                    break;
                case "FertileField":
                    agriculture += 3;
                    foodSources += 4;
                    arr[0]+=5;
                    break;
                case "ForestField":
                    animals+=2;
                    foodSources++;
                    arr[2]+=2;
                    break;
                case "SeaField":
                    foodSources += 3;
                    seaOpenings++;
                    arr[3]+=3;
                    break;
                case "SmallMine":
                    mineralForce++;
                    arr[4]+=5;
                    break;
                case "GreatMine":
                    mineralForce+=4;
                    arr[4] += 10;
                    break;
            }
        }
        int max = -1;
        int max2 = -1;
        int pos = -1;
        int pos2 = -1;
        for (int i=0; i < arr.Length; i++)
        {
            if (arr[i] >= max)
            {
                pos = i;
                max=arr[i];
            }
        }
        for (int i = 0; i < arr.Length; i++)
        {
            if (i == pos) continue;
            if (arr[i] >= max2)
            {
                pos2 = i;
                max2 = arr[i];
            }
        }
        switch (pos)
        {
            case 0:
                type = KingdomType.Agricultor;
                break;
            case 1:
                type = KingdomType.Ganadero;
                break;
            case 2:
                type = KingdomType.Cazador;
                break;
            case 3:
                type = KingdomType.Pesquero;
                break;
            case 4:
                type = KingdomType.Minero;
                break;
        }
        switch (pos2)
        {
            case 0:
                secondaryType = KingdomType.Agricultor;
                break;
            case 1:
                secondaryType = KingdomType.Ganadero;
                break;
            case 2:
                secondaryType = KingdomType.Cazador;
                break;
            case 3:
                secondaryType = KingdomType.Pesquero;
                break;
            case 4:
                secondaryType = KingdomType.Minero;
                break;
        }
        GetName();
    }

    public void SetRelations()
    {
        foreach (GameObject res in resources)
        {
            if (res.GetComponent<ResourceInfo>().interestedKingdoms.Count > 1)
            {
                List<KingdomController> aux = new List<KingdomController>(res.GetComponent<ResourceInfo>().interestedKingdoms);
                aux.RemoveAll(x => x == this);
                foreach(KingdomController k in aux)
                {
                    switch (res.tag)
                    {
                        case "EmptyField":
                            if((Math.Abs(animals-k.animals)<3 && Math.Abs(agriculture - k.agriculture) < 3) || 
                                (type==KingdomType.Ganadero && k.type==KingdomType.Ganadero) || (secondaryType == KingdomType.Ganadero && k.secondaryType == KingdomType.Ganadero) ||
                                (type == KingdomType.Agricultor && k.type == KingdomType.Agricultor) || (secondaryType == KingdomType.Agricultor && k.secondaryType == KingdomType.Agricultor))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "FertileField":
                            if (Math.Abs(agriculture - k.agriculture) < 3 ||
                                (type == KingdomType.Agricultor && k.type == KingdomType.Agricultor) || (secondaryType == KingdomType.Agricultor && k.secondaryType == KingdomType.Agricultor))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "ForestField":
                            if (Math.Abs(animals - k.animals) < 3 ||
                                (((type == KingdomType.Cazador && k.type != KingdomType.Cazador)|| (secondaryType == KingdomType.Cazador && k.secondaryType != KingdomType.Cazador)) && Math.Abs(foodSources - k.foodSources) < 5))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "SeaField":
                            if (Math.Abs(foodSources - k.foodSources) < 5 ||
                                (type == KingdomType.Pesquero && k.type == KingdomType.Pesquero) || (secondaryType == KingdomType.Pesquero && k.secondaryType == KingdomType.Pesquero))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "SmallMine":
                            if ((type != KingdomType.Minero && k.type != KingdomType.Minero)|| (secondaryType != KingdomType.Minero && k.secondaryType != KingdomType.Minero))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "GreatMine":
                            rivals.Add(k);
                            break;
                    }
                }
            }
        }
    }

    public void GetName()
    {
        string p1="";
        switch (type)
        {
            case KingdomType.Agricultor:
                p1 = AgricultorPrimario[UnityEngine.Random.Range(0, AgricultorPrimario.Length)];
                break;
            case KingdomType.Ganadero:
                p1 = GanaderoPrimario[UnityEngine.Random.Range(0, GanaderoPrimario.Length)];
                break;
            case KingdomType.Minero:
                p1 = MineroPrimario[UnityEngine.Random.Range(0, MineroPrimario.Length)];
                break;
            case KingdomType.Cazador:
                p1 = CazadorPrimario[UnityEngine.Random.Range(0, CazadorPrimario.Length)];
                break;
            case KingdomType.Pesquero:
                p1 = PesqueroPrimario[UnityEngine.Random.Range(0, PesqueroPrimario.Length)];
                break;
        }
        string p2="";
        switch (secondaryType)
        {
            case KingdomType.Agricultor:
                p2 = AgricultorSecundario[UnityEngine.Random.Range(0, AgricultorSecundario.Length)];
                break;
            case KingdomType.Ganadero:
                p2 = GanaderoSecundario[UnityEngine.Random.Range(0, GanaderoSecundario.Length)];
                break;
            case KingdomType.Minero:
                p2 = MineroSecundario[UnityEngine.Random.Range(0, MineroSecundario.Length)];
                break;
            case KingdomType.Cazador:
                p2 = CazadorSecundario[UnityEngine.Random.Range(0, CazadorSecundario.Length)];
                break;
            case KingdomType.Pesquero:
                p2 = PesqueroSecundario[UnityEngine.Random.Range(0, PesqueroSecundario.Length)];
                break;
        }
        string p3="";
        if(UnityEngine.Random.Range(0,1f)<0.5)
        {
            switch (size)
            {
                case Size.Mediano:
                    p3 = SufijosMedianos[UnityEngine.Random.Range(0, SufijosMedianos.Length)];
                    break;
                case Size.Grande:
                    p3 = SufijosGrandes[UnityEngine.Random.Range(0, SufijosGrandes.Length)];
                    break;
                case Size.Pequeño:
                    p3 = SufijosPequeños[UnityEngine.Random.Range(0, SufijosPequeños.Length)];
                    break;
            }
        }

        kingdomName = p1 + p2 + p3;
        ownSeed= UnityEngine.Random.Range(10000000, 99999999);
    }

    public void SetRelationLines()
    {
        if (rivals.Count > 0)
        {
            foreach(KingdomController k in rivals)
            {
                GameObject a = new GameObject("RivalLine");
                a.transform.position = transform.position;
                a.transform.SetParent(transform);
                LineRenderer rLine = a.AddComponent<LineRenderer>();
                rLine.positionCount = 4;
                rLine.SetPosition(0, transform.position);
                rLine.SetPosition(1, new Vector3(transform.position.x, 36, transform.position.z));
                rLine.SetPosition(2, new Vector3(k.transform.position.x, 36, k.transform.position.z));
                rLine.SetPosition(3, k.transform.position);
                rLine.startWidth=2.5f;
                rLine.endWidth=2.5f;
                rLine.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(1, 0, 0, 1f) };
                rLine.sortingOrder = 11;
            }
        }
        if (allies.Count > 0)
        {
            foreach (KingdomController k in allies)
            {
                GameObject a = new GameObject("AllyLine");
                a.transform.position = transform.position;
                a.transform.SetParent(transform);
                LineRenderer rLine = a.AddComponent<LineRenderer>();
                rLine.positionCount = 4;
                rLine.SetPosition(0, transform.position);
                rLine.SetPosition(1, new Vector3(transform.position.x, 15, transform.position.z));
                rLine.SetPosition(2, new Vector3(k.transform.position.x, 15, k.transform.position.z));
                rLine.SetPosition(3, k.transform.position);
                rLine.startWidth = 1.5f;
                rLine.endWidth = 1.5f;
                rLine.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(0,0,1,1f) };
            }
        }
    }
}
