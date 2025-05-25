using UnityEngine;
using System;

public class StarPositionCalculator
{
    // 천문 상수
    private const double J2000 = 2451545.0;  // J2000.0 기준 율리우스 날짜
    private const double DEG_TO_RAD = Math.PI / 180.0;
    private const double RAD_TO_DEG = 180.0 / Math.PI;

    // 대기 굴절 보정 (Saemundsson 공식)
    public static double CalculateRefraction(double altitude)
    {
        if (altitude < 0) return 0;
        
        double alt = altitude * DEG_TO_RAD;
        double refraction = 0.1594 + 0.0196 * altitude + 0.00002 * altitude * altitude;
        refraction /= (1 + 0.505 * altitude + 0.0845 * altitude * altitude);
        
        return refraction;
    }

    // 적도좌표계(RA/Dec)를 지평좌표계(Alt/Az)로 변환
    public static (double altitude, double azimuth) EquatorialToHorizontal(
        double ra, double dec, 
        double latitude, double longitude, 
        double julianDate)
    {
        // 그리니치 항성시(GST) 계산
        double t = (julianDate - J2000) / 36525.0;
        double gst = 280.46061837 + 360.98564736629 * (julianDate - J2000) +
                    0.000387933 * t * t - t * t * t / 38710000.0;

        // 관측자 항성시(LST) 계산
        double lst = gst + longitude;

        // 시각각 계산
        double hourAngle = lst - ra;
        hourAngle = (hourAngle + 360.0) % 360.0;

        // 좌표 변환
        double ha = hourAngle * DEG_TO_RAD;
        double dec_rad = dec * DEG_TO_RAD;
        double lat_rad = latitude * DEG_TO_RAD;

        double sin_alt = Math.Sin(dec_rad) * Math.Sin(lat_rad) +
                        Math.Cos(dec_rad) * Math.Cos(lat_rad) * Math.Cos(ha);
        double altitude = Math.Asin(sin_alt) * RAD_TO_DEG;

        double cos_az = (Math.Sin(dec_rad) - Math.Sin(altitude * DEG_TO_RAD) * Math.Sin(lat_rad)) /
                       (Math.Cos(altitude * DEG_TO_RAD) * Math.Cos(lat_rad));
        double sin_az = -Math.Sin(ha) * Math.Cos(dec_rad) /
                       Math.Cos(altitude * DEG_TO_RAD);

        double azimuth = Math.Atan2(sin_az, cos_az) * RAD_TO_DEG;
        azimuth = (azimuth + 360.0) % 360.0;

        // 대기 굴절 보정
        altitude += CalculateRefraction(altitude);

        return (altitude, azimuth);
    }

    // 특정 시각/위치에서의 별 위치 검증
    public static (double altitude, double azimuth) VerifyStarPosition(
        double ra, double dec,
        DateTime dateTime,
        double latitude, double longitude,
        double timeZone)
    {
        // 율리우스 날짜 계산
        double julianDate = CalculateJulianDate(dateTime, timeZone);

        // 좌표 변환
        return EquatorialToHorizontal(ra, dec, latitude, longitude, julianDate);
    }

    // 율리우스 날짜 계산 (Meeus 알고리즘)
    private static double CalculateJulianDate(DateTime dateTime, double timeZone)
    {
        int y = dateTime.Year;
        int m = dateTime.Month;
        double d = dateTime.Day + 
                  (dateTime.Hour + dateTime.Minute/60.0 + dateTime.Second/3600.0)/24.0;

        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }

        int a = (int)(y/100);
        int b = 2 - a + (int)(a/4);

        double jd = (int)(365.25 * (y + 4716)) + 
                   (int)(30.6001 * (m + 1)) + 
                   d + b - 1524.5;

        // 시간대 보정
        jd -= timeZone/24.0;

        return jd;
    }
} 