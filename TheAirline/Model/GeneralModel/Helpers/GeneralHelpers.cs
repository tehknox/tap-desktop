﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PilotModel;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;
using System.ComponentModel;
using TheAirline.Model.GeneralModel.WeatherModel;

namespace TheAirline.Model.GeneralModel
{
    //class for some general helpers
    public class GeneralHelpers
    {
        public enum GameSpeedValue {Slowest = 2500, Slow = 2000, Normal = 1500, Fast = 1000, Fastest = 500 }
        public enum Size { Smallest, Very_small, Small, Medium, Large, Very_large, Largest }
       // public enum Rate { None }
        public static string BigMapXaml;
        // chs, 2011-17-11 changed for showing departures and arrivals from one airport to another
        //returns the list of arrivals or departures from one airport to another
        public static List<RouteTimeTableEntry> GetAirportFlights(Airport fAirport, Airport tAirport, Boolean arrivals)
        {
            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in AirportHelpers.GetAirportRoutes(fAirport))
            {
                if (route.HasAirliner && (route.Destination1 == tAirport || route.Destination2 == tAirport))
                {
                  

                    RouteTimeTableEntry entry = route.getCurrentAirliner() == null || route.getCurrentAirliner().CurrentFlight == null || route.getCurrentAirliner().CurrentFlight.Entry == null ? route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime) : route.getCurrentAirliner().CurrentFlight.Entry;
                    
                    for (int i = 0; i < route.TimeTable.Entries.Count; i++)
                    {
                        if (!arrivals && entry.Destination.Airport == tAirport)
                            entries.Add(entry);

                        if (arrivals && entry.Destination.Airport == fAirport)
                            entries.Add(entry);

                        entry = route.TimeTable.getNextEntry(entry);
                   
                    }
                  
                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries;
        }
        //returns the list of arrivals for an airport
        public static List<RouteTimeTableEntry> GetAirportArrivals(Airport airport, DayOfWeek day)
        {
            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in AirportHelpers.GetAirportRoutes(airport))
            {
                if (route.HasAirliner)
                {
                    var rEntries = route.TimeTable.Entries.Where(e => e.Day == day && e.Destination.Airport == airport);

                    entries.AddRange(rEntries);
                   

                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries;
        }
        public static List<RouteTimeTableEntry> GetAirportArrivals(Airport airport,int count)
        {

            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in AirportHelpers.GetAirportRoutes(airport))
            {
                if (route.HasAirliner && route.getCurrentAirliner()!=null)
                {
                    RouteTimeTableEntry entry = route.getCurrentAirliner().CurrentFlight == null ? route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, (airport == route.Destination1 ? route.Destination2 : route.Destination1)) : route.getCurrentAirliner().CurrentFlight.Entry;

                    for (int i = 0; i < route.TimeTable.Entries.Count; i++)
                    {
                        if (entry.Destination.Airport == airport)
                            entries.Add(entry);
                        entry = route.TimeTable.getNextEntry(entry);
                   
                    }
                   
                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries.GetRange(0, Math.Min(entries.Count, count));
        }
        //returns the list of departures for an airport
        public static List<RouteTimeTableEntry> GetAirportDepartures(Airport airport, DayOfWeek day)
        {
            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in AirportHelpers.GetAirportRoutes(airport))
            {
                if (route.HasAirliner)
                {
                    var rEntries = route.TimeTable.Entries.Where(e => e.Day == day && e.DepartureAirport == airport);

                    entries.AddRange(rEntries);


                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries;
        }
        public static List<RouteTimeTableEntry> GetAirportDepartures(Airport airport, int count)
        {
            
            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in AirportHelpers.GetAirportRoutes(airport))
            {
                if (route.HasAirliner && route.getCurrentAirliner() != null)
                {
                    RouteTimeTableEntry entry = route.getCurrentAirliner().CurrentFlight == null ? route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airport) : route.getCurrentAirliner().CurrentFlight.Entry;

                    if (!entry.Destination.Airport.Profile.Coordinates.Equals(airport.Profile.Coordinates))
                        entries.Add(entry);

                    while (entries.Count<4)
                    {
                        entry = route.TimeTable.getNextEntry(entry);
                        if (entry.Destination.Airport != airport)
                            entries.Add(entry);
                    }
                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries.GetRange(0, Math.Min(entries.Count, count));
        }
        //returns the rate (for loan) for an airline
        public static double GetAirlineLoanRate(Airline airline)
        {
            double value = (double)airline.getAirlineValue();

            return (GameObject.GetInstance().Difficulty.LoanLevel + 0.5) *((double)Airline.AirlineValue.Very_high + 1 - value);
         
        }
       
        //creates the big image map
        public static void CreateBigImageCanvas()
        {
            int zoom = 3;
            int imageSize = 256;

            Canvas panelMap = new Canvas();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    string name = string.Format(@"{0}\{1}\{2}.png", zoom, x, y);

                    Image imgMap = new Image();
                    imgMap.Width = imageSize;
                    imgMap.Height = imageSize;
                    imgMap.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\maps\\" + name, UriKind.RelativeOrAbsolute));
                    RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

                    Canvas.SetTop(imgMap, y * imageSize);
                    Canvas.SetLeft(imgMap, x * imageSize);

                    panelMap.Children.Add(imgMap);



                }
            }
            BigMapXaml = XamlWriter.Save(panelMap);


        }
        //converts a range (route distance) to a airliner range type
        public static AirlinerType.TypeRange ConvertDistanceToRangeType(double distance)
        {
            if (distance < 2000) return AirlinerType.TypeRange.Regional;
            if (distance >= 2000 && distance < 3500) return AirlinerType.TypeRange.Short_Range;
            if (distance >= 3500 && distance < 9000) return AirlinerType.TypeRange.Medium_Range;
            if (distance >= 9000) return AirlinerType.TypeRange.Long_Range;

            return AirlinerType.TypeRange.Medium_Range;
        }
        //converts airliner class type to a price factor
        public static int ClassToPriceFactor(AirlinerClass.ClassType type)
        {
            if (type == AirlinerClass.ClassType.Business_Class)
                return 3;
            if (type == AirlinerClass.ClassType.Economy_Class)
                return 1;
            if (type == AirlinerClass.ClassType.First_Class)
                return 6;
            return 1;
        }


