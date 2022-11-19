void StartMenu()
{
    Console.WriteLine("1 - Odradi trening \n2 - Odigraj utakmicu" +
        "\n3 - Statistika \n4 - Kontrola igraca \n0 - Izlaz iz aplikacije");

    var myOptions = new List<string>() { "0", "1", "2", "3", "4" };
    var myChoice = Console.ReadLine().Trim();
    myChoice = Input(myChoice, myOptions);

    Console.Clear();
    switch (myChoice)
    {
        case "0":
            Environment.Exit(0);
            break;
        case "1":
            Training();
            break;
        case "2":
            PlayAMatch();
            break;
        case "3":
            Statistics();
            break;
        case "4":
            PlayerCheckup();
            break;
    };
}
string Input(string choice, List<string> options)
{
    while (!options.Contains(choice))
    {
        Console.WriteLine("Ta opcija u izborniku ne postoji, pokušajte ponovno!");
        choice = Console.ReadLine().Trim().ToUpper();
    }
    return choice;
}
void ReturnToStartMenu()
{
    Console.WriteLine("\nP - Povratak na glavni izbornik");
    if (Console.ReadLine().Trim().ToUpper() == "P")
    {
        Console.Clear();
        StartMenu();
    }
}

//___1 - TRAINING______________________________________________________________________________________________
void Training()
{
    Console.WriteLine("Igrači su uspješno odradili trening:\n");

    var rnd = new Random();
    int percentage, newRating, oldRating;

    foreach (var player in Global.playersDict.Keys)
    {
        percentage = rnd.Next(-5, 6); // minus if the player loses percentage, plus if they gain percentage
        oldRating = Global.playersDict[player].rating;
        newRating = (100 + percentage) * oldRating / 100;
        Global.playersDict[player] = (Global.playersDict[player].position, newRating);
        Console.WriteLine($"{player} - stari rating: {oldRating}, novi rating: {newRating}");
    }

    ReturnToStartMenu();
}

