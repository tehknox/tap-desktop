﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.AirlineCooperationModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.WeatherModel;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
  
    public class AirportMVVM : INotifyPropertyChanged
    {
        public Airport Airport { get; set; }
        public List<Weather> Weather { get; set; }
        public HourlyWeather CurrentWeather { get; set; }
        public ObservableCollection<AirportTerminalMVVM> Terminals { get; set; }
        public ObservableCollection<AirportTerminalMVVM> BuildingTerminals { get; set; }
        public ObservableCollection<ContractMVVM> Contracts { get; set; }
        public ObservableCollection<Hub> Hubs { get; set; }
        public List<DemandMVVM> Demands { get; set; }
        public double TerminalPrice { get; set; }
        public double TerminalGatePrice { get; set; }
        public List<AirportFacility> AirportFacilities { get; set; }
        public ObservableCollection<Cooperation> Cooperations { get; set; }
        public ObservableCollection<AirlineAirportFacilityMVVM> AirlineFacilities { get; set; }
        public ObservableCollection<AirlineAirportFacilityMVVM> BuildingAirlineFacilities { get; set; }
        public List<AirportTrafficMVVM> Traffic { get; set; }
        public List<AirportStatisticsMVMM> AirlineStatistics { get; set; }
        public List<DestinationFlightsMVVM> Flights { get; set; }
        public List<AirlineReputationMVVM> AirlineReputations { get; set; }
        public DateTime LocalTime { get; set; }
        public Boolean ShowLocalTime { get; set; }
        private Boolean _canMakeCooperation;
        public Boolean CanMakeCooperation
        {
            get { return _canMakeCooperation; }
            set { _canMakeCooperation = value; NotifyPropertyChanged("CanMakeCooperation"); }
        }
        private Boolean _canBuildHub;
        public Boolean CanBuildHub
        {
            get { return _canBuildHub; }
            set { _canBuildHub = value; NotifyPropertyChanged("CanBuildHub"); }
        }
        private int _freeGates;
        public int FreeGates
        {
            get { return _freeGates; }
            set { _freeGates = value; NotifyPropertyChanged("FreeGates"); }
        }
        public AirportMVVM(Airport airport)
        {
            this.Airport = airport;

            this.TerminalGatePrice = this.Airport.getTerminalGatePrice();
            this.TerminalPrice = this.Airport.getTerminalPrice();

            this.Cooperations = new ObservableCollection<Cooperation>();
            this.Terminals = new ObservableCollection<AirportTerminalMVVM>();
            this.BuildingTerminals = new ObservableCollection<AirportTerminalMVVM>();

            foreach (Terminal terminal in this.Airport.Terminals.getTerminals())
            {
                Boolean isSellable = terminal.Airline != null && terminal.Airline == GameObject.GetInstance().HumanAirline;

                if (terminal.IsBuilt)
                    this.Terminals.Add(new AirportTerminalMVVM(terminal, terminal.IsBuyable, isSellable));
                else
                    this.BuildingTerminals.Add(new AirportTerminalMVVM(terminal, terminal.IsBuyable, isSellable));
            }
            this.Contracts = new ObservableCollection<ContractMVVM>();

            foreach (AirportContract contract in this.Airport.AirlineContracts)
                this.Contracts.Add(new ContractMVVM(contract));

            foreach (Cooperation cooperation in this.Airport.Cooperations)
                this.Cooperations.Add(cooperation);

            AirportHelpers.CreateAirportWeather(this.Airport);

            this.Weather = this.Airport.Weather.ToList();

            if (!GameObject.GetInstance().DayRoundEnabled)
                this.CurrentWeather = this.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour];
            
            this.FreeGates = this.Airport.Terminals.NumberOfFreeGates;

            this.Demands = new List<DemandMVVM>();

            var demands = this.Airport.getDestinationDemands().Where(a => a != null && GeneralHelpers.IsAirportActive(a)).OrderByDescending(a=>this.Airport.getDestinationPassengersRate(a, AirlinerClass.ClassType.Economy_Class));

            var internationalDemand = demands.Where(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) != new CountryCurrentCountryConverter().Convert(this.Airport.Profile.Country));
            var domesticDemand = demands.Where(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) == new CountryCurrentCountryConverter().Convert(this.Airport.Profile.Country));
           
            foreach (Airport destination in internationalDemand)
                this.Demands.Add(new DemandMVVM(destination, (int)this.Airport.getDestinationPassengersRate(destination, AirlinerClass.ClassType.Economy_Class),(int)this.Airport.Profile.Pax, (int)this.Airport.getDestinationCargoRate(destination),DemandMVVM.DestinationType.International));

            foreach (Airport destination in domesticDemand)
                this.Demands.Add(new DemandMVVM(destination, (int)this.Airport.getDestinationPassengersRate(destination, AirlinerClass.ClassType.Economy_Class),(int)this.Airport.Profile.Pax, (int)this.Airport.getDestinationCargoRate(destination), DemandMVVM.DestinationType.Domestic));
            
            this.AirportFacilities = this.Airport.getAirportFacilities().FindAll(f => f.Airline == null && f.Facility.TypeLevel!=0).Select(f=>f.Facility).Distinct().ToList();

            this.AirlineFacilities = new ObservableCollection<AirlineAirportFacilityMVVM>();
            this.BuildingAirlineFacilities = new ObservableCollection<AirlineAirportFacilityMVVM>();
           
            foreach (var facility in this.Airport.getAirportFacilities().FindAll(f => f.Airline != null))
                if (facility.Facility.TypeLevel != 0)
                {
                    Alliance alliance = facility.Airline.Alliances.Count == 0 ? null : facility.Airline.Alliances[0];

                    AirlineAirportFacilityMVVM airlineFacility = new AirlineAirportFacilityMVVM(facility, alliance);

                    if (airlineFacility.IsDelivered)
                    {
                        if (facility == Airport.getAirlineAirportFacility(facility.Airline,facility.Facility.Type))
                            this.AirlineFacilities.Add(airlineFacility);
                    }
                    else
                        this.BuildingAirlineFacilities.Add(airlineFacility);
                }

            this.AirlineStatistics = new List<AirportStatisticsMVMM>();
            
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                StatisticsType passengersType = StatisticsTypes.GetStatisticsType("Passengers");
                StatisticsType passengersAvgType = StatisticsTypes.GetStatisticsType("Passengers%");
                StatisticsType arrivalsType = StatisticsTypes.GetStatisticsType("Arrivals");

                double passengers = this.Airport.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year,airline,passengersType);
                double passengersAvg = this.Airport.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year,airline,passengersAvgType);
                double arrivals = this.Airport.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year,airline,arrivalsType);

                int routes = airline.Routes.Count(r => r.Destination1 == this.Airport || r.Destination2 == this.Airport);

                this.AirlineStatistics.Add(new AirportStatisticsMVMM(airline, passengers, passengersAvg, arrivals,routes));

               
            }

            this.Traffic = new List<AirportTrafficMVVM>();
            
            var passengerDestinations = from a in Airports.GetAllActiveAirports() orderby this.Airport.getDestinationPassengerStatistics(a) descending select a;
            var cargoDestinations = from a in Airports.GetAllActiveAirports() orderby this.Airport.getDestinationCargoStatistics(a) descending select a;

            foreach (Airport a in passengerDestinations.Take(20))
                this.Traffic.Add(new AirportTrafficMVVM(a, this.Airport.getDestinationPassengerStatistics(a),AirportTrafficMVVM.TrafficType.Passengers));
           
            foreach (Airport a in cargoDestinations.Take(20))
                this.Traffic.Add(new AirportTrafficMVVM(a, Convert.ToInt64(this.Airport.getDestinationCargoStatistics(a)), AirportTrafficMVVM.TrafficType.Cargo));

            this.Flights = new List<DestinationFlightsMVVM>();

            var airportRoutes = AirportHelpers.GetAirportRoutes(this.Airport).Where(r=>r.getAirliners().Count>0);

            foreach (Route airportRoute in airportRoutes)
            {
                double distance = MathHelpers.GetDistance(airportRoute.Destination1, airportRoute.Destination2);

                Airport destination = airportRoute.Destination1 == this.Airport ? airportRoute.Destination2 : airportRoute.Destination1;
                if (this.Flights.Exists(f=>f.Airline == airportRoute.Airline && f.Airport == destination))
                {
                    DestinationFlightsMVVM flight = this.Flights.First(f=>f.Airline == airportRoute.Airline && f.Airport == destination);
                    
                    flight.Flights += airportRoute.TimeTable.getEntries(destination).Count;
                    
                    foreach (AirlinerType aircraft in airportRoute.getAirliners().Select(a=>a.Airliner.Type))
                        if (!flight.Aircrafts.Contains(aircraft))
                            flight.Aircrafts.Add(aircraft);

                }
                else
                    this.Flights.Add(new DestinationFlightsMVVM(destination,airportRoute.Airline,distance,airportRoute.getAirliners().Select(a=>a.Airliner.Type).ToList(),airportRoute.TimeTable.getEntries(destination).Count));
            }
            /*
            Dictionary<Airport, int> destinations = new Dictionary<Airport, int>();
            foreach (Route route in AirportHelpers.GetAirportRoutes(this.Airport).FindAll(r => r.getAirliners().Count > 0))
            {
                if (route.Destination1 != this.Airport)
                {
                    if (!destinations.ContainsKey(route.Destination1))
                        destinations.Add(route.Destination1, 0);
                    destinations[route.Destination1] += route.TimeTable.getEntries(route.Destination1).Count;


                }
                if (route.Destination2 != this.Airport)
                {
                    if (!destinations.ContainsKey(route.Destination2))
                        destinations.Add(route.Destination2, 0);
                    destinations[route.Destination2] += route.TimeTable.getEntries(route.Destination2).Count;
                }
            }

            foreach (Airport a in destinations.Keys)
                this.Flights.Add(new DestinationFlightsMVVM(a, destinations[a]));
            */

            this.Hubs = new ObservableCollection<Hub>();

            foreach (Hub hub in this.Airport.getHubs())
                this.Hubs.Add(hub);

            this.CanBuildHub = canBuildHub();
            this.CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a=>a == this.Airport);

            this.LocalTime = MathHelpers.ConvertDateTimeToLoalTime(GameObject.GetInstance().GameTime, this.Airport.Profile.TimeZone);

            this.ShowLocalTime = !GameObject.GetInstance().DayRoundEnabled;

            this.AirlineReputations = new List<AirlineReputationMVVM>();

            IDictionary<Airline,double> airlineScores = new Dictionary<Airline,double>();

            foreach (Airline airline in Airlines.GetAllAirlines())
                airlineScores.Add(airline,this.Airport.getAirlineReputation(airline));

            foreach (KeyValuePair<Airline, double> score in StatisticsHelpers.GetRatingScale(airlineScores))
                this.AirlineReputations.Add(new AirlineReputationMVVM(score.Key, (int)score.Value));
        }
        //returns if hub can be build
        private Boolean canBuildHub()
        {
            Boolean hasServiceCenter = this.Airport.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline,AirportFacility.FacilityType.Service).Facility.TypeLevel > 0;
            
            double gatesPercent = Convert.ToDouble(this.Contracts.Count(c=>c.Airline == GameObject.GetInstance().HumanAirline)) / Convert.ToDouble(this.Airport.Terminals.NumberOfGates);
         
            return gatesPercent > 0.2 && this.Hubs.Count == 0 && hasServiceCenter;
        }
        //adds a terminal to the airport
        public void addTerminal(Terminal terminal)
        {
            this.Airport.addTerminal(terminal);

            this.BuildingTerminals.Add(new AirportTerminalMVVM(terminal, false,terminal.Airline!=null && terminal.Airline == GameObject.GetInstance().HumanAirline));
 
        }
        //removes a terminal from the airport
        public void removeTerminal(AirportTerminalMVVM terminal)
        {
            this.Airport.removeTerminal(terminal.Terminal);

            this.Terminals.Remove(terminal);
        }
        //adds an airline contract to the airport
        public void addAirlineContract(AirportContract contract)
        {
            AirportHelpers.AddAirlineContract(contract);

            this.Contracts.Add(new ContractMVVM(contract));

            this.FreeGates = this.Airport.Terminals.NumberOfFreeGates;
                       
            this.CanBuildHub = this.Contracts.Count(c => c.Airline == GameObject.GetInstance().HumanAirline) > 0;

            foreach (AirportTerminalMVVM terminal in Terminals)
            {
                terminal.FreeGates = terminal.Terminal.getFreeGates();
            }

            this.CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a => a == this.Airport);
    
        }
        //removes an airline contract from the airport
        public void removeAirlineContract(ContractMVVM contract)
        {
            this.Airport.removeAirlineContract(contract.Contract);

            this.Contracts.Remove(contract);

            this.FreeGates = this.Airport.Terminals.NumberOfFreeGates;

            foreach (AirportTerminalMVVM terminal in Terminals)
            {
                terminal.FreeGates = terminal.Terminal.getFreeGates();
            }

            this.CanMakeCooperation = GameObject.GetInstance().HumanAirline.Airports.Exists(a => a == this.Airport);

    
        }
        //adds a hub
        public void addHub(Hub hub)
        {
            this.Hubs.Add(hub);
            this.Airport.addHub(hub);

            this.CanBuildHub = canBuildHub();

        }
        //removes a hub
        public void removeHub(Hub hub)
        {
            this.Hubs.Remove(hub);
            this.Airport.removeHub(hub);

            this.CanBuildHub = canBuildHub();
        }
        //removes a cooperation from the airport
        public void removeCooperation(Cooperation cooperation)
        {
            this.Cooperations.Remove(cooperation);
            this.Airport.removeCooperation(cooperation);
        }
        //adds a cooperation to the airport
        public void addCooperation(Cooperation cooperation)
        {
            this.Cooperations.Add(cooperation);
            this.Airport.addCooperation(cooperation);
            
            AirlineHelpers.AddAirlineInvoice(cooperation.Airline, cooperation.BuiltDate, Invoice.InvoiceType.Purchases, -cooperation.Type.Price);
            
        }
        //removes an airline facility from the airport
        public void removeAirlineFacility(AirlineAirportFacilityMVVM facility)
        {
            
            this.Airport.removeFacility(facility.Facility.Airline, facility.Facility.Facility);

            this.AirlineFacilities.Remove(facility);

            if (this.Airport.getAirlineAirportFacility(facility.Facility.Airline, facility.Facility.Facility.Type).Facility.TypeLevel > 0)
            {
                Alliance alliance = facility.Facility.Airline.Alliances.Count == 0 ? null : facility.Facility.Airline.Alliances[0];
               
                AirlineAirportFacilityMVVM airlineFacility = new AirlineAirportFacilityMVVM(this.Airport.getAirlineAirportFacility(facility.Facility.Airline, facility.Facility.Facility.Type),alliance);

                if (airlineFacility.IsDelivered)
                    this.AirlineFacilities.Add(airlineFacility);
                else
                    this.BuildingAirlineFacilities.Add(airlineFacility);
            }
        }
        //adds an airline facility to the airport
        public void addAirlineFacility(AirportFacility facility)
        {
            AirlineAirportFacility nextFacility = new AirlineAirportFacility(GameObject.GetInstance().HumanAirline,this.Airport, facility, GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));
            this.Airport.addAirportFacility(nextFacility);

            /*
            AirlineAirportFacilityMVVM currentFacility = this.AirlineFacilities.Where(f => f.Facility.Facility.Type == facility.Type).FirstOrDefault();

            if (currentFacility != null)
                removeAirlineFacility(currentFacility);
            
            Alliance alliance = nextFacility.Airline.Alliances.Count == 0 ? null : nextFacility.Airline.Alliances[0];
            
            this.AirlineFacilities.Add(new AirlineAirportFacilityMVVM(nextFacility,alliance));
             * */
            this.AirlineFacilities.Clear();
            this.BuildingAirlineFacilities.Clear();
           
            foreach (var tFacility in this.Airport.getAirportFacilities().FindAll(f => f.Airline != null))
                if (tFacility.Facility.TypeLevel != 0)
                {
                    Alliance alliance = tFacility.Airline.Alliances.Count == 0 ? null : tFacility.Airline.Alliances[0];

                    AirlineAirportFacilityMVVM airlineFacility = new AirlineAirportFacilityMVVM(tFacility, alliance);

                    if (airlineFacility.IsDelivered)
                        this.AirlineFacilities.Add(airlineFacility);
                    else
                        this.BuildingAirlineFacilities.Add(airlineFacility);
                }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    //the mvvm class for airport traffic
    public class AirportTrafficMVVM
    {
        public Airport Destination { get; set; }
        public long Value { get; set; }
        public enum TrafficType { Passengers, Cargo }
        public TrafficType Type { get; set; }
        public AirportTrafficMVVM(Airport destination, long value, TrafficType type)
        {
            this.Destination = destination;
            this.Value = value;
            this.Type = type;
        }
    }
    //the mvvm class for passenger demand
    public class DemandMVVM : INotifyPropertyChanged
    {
        public int Cargo { get; set; }
        public int Passengers { get; set; }
        public int TotalPax { get; set; }
        public Airport Destination { get; set; }
        public enum DestinationType { Domestic, International }
        public DestinationType Type{ get; set; }
        private Boolean _contracted;
        public Boolean Contracted
        {
            get { return _contracted; }
            set { _contracted = value; NotifyPropertyChanged("Contracted"); }
        }
        public DemandMVVM(Airport destination, int passengers, int totalpax, int cargo, DestinationType type)
        {
            this.Cargo = cargo;
            this.Passengers = passengers;
            this.TotalPax = totalpax;
            this.Destination = destination;
            this.Type = type;
            this.Contracted = this.Destination.AirlineContracts.Exists(c => c.Airline == GameObject.GetInstance().HumanAirline);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    //the mvvm class for an airline contract
    public class ContractMVVM : INotifyPropertyChanged
    {
        public Boolean IsHuman { get; set; }
        public AirportContract Contract { get; set; }
        public Airline Airline { get; set; }
        private int _numberofgates;
        public int NumberOfGates 
        {
            get { return _numberofgates; }
            set { _numberofgates = value; NotifyPropertyChanged("NumberOfGates"); } 
        }
        private int _monthsleft;
        public int MonthsLeft
        {
            get { return _monthsleft; }
            set { _monthsleft = value; NotifyPropertyChanged("MonthsLeft"); }
        }
        public ContractMVVM(AirportContract contract)
        {
            this.Contract = contract;
            this.Airline = this.Contract.Airline;
            this.NumberOfGates = this.Contract.NumberOfGates;
            this.MonthsLeft = this.Contract.MonthsLeft;
            this.IsHuman = this.Airline == GameObject.GetInstance().HumanAirline;
        }
        //extends the contract with a number of year
        public void extendContract(int years)
        {
            this.Contract.Length += years;
            this.Contract.ExpireDate = this.Contract.ExpireDate.AddYears(years);
            this.MonthsLeft = this.Contract.MonthsLeft;
        }
        //sets the number of gates
        public void setNumberOfGates(int gates)
        {
            this.NumberOfGates = gates;
            this.Contract.NumberOfGates = gates;
            this.Contract.YearlyPayment = AirportHelpers.GetYearlyContractPayment(this.Contract.Airport,this.Contract.Type, gates, this.Contract.Length);
        }
        //sets the expire date
        public void setExpireDate(DateTime expireDate)
        {
            int years = MathHelpers.CalculateAge(this.Contract.ExpireDate, expireDate);
            extendContract(years);
     
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    //the mvvm class for an airport gate
    public class AirportGateMVVM : INotifyPropertyChanged
    {
        public int GateNumber { get; set; }
        public Airline Airline { get; set; }
        public Boolean IsFree { get; set; }
        public AirportGateMVVM(int gatenumber, Airline airline)
        {
            this.Airline = airline;
            this.GateNumber = gatenumber;
            this.IsFree = this.Airline == null;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
    //the mvvm class for an airport terminal
    public class AirportTerminalMVVM : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private Airline _airline;
        public Airline Airline
        {
            get { return _airline; }
            set { _airline = value; NotifyPropertyChanged("Airline"); }
        }
     
        public int Gates { get; set; }

        private int _freegates;
        public int FreeGates
        {
            get { return _freegates; }
            set { _freegates = value; NotifyPropertyChanged("FreeGates"); }
        }

        private Boolean _isBuyable;
        public Boolean IsBuyable
        {
            get { return _isBuyable; }
            set { _isBuyable = value; NotifyPropertyChanged("IsBuyable"); }
        }

        private Boolean _isSellable;
        public Boolean IsSellable
        {
            get { return _isSellable; }
            set { _isSellable = value; NotifyPropertyChanged("IsSellable"); }
        }
        public DateTime DeliveryDate { get; set; }

  
        public Terminal Terminal { get; set; }
        public ObservableCollection<AirportGateMVVM> AllGates { get; set; }
        public AirportTerminalMVVM(Terminal terminal, Boolean isBuyable, Boolean isSellable)
        {
            this.Terminal = terminal;

            this.Name = this.Terminal.Name;
            this.Airline = this.Terminal.Airline;
            this.Gates = this.Terminal.Gates.NumberOfGates;
            this.FreeGates = this.Terminal.getFreeGates();
            this.IsBuyable = isBuyable;
            this.DeliveryDate = this.Terminal.DeliveryDate;
            this.IsSellable = isSellable;

             this.AllGates = new ObservableCollection<AirportGateMVVM>();

             int gatenumber = 1;
            
            foreach (Gate gate in this.Terminal.Gates.getGates())
            {
                this.AllGates.Add(new AirportGateMVVM(gatenumber,gate.Airline));

                gatenumber++;
            }
            
     
        }
        //purchase a terminal
        public void purchaseTerminal(Airline airline)
        {
            this.Terminal.purchaseTerminal(airline);
            this.Airline = airline;

            this.IsBuyable = false;

            this.FreeGates = 0;
        }
         public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    //the mvvm object for airport distance
    public class AirportDistanceMVVM
    {
        public Airport Destination { get; set; }
        public double Distance { get; set; }
        public AirportDistanceMVVM(Airport destination,double distance)
        {
            this.Destination = destination;
            this.Distance = distance;
        }
    }
    //the mvvm object for airport flights
     public class DestinationFlightsMVVM
    {
        public int Flights { get; set; }
        public Airport Airport { get; set; }
        public Airline Airline { get; set; }
        public double Distance{ get; set; }
        public List<AirlinerType> Aircrafts { get; set; }
        public DestinationFlightsMVVM(Airport airport,Airline airline, double distance, List<AirlinerType> aircrafts, int flights)
        {
            this.Flights = flights;
            this.Airport = airport;
            this.Distance = distance;
            this.Airline = airline;
            this.Aircrafts = aircrafts;
        }
    }
    //the mvvm object for airport statistics
    public class AirportStatisticsMVMM
    {
        public double Passengers { get; set; }
        public double PassengersPerFlight { get; set; }
        public double Flights { get; set; }
        public int Routes { get; set; }
        public Airline Airline { get; set; }
        public AirportStatisticsMVMM(Airline airline, double passengers, double passengersPerFlight, double flights, int routes)
        {
            this.Passengers = passengers;
            this.Airline = airline;
            this.PassengersPerFlight = passengersPerFlight;
            this.Flights = flights;
            this.Routes = routes;
        }
           
    }
    //the mvvm object for airline airport facility
    public class AirlineAirportFacilityMVVM
    {
        public AirlineAirportFacility Facility { get; set; }
        public Alliance Alliance { get; set; }
        public Boolean IsDelivered { get; set; }
        public Boolean IsHuman { get; set; }
        public AirlineAirportFacilityMVVM(AirlineAirportFacility facility, Alliance alliance)
        {
            this.Facility = facility;
            this.Alliance = alliance;
            this.IsHuman = GameObject.GetInstance().HumanAirline == facility.Airline;
            this.IsDelivered = facility.FinishedDate < GameObject.GetInstance().GameTime;
        }
    }
    //the mvvm object for airline reputation score
    public class AirlineReputationMVVM
    {
        public int Reputation { get; set; }
        public Airline Airline { get; set; }
        public AirlineReputationMVVM(Airline airline, int reputation)
        {
            this.Airline = airline;
            this.Reputation = reputation;
        }
    }
    //the converter for the price of a terminal
    public class TerminalPriceConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int gates = System.Convert.ToInt16(values[0]);
            Airport airport = (Airport)values[1];

            double price = gates * airport.getTerminalGatePrice() + airport.getTerminalPrice();
                       
            return new ValueCurrencyConverter().Convert(price);
          
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for the facilities for a specific type
    public class TypeFacilitiesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            AirportFacility.FacilityType type = (AirportFacility.FacilityType)values[0];
            Airport airport = (Airport)values[1];

            var currentFacility = airport.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, type);
            var buildingFacility = airport.getAirlineBuildingFacility(GameObject.GetInstance().HumanAirline, type);

            var facilities = new List<AirportFacility>();
            foreach (AirportFacility facility in AirportFacilities.GetFacilities(type).Where(f=>f.TypeLevel > currentFacility.TypeLevel))
            {
                if (buildingFacility == null || facility.TypeLevel > buildingFacility.TypeLevel) 
                    facilities.Add(facility);
            }

            return facilities;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //the converter for the temperature (in celsius) to text
    public class TemperatureToTextConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double temperature = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return string.Format("{0:0.0}°C", temperature);
            else
                return string.Format("{0:0}°F", MathHelpers.CelsiusToFahrenheit(temperature));


        }
       
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class WindSpeedToUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Weather.eWindSpeed windspeed = (Weather.eWindSpeed)value;

            double v = (double)windspeed;

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Imperial)
                v =  MathHelpers.KMToMiles(v);

            return string.Format("{0:0} {1}", v, new StringToLanguageConverter().Convert("km/t"));
        }
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for the weather
    public class WeatherImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Weather)
            {
                Weather weather = (Weather)value;

                string weatherCondition = "clear";

                if (weather.Cover == Weather.CloudCover.Overcast && weather.Precip != Weather.Precipitation.None)
                    weatherCondition = weather.Precip.ToString();
                else
                    weatherCondition = weather.Cover.ToString();

                return AppSettings.getDataPath() + "\\graphics\\weather\\" + weatherCondition + ".png";
            }
            if (value is HourlyWeather)
            {
                HourlyWeather weather = (HourlyWeather)value;

                string weatherCondition = "clear";

                if (weather.Cover == Weather.CloudCover.Overcast && weather.Precip != Weather.Precipitation.None)
                    weatherCondition = weather.Precip.ToString();
                else
                    weatherCondition = weather.Cover.ToString();

                if (GameObject.GetInstance().GameTime.Hour < Weather.Sunrise || GameObject.GetInstance().GameTime.Hour > Weather.Sunset)
                    weatherCondition += "-night";
                
                return AppSettings.getDataPath() + "\\graphics\\weather\\" + weatherCondition + ".png";
 
            }

            return "";


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
    //the converter for the yearly payment of a contract
    public class ContractYearlyPaymentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int gates = System.Convert.ToInt16(values[0]);
            int lenght = System.Convert.ToInt16(values[1]);
            AirportContract.ContractType contractType = (AirportContract.ContractType)values[2];
            Airport airport = (Airport)values[3];

            return new ValueCurrencyConverter().Convert(AirportHelpers.GetYearlyContractPayment(airport,contractType, gates, lenght));

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
