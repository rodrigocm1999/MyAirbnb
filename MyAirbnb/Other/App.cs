using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Other

{
    public class App
    {
        public static int ItemsPerPage { get; } = 5;
        public static string ManagerRole { get; } = "Manager";
        public static string WorkerRole { get; } = "Worker";
        public static string AdminRole { get; } = "Admin";
        public static string ClientRole { get; } = "Client";

        public static string PostImagesFolderName { get; } = "postimages";
        public static string ReservationImagesFolderName { get; } = "reservationimages";

    }
}