//___2 - PLAY A MATCH________________________________________________________________________________________________
void PlayAMatch()
{
    var bestPlayers = ChooseBestPlayers();

    if (Global.scoresByMatches.Count() == 6)
        Console.WriteLine("Već ste odigrali sve utakmice u skupini!");
    else if (bestPlayers.Count < 11)
        Console.WriteLine("Nemate dovoljno igraca za utakmicu!");
    else
    {
        var rnd = new Random();
        var score1 = rnd.Next(0, 6); //suppose that each team can score 0-5 goals in a single match
        var score2 = rnd.Next(0, 6);
        var score3 = rnd.Next(0, 6);
        var score4 = rnd.Next(0, 6);

        if (!Global.scoresByMatches.ContainsKey((teams.Hrvatska.ToString(), teams.Maroko.ToString()))) // 1.round
        {
            Global.scoresByMatches.Add((teams.Hrvatska.ToString(), teams.Maroko.ToString()), (score1, score2));
            Global.scoresByMatches.Add((teams.Belgija.ToString(), teams.Kanada.ToString()), (score3, score4));
            Console.WriteLine($"Svi rezultati prvog kola:\n\n{teams.Hrvatska} - {teams.Maroko} : {score1} - {score2}" +
                $"\n{teams.Belgija} - {teams.Kanada} : {score3} - {score4}");
        }
        else if (!Global.scoresByMatches.ContainsKey((teams.Hrvatska.ToString(), teams.Kanada.ToString()))) // 2.round
        {
            Global.scoresByMatches.Add((teams.Hrvatska.ToString(), teams.Kanada.ToString()), (score1, score2));
            Global.scoresByMatches.Add((teams.Belgija.ToString(), teams.Maroko.ToString()), (score3, score4));
            Console.WriteLine($"Svi rezultati drugog kola:\n\n{teams.Hrvatska} - {teams.Kanada} : {score1} - {score2}" +
                $"\n{teams.Belgija} - {teams.Maroko} : {score3} - {score4}");
        }
        else // 3.round
        {
            Global.scoresByMatches.Add((teams.Hrvatska.ToString(), teams.Belgija.ToString()), (score1, score2));
            Global.scoresByMatches.Add((teams.Kanada.ToString(), teams.Maroko.ToString()), (score3, score4));
            Console.WriteLine($"Svi rezultati treceg kola:\n\n{teams.Hrvatska} - {teams.Belgija} : {score1} - {score2}" +
                $"\n{teams.Kanada} - {teams.Maroko} : {score3} - {score4}");
        }

        GetAndUpdateStrikers(score1, bestPlayers, rnd);
        UpdatePlayersRatings(bestPlayers, score1, score2);
    }
    ReturnToStartMenu();
}
List<string> ChooseBestPlayers()
{
    var bestPlayers = new List<string>();
    var playersDictSorted = Global.playersDict.OrderByDescending(player => player.Value.rating);
    int gk = 0, df = 0, mf = 0, fw = 0;

    foreach (var player in playersDictSorted)
    {
        if (player.Value.position == "GK" && gk < 1) //add the first (best) player with position GK
        {
            bestPlayers.Add(player.Key);
            gk++;
        }
        else if (player.Value.position == "DF" && df < 4) //add players with position DF until you have 4 of them
        {
            bestPlayers.Add(player.Key);
            df++;
        }
        else if (player.Value.position == "MF" && mf < 3) //add players with position MF until you have 3 of them
        {
            bestPlayers.Add(player.Key);
            mf++;
        }
        else if (player.Value.position == "FW" && fw < 3) //add players with position FW until you have 3 of them
        {
            bestPlayers.Add(player.Key);
            fw++;
        }

        if (bestPlayers.Count == 11)
            break; // stop if you already have 11 players in the list,that way you don't have
                   // to go through the rest of the dict unnecessarily
    }
    return bestPlayers;
}
void GetAndUpdateStrikers(int score, List<string> bestPlayers, Random rnd)
{
    Console.WriteLine("\nRaspored zabijanja golova tijekom utakmice:\n");

    int index, newRating;
    string striker;
    for (var i = 0; i < score; i++)
    {
        index = rnd.Next(0, 11); //choose the index of a player that will be the i-th striker
        striker = bestPlayers[index];
        newRating = 105 * Global.playersDict[striker].rating / 100;
        Console.WriteLine($"{i + 1}. {striker}, stari rating: {Global.playersDict[striker].rating}, novi rating: {newRating}");
        Global.playersDict[striker] = (Global.playersDict[striker].position, newRating); // increase the strikers rating by 5%

        if (Global.strikersDict.ContainsKey(striker)) //add the striker to the list of strikers or update his number of goals
            Global.strikersDict[striker] += 1;
        else
            Global.strikersDict.Add(striker, 1);
    }
}
void UpdatePlayersRatings(List<string> bestPlayers, int score1, int score2)
{
    Console.WriteLine("\nRating igraca nakon utakmice:\n");

    int newRating;
    if (score1 > score2) // if our team won
    {
        foreach (var player in bestPlayers)
        {
            newRating = 102 * Global.playersDict[player].rating / 100;
            Console.WriteLine($"{player}, stari rating: {Global.playersDict[player].rating}, novi rating: {newRating}");
            Global.playersDict[player] = (Global.playersDict[player].position, newRating);
        }
    }
    else if (score1 < score2) //if our team lost
    {
        foreach (var player in bestPlayers)
        {
            newRating = 98 * Global.playersDict[player].rating / 100;
            Console.WriteLine($"{player}, stari rating: {Global.playersDict[player].rating}, novi rating: {newRating}");
            Global.playersDict[player] = (Global.playersDict[player].position, newRating);
        }
    }
}

