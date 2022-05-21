using System;
using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// Класс для работы со временем
    /// </summary>
    public class Clocks : MonoBehaviour
    {
        /// <summary>
        /// Экземпляр класса времени
        /// </summary>
        public static Clocks Instance { get; private set; }

        /// <summary>
        /// Вызывается кажую секунду
        /// </summary>
        public static event Action OnTick;

        /// <summary>
        /// Во сколько началась текущая сессия
        /// </summary>
        static long _sessionStartAt = 0;

        /// <summary>
        /// Сколько игрок играл вплоть до текущей сессии
        /// </summary>
        static long _totalTimeEx = 0;

        /// <summary>
        /// Текущее игровое время
        /// </summary>
        public static long time { get; private set; }

        /// <summary>
        /// Продолжительность тика
        /// </summary>
        public static float tillTick => 1f + time - (float)(Clocks.DateTimeNow.Ticks - 1f) / 10000000f;

        /// <summary>
        /// Общее время проведённое в игре
        /// </summary>
        public static long totalTimeInGame
        {
            get => timeSienceStartup + _totalTimeEx;
            set
            {
                if (_totalTimeEx == 0L) // Заполняется при первой загрузке профиля. Хранится в профиле игрока
                    _totalTimeEx = value;
            }
        }

        /// <summary>
        /// Время прошедшее с момента начала сессии
        /// </summary>
        public static long timeSienceStartup => time - _sessionStartAt;

        private void Awake()
        {
            Instance = this;
            Update();
            _sessionStartAt = time;
        }

        private void Start()
        {
            Update();
        }

        private void Update()
        {
            long timeNew = DateTimeNow.Ticks / 10000000L;
            if (timeNew == time) return;

            time = timeNew;
            OnTick?.Invoke();
        }

        /// <summary>
        /// Перевод из <see cref="DateTime"/>
        /// </summary>
        public static long ToTime(DateTime _time)
        {
            return _time.Ticks / 10000000L;
        }

        /// <summary>
        /// Перевод из Unix TimeStamp
        /// </summary>
        public static long ToTime(long _unixTimeStamp)
        {
            DateTime dateTime = epochStartTime.AddSeconds(_unixTimeStamp).Add(TimeZoneInfo.Local.BaseUtcOffset);

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.UtcNow))
            {
                dateTime = dateTime.AddHours(1);
            }

            return ToTime(dateTime);
        }

        /// <summary>
        /// Converts seconds timer to formated time
        /// </summary>
        public static string SecToTimeWithoutLetters(float sec)
        {
            TimeSpan t = TimeSpan.FromSeconds(sec);

            if (t.Hours > 0 || t.Days > 0)
            {
                int hours = t.Hours + t.Days * 24;

                if (t.Minutes != 0)
                    return $"{hours}:{t.Minutes}";

                if (hours % 10 == 1 && hours / 10 != 1)
                    return $"{hours}:00";

                if (hours % 10 > 1 && hours % 10 < 5 && hours / 10 != 1)
                    return $"{hours}:00";

                return $"{hours}:00";
            }

            if (t.Minutes >= 10)
                if (t.Seconds >= 10)
                    return $"{t.Minutes}:{t.Seconds}";
                else
                    return $"{t.Minutes}:0{t.Seconds}";

            if (t.Seconds >= 10)
                return $"0{t.Minutes}:{t.Seconds}";

            return $"0{t.Minutes}:0{t.Seconds}";
        }

        /// <summary>
        /// Converts seconds timer to formated time (return min and sec)
        /// </summary>
        public static int[] SecToArray(float sec)
        {
            TimeSpan t = TimeSpan.FromSeconds(sec);
            int[] timeArray = new int[4];

            if (t.Hours > 0)
            {
                timeArray[0] = t.Hours / 10;
                timeArray[1] = t.Hours % 10;
                timeArray[2] = t.Minutes / 10;
                timeArray[3] = t.Minutes % 10;
                return timeArray;
            }

            timeArray[0] = t.Minutes / 10;
            timeArray[1] = t.Minutes % 10;
            timeArray[2] = t.Seconds / 10;
            timeArray[3] = t.Seconds % 10;
            return timeArray;
        }

        public static int GetClosestHour(IEnumerable<int> _hourList, int _hour)
        {
            List<int> hList = new List<int>(_hourList);

            if (hList.Count == 0)
                return 5;

            hList.Sort();

            foreach (int hour in hList)
                if (hour > _hour)
                    return hour;

            return hList[0];
        }

        public static int GetClosestHour(IEnumerable<int> _hourList)
        {
            return GetClosestHour(_hourList, DateTimeNow.Hour);
        }

        /// <summary>
        /// Текущее время
        /// </summary>
        public static DateTime DateTimeNow
        {
            get
            {
                if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.UtcNow))
                    return DateTime.UtcNow.Add(TimeZoneInfo.Local.BaseUtcOffset).AddHours(1);

                return DateTime.UtcNow.Add(TimeZoneInfo.Local.BaseUtcOffset);
            }
        }

        /// <summary>
        /// Начало эпохи Unix
        /// </summary>
        private static readonly DateTime epochStartTime = new DateTime(1970, 1, 1);

        /// <summary>
        /// Текущее время у игрока по UnixTime
        /// </summary>
        public static long UnixTime => (long)DateTime.UtcNow.Subtract(epochStartTime).TotalSeconds;
    }