        //returns the discount for an order of airliners
        public static double GetAirlinerOrderDiscount(int orders)
        {
            
            if (orders > 2)
                return ((orders - 2) * 0.5) / 100;
           else
                return 0;
        }
       
        //the converter for a price based on inflation
        public static double GetInflationPrice(double price)
        {
            Inflation baseInflation = Inflations.GetInflation(Inflations.BaseYear);
            Inflation currentInflation = Inflations.GetInflation(GameObject.GetInstance().GameTime.Year);

            double modifier = currentInflation.Modifier / baseInflation.Modifier;

            double newPrice = price * modifier;

            return newPrice;
        }
        //returns if an airport is active (built)
        public static Boolean IsAirportActive(Airport airport)
        {
            return airport.Profile.Period.From <= GameObject.GetInstance().GameTime && airport.Profile.Period.To > GameObject.GetInstance().GameTime && Airports.Contains(airport);
        }
        //returns the season for a date
        public static Weather.Season GetSeason(DateTime date)
        {
            DateTime summertimeStart = new DateTime(date.Year, 4, 1);
            DateTime summertimeEnd = new DateTime(date.Year, 10, 1);

            if (date >= summertimeStart && date < summertimeEnd)
                return Weather.Season.Summer;
            else
                return Weather.Season.Winter;
        }
        //creates the holidays for a year
        public static void CreateHolidays(int startYear)
        {
            HolidayYear.Clear();
    
            int endYear = startYear +5;

            for (int i = startYear; i < endYear; i++)
            {
                foreach (Holiday holiday in Holidays.GetHolidays())
                {
                    if (holiday.Type == Holiday.HolidayType.Fixed_Date)
                    {
                        HolidayYear.AddHoliday(new HolidayYearEvent(new DateTime(startYear, holiday.Date.Month, holiday.Date.Day), holiday, 1));
                    }
                    if (holiday.Type == Holiday.HolidayType.Fixed_Month)
                    {
                        HolidayYearEvent eHoliday = new HolidayYearEvent(new DateTime(startYear, holiday.Month, 1), holiday, DateTime.DaysInMonth(startYear, holiday.Month));
                        HolidayYear.AddHoliday(eHoliday);
                    }
                    if (holiday.Type == Holiday.HolidayType.Fixed_Week)
                    {
                        HolidayYear.AddHoliday(new HolidayYearEvent(MathHelpers.GetFirstDateOfWeek(startYear, holiday.Week), holiday, 7));

                    }
                    if (holiday.Type == Holiday.HolidayType.Non_Fixed_Date)
                    {
                        HolidayYear.AddHoliday(new HolidayYearEvent(MathHelpers.GetNthWeekdayOfMonth(startYear, holiday.Month, holiday.Day, holiday.Week), holiday, 1));
                    }
                }
            }
            
        }
        //creates the rating for a pilot student
        public static PilotRating GetPilotStudentRating(Instructor instructor, List<PilotRating> ratings)
        {
            PilotRating instructorRanking = instructor.Rating;
            int aircraftCoeff = instructor.FlightSchool.TrainingAircrafts.Exists(a => a.Type.MaxNumberOfStudents > 5) ? 10 : 0;

            int instructorRankingIndex = PilotRatings.GetRatings().IndexOf(instructorRanking);
            Dictionary<PilotRating, int> rankings = new Dictionary<PilotRating, int>();
            
            if (ratings.Contains(instructorRanking))
                rankings.Add(instructorRanking, 50);

            if (instructorRankingIndex > 0)
            {
                PilotRating prevRating = PilotRatings.GetRatings()[instructorRankingIndex - 1];

                if (ratings.Contains(prevRating))
                    rankings.Add(prevRating, 35 - aircraftCoeff);
            }
            if (instructorRankingIndex < PilotRatings.GetRatings().Count -1)
            {
                PilotRating nextRating = PilotRatings.GetRatings()[instructorRankingIndex + 1];

                if (ratings.Contains(nextRating))
                    rankings.Add(nextRating, 15 + aircraftCoeff);
            }

            if (rankings.Count == 0)
            {
                ratings.ForEach(r => rankings.Add(r, 20 - r.CostIndex));
            }

            if (rankings.Count == 0)
                return GetPilotRating();

            PilotRating rating = AIHelpers.GetRandomItem<PilotRating>(rankings);

            return rating;
        }
        //creates the pilot rating for a pilot student
        public static PilotRating GetPilotStudentRating(Instructor instructor)
        {
            PilotRating instructorRanking = instructor.Rating;
            int aircraftCoeff = instructor.FlightSchool.TrainingAircrafts.Exists(a => a.Type.MaxNumberOfStudents > 5) ? 10 : 0;

            int instructorRankingIndex = PilotRatings.GetRatings().IndexOf(instructorRanking);
            Dictionary<PilotRating, int> rankings = new Dictionary<PilotRating, int>();
            rankings.Add(instructorRanking, 50);

            if (instructorRankingIndex > 0)
            {
                PilotRating prevRating = PilotRatings.GetRatings()[instructorRankingIndex - 1];
                rankings.Add(prevRating, 35 - aircraftCoeff);
            }
            if (instructorRankingIndex < PilotRatings.GetRatings().Count)
            {
                PilotRating nextRating = PilotRatings.GetRatings()[instructorRankingIndex + 1];

                rankings.Add(nextRating, 15 + aircraftCoeff);
            }

            PilotRating rating = AIHelpers.GetRandomItem<PilotRating>(rankings);

            return rating;
        }
        //creates the pilot rating for a pilot student
        public static PilotRating GetPilotStudentRating(PilotStudent student)
        {
            return GetPilotStudentRating(student.Instructor);
        }
        //creates a number of pilots with a specific aircraft
        public static void CreatePilots(int count, string airlinerFamily)
        {
            List<Town> towns = Towns.GetTowns();

            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {

                Town town = towns[rnd.Next(towns.Count)];
                DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-Pilot.RetirementAge), GameObject.GetInstance().GameTime.AddYears(-23));

                PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(town.Country), Names.GetInstance().getRandomLastName(town.Country), birthdate, town);

                PilotRating rating = GetPilotRating();

                int fromYear = Math.Min(GameObject.GetInstance().GameTime.Year - 1, birthdate.AddYears(23).Year);
                int toYear = Math.Min(GameObject.GetInstance().GameTime.Year, birthdate.AddYears(Pilot.RetirementAge).Year);

                DateTime educationTime = MathHelpers.GetRandomDate(birthdate.AddYears(23), new DateTime(toYear, 1, 1));
                Pilot pilot = new Pilot(profile, educationTime, rating);

                pilot.Aircrafts = GetPilotAircrafts(pilot);
                pilot.Aircrafts.RemoveAt(0);
                pilot.Aircrafts.Add(airlinerFamily);

                Pilots.AddPilot(pilot);
            }
        }
        //creates a number of pilots
        public static void CreatePilots(int count)
        {
            List<Town> towns = Towns.GetTowns();
                       
            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {

                Town town = towns[rnd.Next(towns.Count)];
                DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-Pilot.RetirementAge), GameObject.GetInstance().GameTime.AddYears(-23));
                
                PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(town.Country), Names.GetInstance().getRandomLastName(town.Country), birthdate, town);

                PilotRating rating = GetPilotRating();

                int fromYear = Math.Min(GameObject.GetInstance().GameTime.Year - 1, birthdate.AddYears(23).Year);
                int toYear = Math.Min(GameObject.GetInstance().GameTime.Year, birthdate.AddYears(Pilot.RetirementAge).Year);

                DateTime educationTime = MathHelpers.GetRandomDate(birthdate.AddYears(23), new DateTime(toYear, 1, 1));
                Pilot pilot = new Pilot(profile, educationTime, rating);

                pilot.Aircrafts = GetPilotAircrafts(pilot);

                Pilots.AddPilot(pilot);
            }
        }
        //creates a number of airliner families a pilot can fly
        public static List<string> GetPilotAircrafts(Pilot pilot)
        {
            int year = GameObject.GetInstance().GameTime.Year;

            var airlinerFamilies = AirlinerTypes.GetTypes(t => t.Produced.From.Year <= GameObject.GetInstance().GameTime.Year && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30)).Select(a => a.AirlinerFamily).Distinct();
            
            Random rnd = new Random();
            List<string> families = new List<string>();

            int numberOfAircrafts=PilotRatings.GetRatings().IndexOf(pilot.Rating) + 1;

            for (int i = 0; i < numberOfAircrafts; i++)
            {
                var freeFamilies = airlinerFamilies.Where(a=>!families.Contains(a)).ToList();
                string family = freeFamilies[rnd.Next(freeFamilies.Count)];

                families.Add(family);
            }

            return families;
        }
        
        //returns a rating for a pilot
        public static PilotRating GetPilotRating()
        {
            Dictionary<PilotRating, int> ratings = new Dictionary<PilotRating, int>();
            ratings.Add(PilotRatings.GetRating("A"), 10);
            ratings.Add(PilotRatings.GetRating("B"), 20);
            ratings.Add(PilotRatings.GetRating("C"), 40);
            ratings.Add(PilotRatings.GetRating("D"), 20);
            ratings.Add(PilotRatings.GetRating("E"), 10);

            return AIHelpers.GetRandomItem<PilotRating>(ratings);

        }
        //creates a number of instructors
        public static void CreateInstructors(int count)
        {
            List<Town> towns = Towns.GetTowns();

            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                Town town = towns[rnd.Next(towns.Count)];
                DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-Pilot.RetirementAge), GameObject.GetInstance().GameTime.AddYears(-23));
                PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(town.Country), Names.GetInstance().getRandomLastName(town.Country), birthdate, town);

                Dictionary<PilotRating, int> rankings = new Dictionary<PilotRating, int>();
                rankings.Add(PilotRatings.GetRating("A"), 10);
                rankings.Add(PilotRatings.GetRating("B"), 20);
                rankings.Add(PilotRatings.GetRating("C"), 40);
                rankings.Add(PilotRatings.GetRating("D"), 20);
                rankings.Add(PilotRatings.GetRating("E"), 10);

                PilotRating ranking = AIHelpers.GetRandomItem<PilotRating>(rankings);

                Instructor instructor = new Instructor(profile, ranking);

                Instructors.AddInstructor(instructor);
            }
        }
       
    }
    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (T)attributes[0];
        }
    }
    //the class for a key value pair for use on convertres etc.
    public class GameKeyValuePair<T,S>
    {
        public T Key { get; set; }
        public S Value { get; set; }
        public GameKeyValuePair(T key, S value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