//___3 - STATISTICS__________________________________________________________________________________________________
void Statistics()
{
    Console.WriteLine("1 - Ispis svih igraca onako kako su spremljeni \n2 - Ispis svih igraca po ratingu uzlazno" +
        "\n3 - Ispis svih igraca po ratingu silazno \n4 - Ispis igraca po imenu i prezimenu" +
        "\n5 - Ispis igraca po ratingu \n6 - Ispis igraca po poziciji \n7 - Ispis trenutnih prvih 11 igraca" +
        "\n8 - Ispis strijelaca i koliko golova imaju \n9 - Ispis svih rezultata ekipe " +
        "\n10 - Ispis rezultata svih ekipa \n11 - Ispis tablice grupe \nP - Povratak na glavni izbornik");

    var myOptions = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "P" };
    var myChoice = Console.ReadLine().Trim().ToUpper();
    myChoice = Input(myChoice, myOptions);

    Console.Clear();
    switch (myChoice)
    {
        case "1":
            PrintInOrderAsSaved();
            break;
        case "2":
            PrintAscending();
            break;
        case "3":
            PrintDescending();
            break;
        case "4":
            PrintByNameAndSurname();
            break;
        case "5":
            PrintByRating();
            break;
        case "6":
            PrintByPosition();
            break;
        case "7":
            PrintBestPlayers();
            break;
        case "8":
            PrintStrikers();
            break;
        case "9":
            PrintMyTeamResults();
            break;
        case "10":
            PrintAllTeamsResults();
            break;
        case "11":
            PrintTable();
            break;
        case "P":
            StartMenu();
            break;
    }
}
void PrintInOrderAsSaved()
{
    Console.WriteLine("Ispis igraca onako kako su spremljeni:\n");

    foreach (var player in Global.playersDict)
        Console.WriteLine($"{player.Key}, pozicija: {player.Value.position}, rating: {player.Value.rating}");

    ReturnToStartMenu();
}
void PrintAscending()
{
    Console.WriteLine("Ispis igraca po ratingu uzlazno:\n");

    var newplayersDict = Global.playersDict.OrderBy(player => player.Value.rating);

    foreach (var player in newplayersDict)
        Console.WriteLine($"{player.Key}, pozicija: {player.Value.position}, rating: {player.Value.rating}");

    ReturnToStartMenu();
}
void PrintDescending()
{
    Console.WriteLine("Ispis igraca po ratingu silazno:\n");

    var newplayersDict = Global.playersDict.OrderByDescending(player => player.Value.rating);

    foreach (var player in newplayersDict)
        Console.WriteLine($"{player.Key}, pozicija: {player.Value.position}, rating: {player.Value.rating}");

    ReturnToStartMenu();
}
void PrintByNameAndSurname()
{
    Console.WriteLine("Unesite ime igraca cija vas pozicija i rating zanimaju: ");
    var name = InputName();
    Console.WriteLine("Unesite prezime igraca cija vas pozicija i rating zanimaju:");
    var surname = InputName();

    if (!Global.playersDict.Keys.Contains(name + " " + surname))
        Console.WriteLine("\nTrazeni igrac ne nalazi se na popisu!");
    else
        Console.WriteLine($"\n{name + " " + surname}, pozicija: {Global.playersDict[name + " " + surname].position}, " +
            $"rating: {Global.playersDict[name + " " + surname].rating}");

    ReturnToStartMenu();
}
void PrintByRating()
{
    var rating = InputRating();
    Console.WriteLine("\nIgraci s trazenim ratingom:\n");

    foreach (var player in Global.playersDict)
    {
        if (player.Value.rating == rating)
            Console.WriteLine($"{player.Key}, pozicija: {player.Value.position}, rating: {player.Value.rating}");
    }
    ReturnToStartMenu();
}
void PrintByPosition()
{
    var position = InputPosition();
    Console.WriteLine("\nIgraci s trazenom pozicijom:\n");

    foreach (var player in Global.playersDict)
    {
        if (player.Value.position == position)
            Console.WriteLine($"{player.Key}, pozicija: {player.Value.position}, rating: {player.Value.rating}");
    }
    ReturnToStartMenu();
}
void PrintBestPlayers()
{
    var bestPlayers = ChooseBestPlayers();

    if (bestPlayers.Count < 11)
        Console.WriteLine("Nema dovoljno igraca na svim pozicijama!");
    else
    {
        Console.WriteLine("Najboljih 11 igraca:\n");
        foreach (var player in bestPlayers)
            Console.WriteLine($"{player}, pozicija: {Global.playersDict[player].position}, rating: {Global.playersDict[player].rating}");
    }
    ReturnToStartMenu();
}
void PrintStrikers()
{
    Console.WriteLine("Strijelci i njihov broj zabijenih golova:\n");

    foreach (var player in Global.strikersDict)
        Console.WriteLine($"{player.Key}: {player.Value}");

    ReturnToStartMenu();
}
void PrintMyTeamResults()
{
    Console.WriteLine("Ispis svih rezultata ekipe:\n");

    foreach (var match in Global.scoresByMatches.Keys)
    {
        if (match.team1 == teams.Hrvatska.ToString() || match.team2 == teams.Hrvatska.ToString())
            Console.WriteLine($"{match.team1} - {match.team2} : {Global.scoresByMatches[match].score1} - {Global.scoresByMatches[match].score2}");
    }

    ReturnToStartMenu();
}
void PrintAllTeamsResults()
{
    Console.WriteLine("Ispis rezultata svih ekipa:\n");

    foreach (var match in Global.scoresByMatches.Keys)
    {
        Console.WriteLine($"{match.team1} - {match.team2} : {Global.scoresByMatches[match].score1} - {Global.scoresByMatches[match].score2}");
    }

    ReturnToStartMenu();
}
void PrintTable()
{
    var myTable = new Dictionary<string, (int points, int difference)>()
    {
        {teams.Hrvatska.ToString(), (0,0)},
        {teams.Kanada.ToString(), (0,0)},
        {teams.Maroko.ToString(), (0,0)},
        {teams.Belgija.ToString(), (0,0)}
    };
    string team1, team2;
    int currentDifference;

    foreach (var match in Global.scoresByMatches)
    {
        team1 = match.Key.team1;
        team2 = match.Key.team2;

        currentDifference = match.Value.score1 - match.Value.score2;

        if (currentDifference > 0) // if team1 won
        {
            myTable[team1] = (myTable[team1].points + 3, myTable[team1].difference + currentDifference);
            myTable[team2] = (myTable[team2].points, myTable[team2].difference - currentDifference);
        }
        else if (currentDifference < 0)
        {
            myTable[team2] = (myTable[team2].points + 3, myTable[team2].difference - currentDifference);
            myTable[team1] = (myTable[team1].points, myTable[team1].difference + currentDifference);
        }
        else
        {
            myTable[team1] = (myTable[team1].points + 1, myTable[team1].difference);
            myTable[team2] = (myTable[team2].points + 1, myTable[team2].difference);
        }
    }
    var orderedTable = myTable.OrderByDescending(team => team.Value.points).ThenByDescending(team => team.Value.difference);
    int i = 0;

    foreach (var team in orderedTable)
        Console.WriteLine($"{++i}. {team.Key}, bodovi: {team.Value.points}, gol razlika: {team.Value.difference}");

    ReturnToStartMenu();
}

