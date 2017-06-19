using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;


namespace StadiumBot2
{
    class Program
    {
        static Byte[] oldboxstring = new Byte[0x80];
        static Random rand = new Random();

        static void Main(string[] args)
        {
            UInt32 input1;
            UInt32 input2;
            UInt32 cycle = 0;
            UInt32 player = 1;
            UInt32 move1 = 0; //0 undecided; 1-4 moves1-4; 5-7 pokemon1-3
            UInt32 move2 = 0;

            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i] == "-ip")
                {
                    Project64Watch.host = args[i + 1];
                }

                if (args[i] == "-p")
                {
                    Project64Watch.port = Convert.ToInt32(args[i + 1]);
                }
            }

            Project64Watch.Connect();
            while (true)
            {
                input1 = 0;
                input2 = 0;

                switch (Project64Watch.Read8(0x90CC3))
                {
                    case 0x00://Intro
                    case 0x01://Title
                        {
                            player = 1;
                            move1 = 0;
                            move2 = 0;

                            Project64Watch.SetInput(ref input1, "A");
                        }
                        break;

                    case 0x02://Menus
                        {
                            switch (Project64Watch.Read8(0x9031B))
                            {
                                case 0x00://Game Pak Check
                                case 0x33://Please Select
                                    {
                                        Project64Watch.SetInput(ref input1, "A");
                                    }
                                    break;

                                case 0x37://Hub World
                                    {
                                        switch (Project64Watch.Read8(0xA9EF7))
                                        {
                                            case 0xA7://Stadium
                                                {
                                                    Project64Watch.SetInput(ref input1, "DPad R");
                                                }
                                                break;
                                            case 0xEE://Pokémon Academy
                                                {
                                                    Project64Watch.SetInput(ref input1, "DPad U");
                                                }
                                                break;
                                            case 0xC5://Free Battle
                                                {
                                                    Project64Watch.SetInput(ref input1, "A");
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;

                    case 0x07://Free Battle Setup
                        {
                            switch (Project64Watch.Read8(0xAA6F3))
                            {
                                case 0x0C://1P
                                    {
                                        Project64Watch.SetInput(ref input1, "A");
                                    }
                                    break;

                                case 0x7C:
                                    {
                                        switch (Project64Watch.Read8(0xAA6A7))
                                        {
                                            case 0x76://COM
                                                {
                                                    Project64Watch.SetInput(ref input1, "DPad D");
                                                }
                                                break;

                                            case 0x94://2P
                                                {
                                                    Project64Watch.SetInput(ref input1, "A");
                                                }
                                                break;
                                        }
                                    }
                                    break;

                                case 0xC5://Free Battle
                                    {
                                        Project64Watch.SetInput(ref input1, "DPad U");
                                    }
                                    break;

                                case 0xAB://Random
                                    {
                                        Project64Watch.SetInput(ref input1, "A");
                                    }
                                    break;

                                default:
                                    {
                                        switch (Project64Watch.Read8(0xAA8DB))
                                        {
                                            case 0x00://Choose Entry
                                            case 0x10://Anything Goes
                                                {
                                                    if (player == 1)
                                                    {
                                                        if (Project64Watch.Read8(0x146240) == 0x00)//No 1P Team
                                                        {
                                                            Project64Watch.SetInput(ref input1, "A");
                                                        }
                                                        else
                                                        {
                                                            player = 2;
                                                        }
                                                    }
                                                    else if (Project64Watch.Read8(0x146480) == 0x00)//No 2P Team
                                                    {
                                                        Project64Watch.SetInput(ref input2, "A");
                                                    }
                                                    else
                                                    {
                                                        InjectTeams();
                                                    }
                                                }
                                                break;

                                            case 0x02://OK
                                                {
                                                    Project64Watch.SetInput(ref input1, "A");
                                                    Project64Watch.SetInput(ref input2, "A");
                                                }
                                                break;

                                            case 0x26://Team 2
                                                {
                                                    Project64Watch.SetInput(ref input2, "A");
                                                }
                                                break;

                                            case 0x80://Team 1
                                                {
                                                    if (player == 1)
                                                    {
                                                        Project64Watch.SetInput(ref input1, "A");
                                                    }
                                                    else
                                                    {
                                                        Project64Watch.SetInput(ref input2, "DPad D");
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;

                    case 0x08://In Battle
                        {
                            ReadString();
                            if (Project64Watch.Read8(0x91B7C) == 0x01)//Waiting
                            {
                                if (move1 == 0x00)
                                {
                                    PickMove(1, ref move1);
                                }
                                else
                                {
                                    DoMove(1, move1, ref input1);
                                }

                                if (move2 == 0x00)
                                {
                                    PickMove(2, ref move2);
                                }
                                else
                                {
                                    DoMove(2, move2, ref input2);
                                }
                            }
                            else
                            {
                                move1 = 0;
                                move2 = 0;
                            }
                        }
                        break;

                    case 0x09://Battle Setup
                        {
                            if (Project64Watch.Read8(0x147D30) == 0x31)//Names
                            {
                                EditNames();
                            }

                            if (Project64Watch.Read8(0xAA8DB) == 0xE9)//Choose Pokemon
                            {
                                switch (Project64Watch.Read8(0xAA407))
                                {
                                    case 0x03://Pick Pokemon
                                    case 0xD1://Pick Pokemon
                                        {
                                            switch (cycle)
                                            {
                                                case 0x00:
                                                    {
                                                        Project64Watch.SetInput(ref input1, "B");
                                                        Project64Watch.SetInput(ref input2, "B");
                                                        cycle++;
                                                    }
                                                    break;

                                                case 0x01:
                                                    {
                                                        Project64Watch.SetInput(ref input1, "C Left");
                                                        Project64Watch.SetInput(ref input2, "C Left");
                                                        cycle++;
                                                    }
                                                    break;

                                                case 0x02:
                                                    {
                                                        Project64Watch.SetInput(ref input1, "C Up");
                                                        Project64Watch.SetInput(ref input2, "C Up");
                                                        cycle = 0;
                                                    }
                                                    break;
                                            }
                                        }
                                        break;

                                    case 0xB0://Yes
                                        {
                                            Project64Watch.SetInput(ref input1, "A");
                                            Project64Watch.SetInput(ref input2, "A");
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case 0x0A://Battle Result
                        {
                            Project64Watch.SetInput(ref input1, "A");
                            cycle = (uint)rand.Next(0, 2);

                            if (Project64Watch.Read8(0x147D61) != 0x00)//1P Wins!
                            {
                                Console.WriteLine("\n1P Wins!\n");
                                Project64Watch.Write8(0x147D61, 0x00);
                                InjectTeams();
                            }
                            else if (Project64Watch.Read8(0x147D63) != 0x00)//2P Wins!
                            {
                                Console.WriteLine("\n2P Wins!\n");
                                Project64Watch.Write8(0x147D63, 0x00);
                                InjectTeams();
                            }
                        }
                        break;
                }

                Project64Watch.SendInput(1, input1);
                Project64Watch.SendInput(2, input2);
                Thread.Sleep(250);
            }
        }

        static void InjectTeams()
        {
            Byte[] TeamData = new Byte[0x60 * 3];
            Byte[] PokemonData;
            String[] FilePaths = Directory.GetFiles("SSD2", "*.pk2", SearchOption.AllDirectories);
            do
            {
                for (int i = 0; i < 3; i++)
                {
                    PokemonData = File.ReadAllBytes(FilePaths[rand.Next(FilePaths.Length)]);
                    Array.Copy(PokemonData, 0x00, TeamData, (0x60 * i), 0x60);
                }
            } while ((TeamData[0x00] == TeamData[0x60]) || (TeamData[0x00] == TeamData[0xC0]) || (TeamData[0x60] == TeamData[0xC0]));//No Same Pokémons
            Project64Watch.Write(0x146240, TeamData);

            do
            {
                for (int i = 0; i < 3; i++)
                {
                    PokemonData = File.ReadAllBytes(FilePaths[rand.Next(FilePaths.Length)]);
                    Array.Copy(PokemonData, 0x00, TeamData, (0x60 * i), 0x60);
                }
            } while ((TeamData[0x00] == TeamData[0x60]) || (TeamData[0x00] == TeamData[0xC0]) || (TeamData[0x60] == TeamData[0xC0]));//No Same Pokémons
            Project64Watch.Write(0x146480, TeamData);
        }

        static void EditNames()
        {
            Byte[] Name = Encoding.ASCII.GetBytes("RED TEAM");
            Project64Watch.Write(0x147D30, Name);

            Name = Encoding.ASCII.GetBytes("BLUE TEAM");
            Project64Watch.Write(0x147D3C, Name);
        }

        static void ReadString()
        {
            Byte[] boxstring;

            if (Project64Watch.Read8(0x1E1DF8) != 0x00)
            {
                boxstring = Project64Watch.Read(0x1E1DF8, 0x80);
                if (!oldboxstring.SequenceEqual(boxstring))
                {
                    Array.Copy(boxstring, oldboxstring, 0x80);
                    Console.WriteLine(Encoding.UTF8.GetString(boxstring).Replace("\0", ""));
                    if(Project64Watch.Read8(0x1E1E78) != 0x00)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(Project64Watch.Read(0x1E1E78, 0x80)).Replace("\0", ""));
                        if (Project64Watch.Read8(0x1E1EF8) != 0x00)
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(Project64Watch.Read(0x1E1EF8, 0x80)).Replace("\0", ""));
                        }
                    }
                }
            }
        }

        static void PickMove(UInt32 player, ref UInt32 move)
        {
            UInt32 Offset = 0;
            if (player == 1)
            {
                Offset = 0x1DD265;
            }
            else if (player == 2)
            {
                Offset = 0x1DD27D;
            }

            switch (Project64Watch.Read16(Offset))
            {
                case 0x0101://Attack / Switch / Run
                    {
                        if (rand.Next(10) == 0x00)//Switch Chance
                        {
                            PickPokemon(player, ref move, false);
                        }
                        else
                        {
                            PickAttack(player, ref move);
                        }
                    }
                    break;

                case 0x0505://Replace fainted Pokémon
                    {
                        PickPokemon(player, ref move, true);
                    }
                    break;
            }
        }

        static void PickAttack(UInt32 player, ref UInt32 move)
        {
            UInt32 randMove = 0x00;
            UInt32 Offset = 0x00;

            if (player == 1)
            {
                Offset = 0xD1CF8;
            }
            else if (player == 2)
            {
                Offset = 0xD1D4C;
            }

            for (uint i = 0; i < 4; i++)
            {
                if (Project64Watch.Read8(Offset + i + 0x09) != 0x00)
                {
                    randMove++;
                }
            }

            if (randMove != 00)//Checks if all moves are 0 PP
            {
                if ((Project64Watch.Read8(Offset + 0x11) & 0x10) == 0x00)//Encore Check
                {
                    while (true)
                    {
                        randMove = (uint)rand.Next(4);
                        if ((Project64Watch.Read8(Offset + randMove + 0x09) != 0x00) && (Project64Watch.Read8(Offset + randMove + 0x05) != Project64Watch.Read8(Offset + 0x25)))//if PP not 0 and move not disabled
                        {
                            move = randMove + 1;
                            break;
                        }
                    }
                }
                else
                {
                    for (uint i = 0; i < 4; i++)
                    {
                        if (Project64Watch.Read8(Offset + 0x03) == Project64Watch.Read8(Offset + i + 0x05))//Encore Check
                        {
                            move = i + 1;
                        }
                    }
                }
            }
            else//All moves had 0 PP
            {
                move = 1 + (uint)rand.Next(4);
            }
        }

        static void PickPokemon(UInt32 player, ref UInt32 move, Boolean forced)
        {
            UInt32 mon = 0;
            UInt32 moncount = 0;
            UInt32 activeteam = 0;
            UInt32 activespecies = 0;
            UInt32 activemon1 = 0;
            UInt32 activemon2 = 0;

            if (player == 1)
            {
                activeteam = 0xD1DC0;
                activespecies = 0x1DD237;
                activemon1 = 0xD1CF8;
                activemon2 = 0xD1D4C;
            }
            else if (player == 2)
            {
                activeteam = 0xD22A0;
                activespecies = 0x1DD24F;
                activemon1 = 0xD1D4C;
                activemon2 = 0xD1CF8;
            }

            if ((Project64Watch.Read8(activemon1 + 0x1B) == 0x00) && ((Project64Watch.Read8(activemon2 + 0x11) & 0x80) == 0x00) || forced)//not trapped from Wrap, Mean Look, etc.
            {
                for (uint i = 0; i < 3; i++)//check to see if we have more than 1 pokemon alive, otherwise can't switch. if forced switch then 1 pokemon left is possible
                {
                    if (Project64Watch.Read16(activeteam + (0x58 * i) + 0x26) != 0x00)
                    {
                        moncount++;
                    }
                }

                while ((moncount > 1) || forced)
                {
                    mon = (uint)rand.Next(3);
                    if ((Project64Watch.Read16(activeteam + (0x58 * mon) + 0x26) != 0x00) && (Project64Watch.Read8(activeteam + (0x58 * mon)) != Project64Watch.Read8(activespecies)))//if HP not 0 and not already active
                    {
                        break;
                    }
                }

                if ((moncount > 1) || forced)
                {
                    move = 5 + mon;
                }
            }
        }

        static void DoMove(UInt32 player, UInt32 move, ref UInt32 input)
        {
            UInt32 Offset = 0x00;

            if (player == 1)
            {
                Offset = 0x1DD265;
            }
            else if (player == 2)
            {
                Offset = 0x1DD27D;
            }

            switch (Project64Watch.Read16(Offset))
            {
                case 0x0101://Attack / Switch / Run
                    {
                        switch (move)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                {
                                    Project64Watch.SetInput(ref input, "A");
                                }
                                break;

                            case 5:
                            case 6:
                            case 7:
                                {
                                    Project64Watch.SetInput(ref input, "B");
                                }
                                break;
                        }
                    }
                    break;

                case 0x0505://Pick Pokemon Forced
                case 0x0501://Pick Pokemon
                    {
                        switch (move)
                        {
                            case 5:
                                {
                                    Project64Watch.SetInput(ref input, "C Left");
                                }
                                break;

                            case 6:
                                {
                                    Project64Watch.SetInput(ref input, "C Up");
                                }
                                break;

                            case 7:
                                {
                                    Project64Watch.SetInput(ref input, "C Right");
                                }
                                break;
                        }
                    }
                    break;

                case 0x0201://Pick Move
                    {
                        switch (move)
                        {
                            case 1:
                                {
                                    Project64Watch.SetInput(ref input, "C Up");
                                }
                                break;

                            case 2:
                                {
                                    Project64Watch.SetInput(ref input, "C Right");
                                }
                                break;

                            case 3:
                                {
                                    Project64Watch.SetInput(ref input, "C Down");
                                }
                                break;

                            case 4:
                                {
                                    Project64Watch.SetInput(ref input, "C Left");
                                }
                                break;
                        }
                    }
                    break;
            }
        }
    }
}