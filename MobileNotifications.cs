using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;


/*
 *  TODO
 *      - test thoroughly nextMonth
 *      - automatically -> battery not optimise
 */



public class MobileNotifications : MonoBehaviour
{

    /*
        TERMS
        
        - reminder -> 1st message 15min before
        - notifaction -> 2nd message exactly at time 00min (even if opened)
        - alert -> 3rd message 15min after (if unopened)
 
        - time window -> at 00min till 30min after

    */


    static int[] firetimes = new int[] { 8, 11, 14, 17, 20 };
    static string reminderText = "The window opens soon";
    static string notificationText = "Now the window has opened";
    static string alertText = "The window has already been opened";


    static bool showTime = true;                   // show fireTime in Title (eg. 17)
    static string titleBeforeTime = ": ";          // text that is in front of time if showTime==true
    static string titleAfterTime = ":00h";         // text that is after time if showTime==true

    static string reminderTitle = "Reminder";
    static string notificationTitle = "Notification";
    static string alertTitle = "Alert";



    static int timeInterval = 15;      // min between notifications

    static int reminderTime = 60 - timeInterval;      // fire minutes before 00min
    static int alertTime = timeInterval;              // fire minutes after 00min


    static int alertPrepareTime = 2;                  // min before 00min for which alert does not fire anymore (eg. opening app 3min before 00min => alert still fires)









    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
        
    }


    void OnApplicationFocus(bool hasFocus)
    {


        if (hasFocus)
        {
            AndroidNotificationCenter.CancelAllNotifications();

            for (int i = 0; i < firetimes.Length; i++)
            {

                /*
                        CHANNELS
                */


                // Create the Android Reminder Channel
                AndroidNotificationChannel channel_rem = new AndroidNotificationChannel()
                {
                    Id = "reminder" + i,
                    Name = "Reminder Channel",
                    Importance = Importance.High,
                    Description = "Reminder",
                };

                // Create the Android Notification Channel
                AndroidNotificationChannel channel_not = new AndroidNotificationChannel()
                {
                    Id = "notification" + i,
                    Name = "Notification Channel",
                    Importance = Importance.High,
                    Description = "Notification",
                };

                // Create the Android Alert Channel
                AndroidNotificationChannel channel_alert = new AndroidNotificationChannel()
                {
                    Id = "alert" + i,
                    Name = "Alert Channel",
                    Importance = Importance.High,
                    Description = "Alert",
                };


                AndroidNotificationCenter.RegisterNotificationChannel(channel_not);
                AndroidNotificationCenter.RegisterNotificationChannel(channel_rem);
                AndroidNotificationCenter.RegisterNotificationChannel(channel_alert);


                /*
                        TIME CALCULATION
                */



                bool nextYear = System.DateTime.Now.Month == 12 && System.DateTime.Now.Day == 31;
                bool nextMonth = (System.DateTime.Now.Day == 31 && (System.DateTime.Now.Month == 1 || System.DateTime.Now.Month == 3 ||
                    System.DateTime.Now.Month == 5 || System.DateTime.Now.Month == 7 || System.DateTime.Now.Month == 8 || System.DateTime.Now.Month == 10 || System.DateTime.Now.Month == 12)) ||
                    (System.DateTime.Now.Day == 30 && (System.DateTime.Now.Month == 4 || System.DateTime.Now.Month == 6 ||
                    System.DateTime.Now.Month == 9 || System.DateTime.Now.Month == 11)) ||
                    ((System.DateTime.Now.Day == 29 || (System.DateTime.Now.Day == 28 && System.DateTime.Now.Year % 4 != 0)) && System.DateTime.Now.Month == 2);       // LeapYear not 100% accurate as quite unrealistic to wait for 100 years

                    /*

                         NOTIFICATIONS

                    */


                    // 15min before
                    AndroidNotification reminder = new AndroidNotification();
                bool skipReminder = System.DateTime.Now.Hour > firetimes[i] - 1 || (System.DateTime.Now.Hour == firetimes[i] - 1 && System.DateTime.Now.Minute >= reminderTime);
                reminder.Title = showTime ? reminderTitle + titleBeforeTime + firetimes[i] + titleAfterTime : reminderTitle;
                reminder.Text = reminderText;
                reminder.FireTime = new System.DateTime(!(nextYear && skipReminder) ? System.DateTime.Now.Year : System.DateTime.Now.Year + 1,
                    !(nextMonth && skipReminder) ? System.DateTime.Now.Month : (System.DateTime.Now.Month + 1) % 12, !skipReminder ? System.DateTime.Now.Day : (!nextMonth ? System.DateTime.Now.Day + 1 : 1), firetimes[i] - 1, reminderTime, 0);
                reminder.RepeatInterval = new System.TimeSpan(1, 0, 0, 0); // -> Every Day  (if same text, otherwise use different IDs)
                reminder.ShouldAutoCancel = true;


                // opened window
                AndroidNotification notification = new AndroidNotification();
                bool skipNotification = System.DateTime.Now.Hour >= firetimes[i];
                notification.Title = showTime ? notificationTitle + titleBeforeTime + firetimes[i] + titleAfterTime : notificationTitle;
                notification.Text = notificationText;
                notification.FireTime = new System.DateTime(!(nextYear && skipNotification) ? System.DateTime.Now.Year : System.DateTime.Now.Year + 1,
                    !(nextMonth && skipNotification) ? System.DateTime.Now.Month : (System.DateTime.Now.Month + 1) % 12, !skipNotification ? System.DateTime.Now.Day : (!nextMonth ? System.DateTime.Now.Day + 1 : 1), firetimes[i], 0, 0);
                notification.RepeatInterval = new System.TimeSpan(1, 0, 0, 0); // -> Every Day  (if same text, otherwise use different IDs)
                notification.ShouldAutoCancel = true;


                

                // 15min after
                AndroidNotification alert = new AndroidNotification();
                bool skipAlert = System.DateTime.Now.Hour > firetimes[i] || (System.DateTime.Now.Hour == firetimes[i] && System.DateTime.Now.Minute >= alertTime);
                alert.Title = showTime ? alertTitle + titleBeforeTime + firetimes[i] + titleAfterTime : alertTitle;
                alert.Text = alertText;
                alert.FireTime = new System.DateTime(!(nextYear && skipAlert) ? System.DateTime.Now.Year : System.DateTime.Now.Year + 1,
                        !(nextMonth && skipAlert) ? System.DateTime.Now.Month : (System.DateTime.Now.Month + 1) % 12,
                        ((System.DateTime.Now.Minute >= 0 && System.DateTime.Now.Hour == firetimes[i]) || (System.DateTime.Now.Hour == firetimes[i] - 1 && System.DateTime.Now.Minute >= 60 - alertPrepareTime) || skipAlert) ? (!nextMonth ? System.DateTime.Now.Day + 1 : 1) : System.DateTime.Now.Day, firetimes[i], alertTime, 0);
                alert.RepeatInterval = new System.TimeSpan(1, 0, 0, 0); // -> Every Day  (if same text, otherwise use different IDs) 
                alert.ShouldAutoCancel = true;


                // Send Notifications
                var notId = AndroidNotificationCenter.SendNotification(reminder, "reminder" + i);
                var remId = AndroidNotificationCenter.SendNotification(notification, "notification" + i);
                var alertId = AndroidNotificationCenter.SendNotification(alert, "alert" + i);

            }
        }
    }


}
 