//___4 - PLAYER CHECKUP_____________________________________________________________________________________________
void PlayerCheckup()
{
    Console.WriteLine("1 - Unos novog igraca \n2 - Brisanje igraca \n3 - Uredivanje igraca " +
        "\nP - Povratak na glavni izbornik");

    var myOptions = new List<string>() { "1", "2", "3", "P" };
    var myChoice = Console.ReadLine().Trim().ToUpper();
    myChoice = Input(myChoice, myOptions);

    Console.Clear();
    switch (myChoice)
    {
        case "1":
            AddPlayer();
            break;
        case "2":
            DeletePlayer();
            break;
        case "3":
            EditPlayer();
            break;
        case "P":
            StartMenu();
            break;
    }
}
bool ConfirmDialogue()
{
    Console.WriteLine("Y - Zelim \nN - Ne zelim");
    var potvrda = Console.ReadLine().Trim().ToUpper();
    while (potvrda != "Y" && potvrda != "N")
    {
        Console.WriteLine("Ta opcija ne postoji, unesite Y ili N:");
        potvrda = Console.ReadLine().Trim().ToUpper();
    }
    return potvrda == "Y";
}
void AddPlayer()
{
    if (Global.playersDict.Count == 26)
        Console.WriteLine("U ekipi vec ima 26 igraca!");
    else
    {
        Console.WriteLine("Unesite ime igraca kojeg zelite unijeti:");
        var name = InputName();
        Console.WriteLine("Unesite prezime igraca kojeg zelite unijeti:");
        var surname = InputName();

        if (Global.playersDict.Keys.Contains(name + " " + surname))
            Console.WriteLine("Igrac s tim imenom i prezimenom vec je na popisu!");
        else
        {
            var position = InputPosition();
            var rating = InputRating();

            Console.WriteLine($"Želite li unijeti igraca {name + " " + surname}, pozicija {position}," +
                $"rating {rating}?");
            if (ConfirmDialogue())
            {
                Global.playersDict.Add(name + " " + surname, (position, rating));
                Console.WriteLine("Igrac je uspjesno dodan!");
            }
            else
                Console.WriteLine("Radnja je zaustavljena!");
        }
    }

    ReturnToStartMenu();
}
string InputName()
{
    var name = Console.ReadLine().Trim();
    while (name == "")
    {
        Console.WriteLine("Ovo polje ne smije biti prazno, pokušajte ponovno!");
        name = Console.ReadLine().Trim();
    }

    name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
    return name;
}
string InputPosition()
{
    Console.WriteLine("Unesite poziciju igraca (GK, DF, MF ili FW):");
    var positionsList = new List<string>() { "GK", "DF", "MF", "FW" };
    var position = Console.ReadLine().Trim().ToUpper();
    while (!positionsList.Contains(position))
    {
        Console.WriteLine("Ta pozicija ne postoji, pokusajte ponovno!");
        position = Console.ReadLine().Trim().ToUpper();
    }
    return position;
}
int InputRating()
{
    Console.WriteLine("Unesite rating igraca (prirodni broj u rasponu od 1 do 100):");

    int rating;
    while (true)
    {
        try
        {
            rating = int.Parse(Console.ReadLine().Trim());
            if (rating < 1 || rating > 100)
                Console.WriteLine("Rating mora biti izmedu 1 i 100!");
            else
                break;
        }
        catch
        {
            Console.WriteLine("Rating mora biti neki prirodan broj od 1 do 100!");
        }
    }
    return rating;
}
void DeletePlayer()
{
    Console.WriteLine("1 - Brisanje igraca unosom imena i prezimena \nP - Povratak na glavni izbornik");

    var myChoice = Console.ReadLine().Trim().ToUpper();
    myChoice = Input(myChoice, new List<string>() { "1", "P" });

    Console.Clear();
    if (myChoice == "1")
        DeleteByNameAndSurname();
    else
        StartMenu();
}
void DeleteByNameAndSurname()
{
    Console.WriteLine("Unesite ime igraca kojeg zelite izbrisati:");
    var name = InputName();
    Console.WriteLine("Unesite prezime igraca kojeg zelize izbrisati:");
    var surname = InputName();

    if (!Global.playersDict.Keys.Contains(name + " " + surname))
        Console.WriteLine($"{name + " " + surname} se ne nalazi na popisu!");
    else
    {
        Console.WriteLine($"Jeste li sigurni da zelite izbrisati {name + " " + surname}?");
        if (ConfirmDialogue())
        {
            Console.WriteLine("Igrac uspjesno izbrisan!");
            Global.playersDict.Remove(name + " " + surname);
        }
        else
            Console.WriteLine("Radnja je zaustavljena!");
    }

    ReturnToStartMenu();
}
void EditPlayer()
{
    Console.WriteLine("1 - Uredi ime i prezime igraca \n2 - Uredi poziciju igraca" +
        "\n3 - Uredi rating igraca \nP - Povratak na glavni izbornik");

    var myOptions = new List<string>() { "1", "2", "3", "P" };
    var myChoice = Console.ReadLine().Trim().ToUpper();
    myChoice = Input(myChoice, myOptions);

    Console.Clear();
    switch (myChoice)
    {
        case "1":
            EditNameAndSurname();
            break;
        case "2":
            EditPosition();
            break;
        case "3":
            EditRating();
            break;
        case "P":
            StartMenu();
            break;
    }
}
void EditNameAndSurname()
{
    Console.WriteLine("Unesite ime igraca kojeg zelite urediti:");
    var name = InputName();
    Console.WriteLine("Unesite prezime igraca kojeg zelite urediti:");
    var surname = InputName();

    if (!Global.playersDict.Keys.Contains(name + " " + surname))
        Console.WriteLine($"{name + " " + surname} ne nalazi se na popisu!");
    else
    {
        Console.WriteLine("Unesite novo ime:");
        var newName = InputName();
        Console.WriteLine("Unesite novo prezime:");
        var newSurname = InputName();

        Console.WriteLine($"Jeste li sigurni da zelite igraca {name + " " + surname} " +
            $"promijeniti u {newName + " " + newSurname}?");
        if (ConfirmDialogue())
        {
            Global.playersDict.Add(newName + " " + newSurname,
                (Global.playersDict[name + " " + surname].position, Global.playersDict[name + " " + surname].rating));
            Global.playersDict.Remove(name + " " + surname);
            Console.WriteLine($"{name + " " + surname} uspjesno preimenovan u {newName + " " + newSurname}!");
        }
        else
            Console.WriteLine("Radnja je zaustavljena!");
    }

    ReturnToStartMenu();
}
void EditPosition()
{
    Console.WriteLine("Unesite ime igraca kojeg zelite urediti:");
    var name = InputName();
    Console.WriteLine("Unesite prezime igraca kojeg zelite urediti:");
    var surname = InputName();

    if (!Global.playersDict.Keys.Contains(name + " " + surname))
        Console.WriteLine($"{name + " " + surname} ne nalazi se na popisu!");
    else
    {
        var pozicija = InputPosition();
        if (pozicija == Global.playersDict[name + " " + surname].position)
            Console.WriteLine("Igrac vec ima tu poziciju!");
        else
        {
            Console.WriteLine($"Jeste li sigurni da igracu {name + " " + surname} zelite promijeniti " +
                $"poziciju iz {Global.playersDict[name + " " + surname].position} u {pozicija}?");
            if (ConfirmDialogue())
            {
                Global.playersDict[name + " " + surname] = (pozicija, Global.playersDict[name + " " + surname].rating);
                Console.WriteLine("Pozicija igraca uspjesno promijenjena!");
            }
            else
                Console.WriteLine("Radnja je zaustavljena!");
        }
    }

    ReturnToStartMenu();
}
void EditRating()
{
    Console.WriteLine("Unesite ime igraca kojeg zelite urediti:");
    var name = InputName();
    Console.WriteLine("Unesite prezime igraca kojeg zelite urediti:");
    var surname = InputName();

    if (!Global.playersDict.Keys.Contains(name + " " + surname))
        Console.WriteLine($"{name + " " + surname} ne nalazi se na popisu!");
    else
    {
        var rating = InputRating();
        if (rating == Global.playersDict[name + " " + surname].rating)
            Console.WriteLine("Igrac vec ima taj rating!");
        else
        {
            Console.WriteLine($"Jeste li sigurni da igracu {name + " " + surname} zelite promijeniti " +
                $"rating iz {Global.playersDict[name + " " + surname].rating} u {rating}?");
            if (ConfirmDialogue())
            {
                Global.playersDict[name + " " + surname] = (Global.playersDict[name + " " + surname].position, rating);
                Console.WriteLine("Rating igraca uspjesno promijenjen!");
            }
            else
                Console.WriteLine("Radnja je zaustavljena!");
        }
    }

    ReturnToStartMenu();
}
//_______________________________________________________________________________________________________________

