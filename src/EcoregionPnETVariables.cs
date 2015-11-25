﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class EcoregionPnETVariables
    {
        #region private variables
        private DateTime _date;
        private IObservedClimate obs_clim;
        private float _snowfraction;
        private float _vpd;
        private float _precin;
        private float _maxmonthlysnowmelt;
        private float _dayspan;
        private float _tave;
        private float _tday;
        //private float _gsSlope;
        //private float _gsInt;
        private float _newsnow;
        float _daylength;
        float _amax;
        #endregion

        #region public accessors

        public float VPD
        {
            get
            {
                return _vpd;
            }
        }
       
        public float NewSnow
        {
            get
            {
                return _newsnow;
            }
        }
       
        public float SnowFraction
        {
            get
            {
                return _snowfraction;
            }
        }
        
        public float Precin
        {
            get
            {
                return _precin;
            }
        }
        public float Maxmonthlysnowmelt
        {
            get
            {
                return _maxmonthlysnowmelt;
            }
        }
        public byte Month 
        { 
            get 
            { 
                return (byte)_date.Month; 
            } 
        }
        public float Tday {
            get 
            {
                return _tday;
            }
        }
        public float Prec
        {
            get
            {
                return obs_clim.Prec;
            }
        }
        
        public float PAR0 {
            get 
            {
                return obs_clim.PAR0;
            }
        }
        public DateTime Date {
            get {
                return _date;
            }
        }
        public float DaySpan
        {
            get
            {
                return _dayspan;
            }
        }
        public float Year 
        { 
            get 
            { 
                return _date.Year + 1F / 12F * (_date.Month - 1); 
            } 
        }
        public float Tave
        {
            get
            {
                return _tave;
            }
        }
        
        public float Tmin 
        {
            get
            {
                return obs_clim.Tmin;
            }
        }
        public float Daylength
        {
            get
            {
                return _daylength;
            }
        }
        public float Amax
        {
            get
            {
                return _amax;
            }
        }
       

        # endregion

        #region static computation functions
        private static int Calculate_DaySpan(int Month)
        {
            if (Month == 1) return 31;
            else if (Month == 2) return 28;
            else if (Month == 3) return 31;
            else if (Month == 4) return 30;
            else if (Month == 5) return 31;
            else if (Month == 6) return 30;
            else if (Month == 7) return 31;
            else if (Month == 8) return 31;
            else if (Month == 9) return 30;
            else if (Month == 10) return 31;
            else if (Month == 11) return 30;
            else if (Month == 12) return 31;
            else throw new System.Exception("Cannot calculate DaySpan, month = " + Month);
        }

        private static float CumputeSnowFraction(float Tave)
        {
            if (Tave > 2) return 0;
            else if (Tave < -5) return 1;
            else return (Tave - 2) / -7;
        }

        private static float Calculate_VP(float a, float b, float c, float T)
        {
            return a * (float)Math.Exp(b * T / (T + c));
        }

        public static float Calculate_VPD(float Tday, float TMin)
        {

            float emean;
            //float delta;

            //saturated vapor pressure
            float es = Calculate_VP(0.61078f, 17.26939f, 237.3f, Tday);
            // 0.61078f * (float)Math.Exp(17.26939f * Tday / (Tday + 237.3f));

            //delta = 4098.0f * es / ((Tday + 237.3f) * (Tday + 237.3f));
            if (Tday < 0)
            {
                es = Calculate_VP(0.61078f, 21.87456f, 265.5f, Tday);
                //0.61078f * (float)Math.Exp(21.87456f * Tday / (Tday + 265.5f));
                //delta = 5808.0f * es / ((Tday + 265.5f) * (Tday + 265.5f));
            }

            emean = Calculate_VP(0.61078f, 17.26939f, 237.3f, TMin);
            //0.61078f * (float)Math.Exp(17.26939f * TMin / (TMin + 237.3f));
            if (TMin < 0) emean = Calculate_VP(0.61078f, 21.87456f, 265.5f, TMin);
            //0.61078f * (float)Math.Exp(21.87456f * TMin / (TMin + 265.5f));

            return es - emean;
        }

        

        public static float LinearPsnTempResponse(float tday, float PsnTOpt, float PsnTMin)
        {
            if (tday < PsnTMin) return 0;
            else if (tday > PsnTOpt) return 1;

            else return (tday - PsnTMin) / (PsnTOpt - PsnTMin);
        }

        public static float Calculate_NightLength(float hr)
        {
            return 60 * 60 * (24 - hr);
        }

        public static float Calculate_DayLength(float hr)
        {
            return 60 * 60 * hr;
        }

        public static float Calculate_hr(int DOY, double Latitude)
        {
            float TA;
            float AC;
            float LatRad;
            float r;
            float z;
            float decl;
            float z2;
            float h;

            LatRad = (float)Latitude * (2.0f * (float)Math.PI) / 360.0f;
            r = 1 - (0.0167f * (float)Math.Cos(0.0172f * (DOY - 3)));
            z = 0.39785f * (float)Math.Sin(4.868961f + 0.017203f * DOY + 0.033446f * (float)Math.Sin(6.224111f + 0.017202f * DOY));

            if ((float)Math.Abs(z) < 0.7f) decl = (float)Math.Atan(z / ((float)Math.Sqrt(1.0f - z * z)));
            else decl = (float)Math.PI / 2.0f - (float)Math.Atan((float)Math.Sqrt(1 - z * z) / z);

            if ((float)Math.Abs(LatRad) >= (float)Math.PI / 2.0)
            {
                if (Latitude < 0) LatRad = (-1.0f) * ((float)Math.PI / 2.0f - 0.01f);
                else LatRad = 1 * ((float)Math.PI / 2.0f - 0.01f);
            }
            z2 = -(float)Math.Tan(decl) * (float)Math.Tan(LatRad);

            if (z2 >= 1.0) h = 0;
            else if (z2 <= -1.0) h = (float)Math.PI;
            else
            {
                TA = (float)Math.Abs(z2);
                if (TA < 0.7) AC = 1.570796f - (float)Math.Atan(TA / (float)Math.Sqrt(1 - TA * TA));
                else AC = (float)Math.Atan((float)Math.Sqrt(1 - TA * TA) / TA);
                if (z2 < 0) h = 3.141593f - AC;
                else h = AC;
            }
            return 2 * (h * 24) / (2 * (float)Math.PI);
        }

        #endregion

        private Dictionary<string, SpeciesPnETVariables> speciesVariables;

        public SpeciesPnETVariables this[string species]
        {
            get
            {
                return speciesVariables[species];
            }
        }

        public EcoregionPnETVariables(IObservedClimate climate_dataset, DateTime Date, bool Wythers, List<ISpeciesPNET> Species)
        {
            
            this._date = Date;
            this.obs_clim = climate_dataset;

            speciesVariables = new Dictionary<string, SpeciesPnETVariables>();

            
            _tave = (float)0.5 * (climate_dataset.Tmin + climate_dataset.Tmax);

            _dayspan = EcoregionPnETVariables.Calculate_DaySpan(Date.Month);

            _maxmonthlysnowmelt = 0.15f * Math.Max(0, Tave) * DaySpan;

            _snowfraction = CumputeSnowFraction(Tave);

            _precin = (1 - _snowfraction) * climate_dataset.Prec;

            
            _newsnow = _snowfraction * climate_dataset.Prec;//mm
             
            float hr = Calculate_hr(Date.DayOfYear, PlugIn.Latitude);
            _daylength = Calculate_DayLength(hr);
            float nightlength = Calculate_NightLength(hr);

            _tday = (float)0.5 * (climate_dataset.Tmax + _tave);
            _vpd = EcoregionPnETVariables.Calculate_VPD(Tday, climate_dataset.Tmin);


            foreach (ISpeciesPNET spc in Species )
            {
                SpeciesPnETVariables speciespnetvars = GetSpeciesVariables(ref climate_dataset, Wythers, Daylength, nightlength, spc);

                speciesVariables.Add(spc.Name, speciespnetvars);
            }

        }

        private SpeciesPnETVariables GetSpeciesVariables(ref IObservedClimate climate_dataset, bool Wythers, float daylength, float nightlength, ISpeciesPNET spc)
        {
            

            float DVPD = Math.Max(0, 1 - spc.DVPD1 * (float)Math.Pow(VPD, spc.DVPD2));

            float cicaRatio = (-0.075f * spc.FolN) + 0.875f;
            float ci350 = 350 * cicaRatio;
            float Arel350 = 1.22f * ((ci350 - 68) / (ci350 + 136));

            float ciElev = climate_dataset.CO2 * cicaRatio;
            float ArelElev = 1.22f * ((ciElev - 68) / (ciElev + 136));
            float delamax = 1 + ((ArelElev - Arel350) / Arel350);

            // CO2 effect on photosynthesis
            // Calculate CO2 effect on conductance and set slope and intercept for A-gs relationship
            //float Ci = climate_dataset.CO2 * (1 - cicaRatio);
            //float Delgs = delamax / ((Ci / (350.0f - ci350))); // denominator -> CO2 conductance effect
            float Delgs = delamax / ((climate_dataset.CO2 - climate_dataset.CO2 * cicaRatio) / (350.0f - ci350));


            //_gsSlope = (float)((-1.1309 * delamax) + 1.9762);   // used to determine ozone uptake
            //_gsInt = (float)((0.4656 * delamax) - 0.9701);

            float wue = (spc.WUEcnst / VPD) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance

            SpeciesPnETVariables speciespnetvars = new SpeciesPnETVariables();

            speciespnetvars.LeafOn = Tmin > spc.PsnTMin;

            speciespnetvars.WUE_CO2_corr = wue / delamax;

            //speciespnetvars.WUE_CO2_corr = (climate_dataset.CO2 - Ci) / 1.6f;

            //wue[ecoregion, spc, date] = (Parameters.WUEcnst[spc] / vpd[ecoregion, date]) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance
            //float wue = (spc.WUEcnst / VPD) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance
            speciespnetvars.LeafOn = Tmin > spc.PsnTMin;

            // NETPSN 
            _amax = delamax * (spc.AmaxA + spc.AmaxB * spc.FolN);

            //Reference net Psn (lab conditions) in gC/timestep
            float RefNetPsn = _dayspan * (_amax * DVPD * daylength * Constants.MC) / Constants.billion;

            //-------------------FTempPSN (public for output file)
            speciespnetvars.FTempPSN = EcoregionPnETVariables.LinearPsnTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin);

            // PSN (g/tstep)
            speciespnetvars.FTempPSNRefNetPsn =  speciespnetvars.FTempPSN * RefNetPsn;
 
            //EcoregionPnETVariables.RespTempResponse(spc, Tday, climate_dataset.Tmin, daylength, nightlength);
             
            // day respiration factor
            float RespTempDay = CalcQ10Factor(spc.Q10, Tday, spc.PsnTOpt);

            float fTempRespNight = CalcQ10Factor(spc.Q10, Tmin , spc.PsnTOpt);
           
           
           
            // Unitless respiration adjustment: public for output file only
            speciespnetvars.FTempRespWeightedDayAndNight = (float)Math.Min(1.0, (RespTempDay * daylength + fTempRespNight * nightlength) / ((float)daylength + (float)nightlength)); ;

            speciespnetvars.MaintRespFTempResp = spc.MaintResp * speciespnetvars.FTempRespWeightedDayAndNight;

            // Respiration gC/timestep (RespTempResponses[0] = day respiration factor)
             
            if (Wythers == true)
            {
                speciespnetvars.FTempRespDay = (0.14F - 0.002F * Tave) * (3.22F - 0.046F * (float)Math.Pow((0.5F * (Tave + spc.PsnTOpt)), ((Tave - spc.PsnTOpt) / 10)));
            }
            else
            {
                speciespnetvars.FTempRespDay = spc.BFolResp * RespTempDay;
            }

            
            
            return speciespnetvars;
        }

        private float CalcQ10Factor(float Q10, float Tday, float PsnTOpt)
        {
            float RespTempDay = ((float)Math.Pow(Q10, (Tday - PsnTOpt) / 10));
            return RespTempDay;
        }

         
        
        
    }
}
