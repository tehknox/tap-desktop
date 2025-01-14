﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageAirlinersHumanFleet.xaml
    /// </summary>
    public partial class PageAirlinersHumanFleet : Page
    {
        public List<FleetAirliner> Fleet { get; set; }
        public List<FleetAirliner> OrderedFleet { get; set; }
        public PageAirlinersHumanFleet()
        {
            this.Fleet = GameObject.GetInstance().HumanAirline.Fleet.Where(f=>f.Airliner.BuiltDate < GameObject.GetInstance().GameTime).ToList();
            this.OrderedFleet = GameObject.GetInstance().HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate >= GameObject.GetInstance().GameTime).ToList();

            InitializeComponent();

          
        }
    }
}
