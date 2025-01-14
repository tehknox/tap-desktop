﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for the statistics for an airliner
  [Serializable]
    public class AirlinerStatistics : GeneralStatistics
    {
      [Versioning("airliner")]
          private FleetAirliner Airliner;
           public double FillingDegree { get { return getFillingDegree(); } set { ;} }
           public double Balance { get { return getBalance(); } set { ;} }
           public double IncomePerPassenger { get { return getIncomePerPassenger(); } set { ;} }
        
      public AirlinerStatistics(FleetAirliner airliner)
        {
            this.Airliner = airliner;
        }
        //get the degree of filling
        private double getFillingDegree()
        {
            double avgPassengers = getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")) / getStatisticsValue(StatisticsTypes.GetStatisticsType("Arrivals"));

            double totalPassengers = Convert.ToDouble(this.Airliner.Airliner.getTotalSeatCapacity());

            double fillingDegree = avgPassengers / totalPassengers;

            if (totalPassengers == 0)
                return 0;
            else
                return avgPassengers / totalPassengers;
        }
          //gets the income per passenger
        private double getIncomePerPassenger()
        {
            double totalPassengers = Convert.ToDouble(getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")));

            if (totalPassengers == 0)
                return 0;
            else
                return getBalance() / totalPassengers;
        }
        //gets the balance
        private double getBalance()
        {
            return getStatisticsValue(StatisticsTypes.GetStatisticsType("Airliner_Income"));
        }
        private AirlinerStatistics(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }


        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }

            base.GetObjectData(info, context);

        }
       
    }
}