StartMenu();

enum teams
{
    Hrvatska,
    Maroko,
    Belgija,
    Kanada
};
static class Global
{
    public static Dictionary<string, (string position, int rating)> playersDict = new Dictionary<string, (string position, int rating)>()
    {
        {"Luka Modric", ("MF",88) },
        {"Marcelo Brozovic", ("MF",86) },
        {"Mateo Kovacic", ("MF",84) },
        {"Ivan Perisic", ("MF",84) },
        {"Andrej Kramaric", ("FW",82) },
        {"Ivan Rakitic", ("MF",82) },
        {"Josko Gvardiol", ("DF",81) },
        {"Mario Pasalic", ("MF",81) },
        {"Lovro Majer", ("MF",80) },
        {"Dominik Livakovic", ("GK",80) },
        {"Ante Rebic", ("FW",80) },
        {"Josip Brekalo", ("MF",79) },
        {"Borna Sosa", ("DF",78) },
        {"Nikola Vlasic", ("MF",78) },
        {"Duje Caleta-Car", ("DF",78) },
        {"Dejan Lovren", ("DF",78) },
        {"Mislav Orsic", ("FW",77) },
        {"Marko Livaja", ("FW",77) },
        {"Domagoj Vida", ("DF",76) },
        {"Ante Budimir", ("FW",76) }
    };

    public static Dictionary<(string team1, string team2), (int score1, int score2)> scoresByMatches = new Dictionary<(string team1, string team2), (int score1, int score2)>();
    public static Dictionary<string, int> strikersDict = new Dictionary<string, int>();
}
