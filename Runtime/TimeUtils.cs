namespace UniGame.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UniGame.Utils.Runtime;
    using UnityEngine;
    using UnityEngine.Localization.SmartFormat.Utilities;

    public static class TimeUtils
    {
        private const string TimerFormat4 = "{0} {1}:{2}:{3}";
        private const string TimerFormat3 = "{0}:{1}:{2}";
        private const string TimerFormat2 = "{0}:{1}";
        private const string TimerFormat1 = "{0}";
        
        public const TimeSpanFormatOptions DefaultOptions = TimeSpanFormatOptions.Abbreviate|TimeSpanFormatOptions.LessThanOff|
                                                            TimeSpanFormatOptions.TruncateFill|TimeSpanFormatOptions.RangeSeconds|
                                                            TimeSpanFormatOptions.RangeHours;
        public const TimeSpanFormatOptions DefaultOptionsWithoutSeconds = TimeSpanFormatOptions.Abbreviate|
                                                                          TimeSpanFormatOptions.LessThanOff|TimeSpanFormatOptions.TruncateFill|
                                                                          TimeSpanFormatOptions.RangeMinutes|TimeSpanFormatOptions.RangeHours;
            
        private static Regex _tokenRegex = new(@"\{[^\{\}]+\}");
        private static List<int> _units = new();
        private static List<int> _resultUnits = new();

        private static TimeZoneInfo easterStandardTimezone = 
            TimeZoneInfo.CreateCustomTimeZone(
                "America/NewYork",
                TimeSpan.FromHours(-5), 
                "America/NewYork", 
                "America/NewYork");
        
        public static readonly DateTime StartTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime EstStartTime => TimeZoneInfo.ConvertTimeFromUtc(StartTime, easterStandardTimezone);

        public static long GetUtcNow()
        {
            return (long)(DateTime.UtcNow - StartTime).TotalMilliseconds;
        }

        public static long GetUnixTimeNow()
        {
            return (long)(DateTime.UtcNow - StartTime).TotalSeconds;
        }
        
        public static long UtcToEst(long utcMs)
        {
            var utc = StartTime.AddMilliseconds(utcMs);
            return (long)(TimeZoneInfo.ConvertTimeFromUtc(utc, easterStandardTimezone) - EstStartTime).TotalMilliseconds;
        }

        public static long EstToUtc(long estMs)
        {
            var est = EstStartTime.AddMilliseconds(estMs);
            return (long) (TimeZoneInfo.ConvertTimeToUtc(est, easterStandardTimezone) - StartTime).TotalMilliseconds;
        }

        public static long GetEstNow()
        {
            var utc = GetUtcNow();
            return UtcToEst(utc);
        }

        public static DateTime UtcDateTimeFromMilliseconds(long ms)
        {
            return StartTime.AddMilliseconds(ms);
        }
        
        public static DateTime EstDateTimeFromMilliseconds(long ms)
        {
            return EstStartTime.AddMilliseconds(ms);
        }

        public static string Format(TimeSpan time, TimeSpanFormatOptions options = DefaultOptions)
        {
            var str = time.ToTimeString(options, LocalizationHelper.LocalizedTimeTextInfo);
            return str;
        }

        public static string Format(string format, TimeSpan timeSpan)
        {
            _units.Clear();
            _resultUnits.Clear();
            
            _units.Add(timeSpan.Days);
            _units.Add(timeSpan.Hours);
            _units.Add(timeSpan.Minutes);
            _units.Add(timeSpan.Seconds);

            var tokens = _tokenRegex.Matches(format).Count;
            if (tokens > _units.Count)
            {
                Debug.LogError($"Input string requires more parameters. Format: {format}, submitted parameters count: {_units.Count}");
                return timeSpan.ToString(@"hh\:mm\:ss");
            }

            var firstValIndex = _units.FindIndex(u => u != 0);
            var isEmpty = firstValIndex < 0;
            
            var index = isEmpty ? 0 : Math.Min(firstValIndex, _units.Count - tokens);
            
            for (var i = index; i < _units.Count; i++)
            {
                _resultUnits.Add(_units[i]);
            }

            return tokens switch
            {
                1 => string.Format(format, _resultUnits[0]),
                2 => string.Format(format, _resultUnits[0], _resultUnits[1]),
                3 => string.Format(format, _resultUnits[0], _resultUnits[1], _resultUnits[2]),
                4 => string.Format(format, _resultUnits[0], _resultUnits[1], _resultUnits[2], _resultUnits[3]),
                _ => format
            };
        }
        
    }
}
