using UnityEngine;
using System.Collections.Generic;

public enum Gender
{
    Male,
    Female
}

public static class NameGenerator
{
    // ---------- MALE FIRST NAMES (40) ----------
    private static readonly string[] maleFirstNames =
    {
        "Liam","Noah","Ethan","Mason","Logan","Lucas","Jackson","Aiden","Oliver","Jacob",
        "Elijah","Alexander","James","Benjamin","Daniel","Henry","Sebastian","Jack","Owen","Samuel",
        "Matthew","Joseph","David","Carter","Wyatt","Jayden","John","Luke","Gabriel","Isaac",
        "Anthony","Dylan","Leo","Lincoln","Julian","Hudson","Grayson","Levi","Nathan","Aaron"
    };

    // ---------- FEMALE FIRST NAMES (40) ----------
    private static readonly string[] femaleFirstNames =
    {
        "Olivia","Emma","Ava","Sophia","Isabella","Mia","Charlotte","Amelia","Harper","Evelyn",
        "Abigail","Emily","Ella","Elizabeth","Camila","Luna","Sofia","Avery","Mila","Aria",
        "Scarlett","Penelope","Layla","Chloe","Victoria","Madison","Eleanor","Grace","Nora","Riley",
        "Zoey","Hannah","Hazel","Lily","Ellie","Violet","Lillian","Zoe","Stella","Aurora"
    };

    // ---------- LAST NAMES (50) ----------
    private static readonly string[] lastNames =
    {
        "Smith","Johnson","Williams","Brown","Jones","Garcia","Miller","Davis","Rodriguez","Martinez",
        "Hernandez","Lopez","Gonzalez","Wilson","Anderson","Thomas","Taylor","Moore","Jackson","Martin",
        "Lee","Perez","Thompson","White","Harris","Sanchez","Clark","Ramirez","Lewis","Robinson",
        "Walker","Young","Allen","King","Wright","Scott","Torres","Nguyen","Hill","Flores",
        "Green","Adams","Nelson","Baker","Hall","Rivera","Campbell","Mitchell","Carter","Roberts"
    };

    // Optional: prevent duplicates during runtime
    private static HashSet<string> usedNames = new HashSet<string>();

    public static string GenerateName(Gender gender, bool ensureUnique = false)
    {
        string firstName = gender == Gender.Male
            ? maleFirstNames[Random.Range(0, maleFirstNames.Length)]
            : femaleFirstNames[Random.Range(0, femaleFirstNames.Length)];

        string lastName = lastNames[Random.Range(0, lastNames.Length)];

        string fullName = firstName + " " + lastName;

        if (!ensureUnique)
            return fullName;

        // Try to ensure uniqueness
        int attempts = 0;
        while (usedNames.Contains(fullName) && attempts < 100)
        {
            firstName = gender == Gender.Male
                ? maleFirstNames[Random.Range(0, maleFirstNames.Length)]
                : femaleFirstNames[Random.Range(0, femaleFirstNames.Length)];

            lastName = lastNames[Random.Range(0, lastNames.Length)];

            fullName = firstName + " " + lastName;
            attempts++;
        }

        usedNames.Add(fullName);
        return fullName;
    }

    public static void ResetUsedNames()
    {
        usedNames.Clear();
    }
}